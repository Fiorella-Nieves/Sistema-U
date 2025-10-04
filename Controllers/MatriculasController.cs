using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Sistema_U.Data;
using Sistema_U.Models;

namespace Sistema_U.Controllers
{
    public class MatriculasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public MatriculasController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int cursoId)
        {
            if (!(User?.Identity?.IsAuthenticated?? false))
            {
                TempData["Error"] = "Debe iniciar sesión para inscribirse en un curso.";
                return RedirectToAction("Details", "Cursos", new { id = cursoId });
            }

            var curso = await _context.Cursos
                .Include(c => c.Matriculas)
                .FirstOrDefaultAsync(c => c.Id == cursoId && c.Activo);

            if (curso == null)
            {
                TempData["Error"] = "Curso no encontrado.";
                return RedirectToAction("Index", "Cursos");
            }

            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null)
            {
                TempData["Error"] = "Usuario no encontrado.";
                return RedirectToAction("Details", "Cursos", new { id = cursoId });
            }

            // Validación 1: No exceder cupo máximo
            var matriculasConfirmadas = curso.Matriculas.Count(m => m.Estado == EstadoMatricula.Confirmada);
            if (matriculasConfirmadas >= curso.CupoMaximo)
            {
                TempData["Error"] = "El curso ha alcanzado su cupo máximo.";
                return RedirectToAction("Details", "Cursos", new { id = cursoId });
            }

            // Validación 2: No estar ya matriculado
            var matriculaExistente = await _context.Matriculas
                .FirstOrDefaultAsync(m => m.CursoId == cursoId && m.UsuarioId == usuario.Id);
            
            if (matriculaExistente != null)
            {
                TempData["Error"] = "Ya está matriculado en este curso.";
                return RedirectToAction("Details", "Cursos", new { id = cursoId });
            }

            // Validación 3: No tener solapamiento de horarios
            var matriculasUsuario = await _context.Matriculas
                .Include(m => m.Curso)
                .Where(m => m.UsuarioId == usuario.Id && 
                           m.Estado != EstadoMatricula.Cancelada &&
                           m.Curso.Activo)
                .ToListAsync();

            foreach (var matricula in matriculasUsuario)
            {
                if (HorariosSolapados(curso, matricula.Curso))
                {
                    TempData["Error"] = $"El horario de este curso se solapa con: {matricula.Curso.Nombre}";
                    return RedirectToAction("Details", "Cursos", new { id = cursoId });
                }
            }

            // Crear matrícula
            var nuevaMatricula = new Matricula
            {
                CursoId = cursoId,
                UsuarioId = usuario.Id,
                FechaRegistro = DateTime.Now,
                Estado = EstadoMatricula.Pendiente
            };

            _context.Matriculas.Add(nuevaMatricula);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Matrícula realizada exitosamente. Estado: Pendiente";
            return RedirectToAction("Details", "Cursos", new { id = cursoId });
        }

        private bool HorariosSolapados(Curso curso1, Curso curso2)
        {
            return curso1.HorarioInicio < curso2.HorarioFin && 
                   curso2.HorarioInicio < curso1.HorarioFin;
        }
    }
}