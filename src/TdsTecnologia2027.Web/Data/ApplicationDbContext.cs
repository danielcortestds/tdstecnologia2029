using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TdsTecnologia2027.Web.Models;

namespace TdsTecnologia2027.Web.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Curso> Cursos { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Limita tamanho de colunas com índice do Identity para caber no
        // limite de chave do MySQL InnoDB. Colunas usadas como chave de PK/FK/Index
        // recebem charset ASCII (1 byte/char) — provider keys são sempre ASCII puros.
        const string asciiVarchar = "varchar(128)";
        const string asciiCharset = "ascii";

        builder.Entity<IdentityUser>(b =>
        {
            b.Property(u => u.Id).HasColumnType(asciiVarchar).HasCharSet(asciiCharset);
            b.Property(u => u.UserName).HasMaxLength(128);
            b.Property(u => u.NormalizedUserName).HasMaxLength(128);
            b.Property(u => u.Email).HasMaxLength(128);
            b.Property(u => u.NormalizedEmail).HasMaxLength(128);
        });

        builder.Entity<IdentityRole>(b =>
        {
            b.Property(r => r.Id).HasColumnType(asciiVarchar).HasCharSet(asciiCharset);
            b.Property(r => r.Name).HasMaxLength(128);
            b.Property(r => r.NormalizedName).HasMaxLength(128);
        });

        builder.Entity<IdentityUserLogin<string>>(b =>
        {
            b.Property(l => l.LoginProvider).HasColumnType(asciiVarchar).HasCharSet(asciiCharset);
            b.Property(l => l.ProviderKey).HasColumnType(asciiVarchar).HasCharSet(asciiCharset);
            b.Property(l => l.UserId).HasColumnType(asciiVarchar).HasCharSet(asciiCharset);
        });

        builder.Entity<IdentityUserToken<string>>(b =>
        {
            b.Property(t => t.UserId).HasColumnType(asciiVarchar).HasCharSet(asciiCharset);
            b.Property(t => t.LoginProvider).HasColumnType(asciiVarchar).HasCharSet(asciiCharset);
            b.Property(t => t.Name).HasColumnType(asciiVarchar).HasCharSet(asciiCharset);
        });

        builder.Entity<IdentityUserRole<string>>(b =>
        {
            b.Property(ur => ur.UserId).HasColumnType(asciiVarchar).HasCharSet(asciiCharset);
            b.Property(ur => ur.RoleId).HasColumnType(asciiVarchar).HasCharSet(asciiCharset);
        });

        builder.Entity<IdentityUserClaim<string>>(b =>
        {
            b.Property(uc => uc.UserId).HasColumnType(asciiVarchar).HasCharSet(asciiCharset);
        });

        builder.Entity<IdentityRoleClaim<string>>(b =>
        {
            b.Property(rc => rc.RoleId).HasColumnType(asciiVarchar).HasCharSet(asciiCharset);
        });

        builder.Entity<Curso>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Titulo).HasMaxLength(150).IsRequired();
            entity.Property(e => e.Slug).HasMaxLength(80).IsRequired();
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.Property(e => e.DescricaoCurta).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Descricao).HasMaxLength(2000).IsRequired();
            entity.Property(e => e.Categoria).HasMaxLength(100);
            entity.Property(e => e.Icone).HasMaxLength(80);
            entity.Property(e => e.Preco).HasPrecision(10, 2);
            entity.Property(e => e.ImagemUrl).HasMaxLength(300);
            entity.Property(e => e.PillSecundaria).HasMaxLength(60);
            entity.Property(e => e.IconePillSecundaria).HasMaxLength(80);
            entity.Property(e => e.Topicos).HasMaxLength(2000);
            entity.Property(e => e.WhatsAppMensagem).HasMaxLength(300);
            entity.Property(e => e.CriadoEm).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }
}
