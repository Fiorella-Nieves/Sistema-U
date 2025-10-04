using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema_U.Data;
using Sistema_U.Models;
using Sistema_U.Services;

namespace Sistema_U.Controllers
{
    public class CursosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICursoService _cursoService;

        public CursosController(ApplicationDbContext context, ICursoService cursoService)
        {
            _context = context;
            _cursoService = cursoService;
        }

        public async Task<IActionResult> Index(string nombre, int? creditosMin, int? creditosMax, string horario)
        {
            var cursos = await _cursoService.GetCursosActivosAsync();
            
            // Aplicar filtros en memoria (para cache)
            if (!string.IsNullOrEmpty(nombre))
            {
                cursos = cursos.Where(c => c.Nombre.Contains(nombre, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (creditosMin.HasValue)
            {
                cursos = cursos.Where(c => c.Creditos >= creditosMin.Value).ToList();
            }

            if (creditosMax.HasValue)
            {
                cursos = cursos.Where(c => c.Creditos <= creditosMax.Value).ToList();
            }

            if (!string.IsNullOrEmpty(horario))
            {
                if (horario == "mañana")
                {
                    cursos = cursos.Where(c => c.HorarioInicio < TimeSpan.FromHours(12)).ToList();
                }
                else if (horario == "tarde")
                {
                    cursos = cursos.Where(c => c.HorarioInicio >= TimeSpan.FromHours(12)).ToList();
                }
            }

            return View(cursos);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Usar el servicio con cache para obtener cursos activos
            var cursos = await _cursoService.GetCursosActivosAsync();
            var curso = cursos.FirstOrDefault(c => c.Id == id);
                
            if (curso == null)
            {
                return NotFound();
            }

            return View(curso);
        }

        // Método para crear matrícula (si lo necesitas aquí también)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Inscribirse(int cursoId)
        {
            if ((!User?.Identity?.IsAuthenticated?? false))
            {
                TempData["Error"] = "Debe iniciar sesión para inscribirse en un curso.";
                return RedirectToAction("Details", new { id = cursoId });
            }

            // Lógica de inscripción aquí...
            // O puedes redirigir al controlador de Matriculas
            return RedirectToAction("Create", "Matriculas", new { cursoId = cursoId });
        }
    }
}