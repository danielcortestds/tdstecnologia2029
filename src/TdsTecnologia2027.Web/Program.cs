using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TdsTecnologia2027.Web.Data;

var builder = WebApplication.CreateBuilder(args);

// Conexão com MySQL via Pomelo.
// Detecta automaticamente o ambiente: dev local (appsettings) ou Azure "MySQL no Aplicativo".
var connectionString = ConnectionStringResolver.Resolve(builder.Configuration);

// Log do destino da conexão (com a senha mascarada) — aparece no Log Stream do Azure
// e ajuda a diagnosticar problemas de conexão sem expor a senha.
Console.WriteLine($"[TDS] Connection string resolvida: {ConnectionStringResolver.Mask(connectionString)}");

// IMPORTANTE: NÃO usar ServerVersion.AutoDetect aqui — ele abre uma conexão de forma
// imediata durante a configuração do DI e, se o MySQL ainda não estiver pronto, derruba
// a aplicação inteira (HTTP 500.30). Usamos uma versão fixa (MySQL no Aplicativo = 5.7)
// e adiamos a conexão real para o passo de Migrate, onde há retry.
var serverVersion = new MySqlServerVersion(new Version(5, 7, 42));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, serverVersion));

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

// MVC com suporte a Areas
var mvcBuilder = builder.Services.AddControllersWithViews();
var razorPagesBuilder = builder.Services.AddRazorPages();

// Em desenvolvimento, recompila as views Razor on-the-fly (sem precisar rebuild).
if (builder.Environment.IsDevelopment())
{
    mvcBuilder.AddRazorRuntimeCompilation();
    razorPagesBuilder.AddRazorRuntimeCompilation();
}

var app = builder.Build();

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

// Rota da área Admin
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
