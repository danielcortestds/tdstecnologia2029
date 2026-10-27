using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TdsTecnologia2027.Web.Data;

var builder = WebApplication.CreateBuilder(args);

// Banco de dados SQLite (arquivo local, gratuito e sem servidor separado).
// O caminho é resolvido conforme o ambiente: pasta persistente no Azure
// (%HOME%\data) ou App_Data localmente.
var connectionString = ConnectionStringResolver.Resolve(builder.Environment);
Console.WriteLine($"[TDS] SQLite: {connectionString}");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

// Serviço de envio de e-mail via Microsoft Graph (Office 365).
// Configuração fica na tabela ConfiguracaoEmail, editável em Admin → Configurações.
// Scoped porque depende do ApplicationDbContext.
builder.Services.AddScoped<TdsTecnologia2027.Web.Services.IEmailSender,
    TdsTecnologia2027.Web.Services.EmailSender>();

// ASP.NET Core Identity
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// Configurar cookie de autenticação
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

// Política de acesso "somente Master".
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SomenteMaster", p => p.RequireRole("Master"));
});

// MVC com suporte a Areas
var mvcBuilder = builder.Services.AddControllersWithViews();
var razorPagesBuilder = builder.Services.AddRazorPages(options =>
{
    // Desabilita o auto-cadastro público: a página de registro padrão do Identity
    // só pode ser acessada por um Master. A criação de usuários é feita em
    // Admin → Usuários.
    options.Conventions.AuthorizeAreaPage("Identity", "/Account/Register", "SomenteMaster");
    options.Conventions.AuthorizeAreaPage("Identity", "/Account/RegisterConfirmation", "SomenteMaster");
});

// Em desenvolvimento, recompila as views Razor on-the-fly (sem precisar rebuild).
if (builder.Environment.IsDevelopment())
{
    mvcBuilder.AddRazorRuntimeCompilation();
    razorPagesBuilder.AddRazorRuntimeCompilation();
}

var app = builder.Build();

// ── Importação de dados (migração) ────────────────────────────────────────
// Se existir um arquivo "<banco>.incoming" na pasta de dados, ele substitui o
// banco atual ANTES de qualquer conexão ser aberta. A troca acontece aqui, no
// boot, quando nada está travando o arquivo — forma segura de importar os dados
// reais enviados pela tela Admin → Importar. Enviado uma vez e removido depois.
try
{
    var dataDir = ConnectionStringResolver.DataDir(builder.Environment);
    var dbPath = Path.Combine(dataDir, ConnectionStringResolver.DbFileName);
    var incoming = dbPath + ".incoming";
    if (File.Exists(incoming))
    {
        Console.WriteLine("[TDS] Importação: arquivo .incoming encontrado, substituindo o banco...");
        foreach (var sufixo in new[] { "", "-wal", "-shm" })
        {
            var f = dbPath + sufixo;
            if (File.Exists(f)) File.Delete(f);
        }
        File.Move(incoming, dbPath);
        Console.WriteLine("[TDS] Importação: banco substituído com sucesso pelos dados enviados.");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"[TDS] Importação: falha ao aplicar o arquivo .incoming: {ex.Message}");
}

// Aplicar migrations e seed na inicialização.
// O MySQL no Aplicativo pode demorar alguns segundos para ficar disponível após o boot
// do App Service, então tentamos várias vezes antes de desistir. Mesmo que falhe, a
// aplicação continua subindo (o site carrega) para que possamos ler os logs.
using (var scope = app.Services.CreateScope())
{
    const int maxTentativas = 10;
    for (var tentativa = 1; tentativa <= maxTentativas; tentativa++)
    {
        try
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            Console.WriteLine($"[TDS] Tentativa {tentativa}/{maxTentativas}: aplicando migrations...");
            db.Database.Migrate();
            await SeedData.InitializeAsync(scope.ServiceProvider, builder.Configuration);
            Console.WriteLine("[TDS] Migrations e seed concluídos com sucesso.");
            break;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TDS] Falha na tentativa {tentativa}/{maxTentativas}: {ex.Message}");
            if (tentativa == maxTentativas)
            {
                Console.WriteLine("[TDS] ERRO: não foi possível conectar ao MySQL após várias tentativas. " +
                                  "A aplicação vai subir mesmo assim para permitir leitura dos logs.");
                break;
            }
            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Área do Aluno: /Aluno cai em Home/Index (dashboard "Meus Cursos").
app.MapAreaControllerRoute(
    name: "alunoArea",
    areaName: "Aluno",
    pattern: "Aluno/{controller=Home}/{action=Index}/{id?}");

// Rota da área Admin (e demais áreas)
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Cursos}/{action=Index}/{id?}");

// Rota padrão
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Razor Pages para Identity
app.MapRazorPages();

app.Run();
