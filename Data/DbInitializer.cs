using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PortalAcademico.Models;

namespace PortalAcademico.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(ApplicationDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Crear roles
            string[] roleNames = { "Coordinador", "Estudiante" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Crear usuario coordinador
            var coordinadorEmail = "coordinador@universidad.edu";
            if (await userManager.FindByEmailAsync(coordinadorEmail) == null)
            {
                var coordinador = new IdentityUser
                {
                    UserName = coordinadorEmail,
                    Email = coordinadorEmail
                };
                
                var result = await userManager.CreateAsync(coordinador, "Password123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(coordinador, "Coordinador");
                }
            }

            // Crear cursos iniciales
            if (!context.Cursos.Any())
            {
                var cursos = new[]
                {
                    new Curso 
                    { 
                        Codigo = "MAT101", 
                        Nombre = "Matemáticas Básicas", 
                        Creditos = 4, 
                        CupoMaximo = 30,
                        HorarioInicio = new TimeSpan(8, 0, 0),
                        HorarioFin = new TimeSpan(10, 0, 0),
                        Activo = true
                    },
                    new Curso 
                    { 
                        Codigo = "PROG201", 
                        Nombre = "Programación I", 
                        Creditos = 5, 
                        CupoMaximo = 25,
                        HorarioInicio = new TimeSpan(10, 0, 0),
                        HorarioFin = new TimeSpan(12, 0, 0),
                        Activo = true
                    },
                    new Curso 
                    { 
                        Codigo = "FIS301", 
                        Nombre = "Física General", 
                        Creditos = 4, 
                        CupoMaximo = 20,
                        HorarioInicio = new TimeSpan(14, 0, 0),
                        HorarioFin = new TimeSpan(16, 0, 0),
                        Activo = true
                    }
                };

                context.Cursos.AddRange(cursos);
                await context.SaveChangesAsync();
            }
        }
    }
}