using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Sistema_U.Models;

namespace Sistema_U.Data 
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Curso> Cursos { get; set; }
        public DbSet<Matricula> Matriculas { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configuraciones
            builder.Entity<Curso>(entity =>
            {
                entity.HasIndex(c => c.Codigo).IsUnique();
                entity.Property(c => c.Codigo).IsRequired().HasMaxLength(20);
                entity.Property(c => c.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(c => c.Creditos).IsRequired();
                entity.Property(c => c.CupoMaximo).IsRequired();
                entity.Property(c => c.HorarioInicio).IsRequired();
                entity.Property(c => c.HorarioFin).IsRequired();
                entity.Property(c => c.Activo).IsRequired();
            });

            // Configurar Matricula
            builder.Entity<Matricula>(entity =>
            {
                entity.HasIndex(m => new { m.CursoId, m.UsuarioId }).IsUnique();
                
                entity.HasOne(m => m.Curso)
                      .WithMany(c => c.Matriculas)
                      .HasForeignKey(m => m.CursoId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(m => m.Usuario)
                      .WithMany()
                      .HasForeignKey(m => m.UsuarioId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Validación: HorarioInicio < HorarioFin
            builder.Entity<Curso>()
                .HasCheckConstraint("CK_Curso_Horario", "[HorarioInicio] < [HorarioFin]");

            // Validación: Créditos > 0
            builder.Entity<Curso>()
                .HasCheckConstraint("CK_Curso_Creditos", "[Creditos] > 0");
        }
    }
}