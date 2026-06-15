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
    public DbSet<ConfiguracaoEmail> ConfiguracoesEmail { get; set; }

    // LMS / Área do Aluno (cursos online)
    public DbSet<CursoOnline> CursosOnline { get; set; }
    public DbSet<ModuloCurso> Modulos { get; set; }
    public DbSet<Licao> Licoes { get; set; }
    public DbSet<Aluno> Alunos { get; set; }
    public DbSet<MatriculaCurso> Matriculas { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // O SQLite não tem os limites de tamanho de chave do MySQL InnoDB,
        // então não é preciso configurar charset/varchar das tabelas do Identity.
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

        // ── LMS / Área do Aluno ───────────────────────────────────────────
        builder.Entity<CursoOnline>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Slug).HasMaxLength(80).IsRequired();
            entity.HasIndex(e => e.Slug).IsUnique();
        });

        builder.Entity<ModuloCurso>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(m => m.CursoOnline)
                  .WithMany(c => c.Modulos)
                  .HasForeignKey(m => m.CursoOnlineId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Licao>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(l => l.Modulo)
                  .WithMany(m => m.Licoes)
                  .HasForeignKey(l => l.ModuloId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Aluno>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId).IsUnique();
        });

        builder.Entity<MatriculaCurso>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.AlunoId, e.CursoOnlineId }).IsUnique();
            entity.HasOne(m => m.Aluno)
                  .WithMany(a => a.Matriculas)
                  .HasForeignKey(m => m.AlunoId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(m => m.CursoOnline)
                  .WithMany()
                  .HasForeignKey(m => m.CursoOnlineId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
