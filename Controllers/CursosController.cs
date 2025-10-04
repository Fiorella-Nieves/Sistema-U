using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalAcademico.Data;
using PortalAcademico.Models;

namespace PortalAcademico.Controllers
{
    public class CursosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CursosController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string nombre, int? creditosMin, int? creditosMax, string horario)
        {
            var cursosQuery = _context.Cursos.Where(c => c.Activo);

            // Filtros
            if (!string.IsNullOrEmpty(nombre))
            {
                cursosQuery = cursosQuery.Where(c => c.Nombre.Contains(nombre));
            }

            if (creditosMin.HasValue)
            {
                cursosQuery = cursosQuery.Where(c => c.Creditos >= creditosMin.Value);
            }

            if (creditosMax.HasValue)
            {
                cursosQuery = cursosQuery.Where(c => c.Creditos <= creditosMax.Value);
            }

            if (!string.IsNullOrEmpty(horario))
            {
                // Filtro simple por horario (mañana/tarde)
                if (horario == "manana")
                {
                    cursosQuery = cursosQuery.Where(c => c.HorarioInicio < TimeSpan.FromHours(12));
                }
                else if (horario == "tarde")
                {
                    cursosQuery = cursosQuery.Where(c => c.HorarioInicio >= TimeSpan.FromHours(12));
                }
            }

            var cursos = await cursosQuery.ToListAsync();
            return View(cursos);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var curso = await _context.Cursos
                .FirstOrDefaultAsync(m => m.Id == id && m.Activo);
                
            if (curso == null)
            {
                return NotFound();
            }

            return View(curso);
        }
    }
}