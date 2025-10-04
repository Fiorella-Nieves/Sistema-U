using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Sistema_U.Data;
using Sistema_U.Models;
using Sistema_U.Services;

namespace PortalAcademico.Controllers
{
    [Authorize(Roles = "Coordinador")]
    public class CoordinadorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICursoService _cursoService;

        public CoordinadorController(ApplicationDbContext context, ICursoService cursoService)
        {
            _context = context;
            _cursoService = cursoService;
        }

        public IActionResult Index()
        {
            return View();
        }

        // CRUD de Cursos
        public async Task<IActionResult> Cursos()
        {
            var cursos = await _context.Cursos.ToListAsync();
            return View(cursos);
        }

        public IActionResult CreateCurso()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCurso(Curso curso)
        {
            if (ModelState.IsValid)
            {
                // Validación personalizada
                if (curso.HorarioInicio >= curso.HorarioFin)
                {
                    ModelState.AddModelError("HorarioInicio", "El horario de inicio debe ser anterior al horario de fin");
                    return View(curso);
                }

                _context.Add(curso);
                await _context.SaveChangesAsync();
                
                // Invalidar cache
                await _cursoService.InvalidarCacheAsync();
                
                TempData["Success"] = "Curso creado exitosamente";
                return RedirectToAction(nameof(Cursos));
            }
            return View(curso);
        }

        public async Task<IActionResult> EditCurso(int? id)
        {
            if (id == null) return NotFound();

            var curso = await _context.Cursos.FindAsync(id);
            if (curso == null) return NotFound();
            
            return View(curso);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCurso(int id, Curso curso)
        {
            if (id != curso.Id) return NotFound();

            if (ModelState.IsValid)
            {
                if (curso.HorarioInicio >= curso.HorarioFin)
                {
                    ModelState.AddModelError("HorarioInicio", "El horario de inicio debe ser anterior al horario de fin");
                    return View(curso);
                }

                try
                {
                    _context.Update(curso);
                    await _context.SaveChangesAsync();
                    
                    // Invalidar cache
                    await _cursoService.InvalidarCacheAsync();
                    
                    TempData["Success"] = "Curso actualizado exitosamente";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CursoExists(curso.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Cursos));
            }
            return View(curso);
        }

        public async Task<IActionResult> ToggleCurso(int id)
        {
            var curso = await _context.Cursos.FindAsync(id);
            if (curso == null) return NotFound();

            curso.Activo = !curso.Activo;
            await _context.SaveChangesAsync();
            
            // Invalidar cache
            await _cursoService.InvalidarCacheAsync();

            TempData["Success"] = $"Curso {(curso.Activo ? "activado" : "desactivado")} exitosamente";
            return RedirectToAction(nameof(Cursos));
        }

        // Gestión de matrículas
        public async Task<IActionResult> Matriculas(int? cursoId)
        {
            var matriculasQuery = _context.Matriculas
                .Include(m => m.Curso)
                .Include(m => m.Usuario)
                .AsQueryable();

            if (cursoId.HasValue)
            {
                matriculasQuery = matriculasQuery.Where(m => m.CursoId == cursoId.Value);
            }

            var matriculas = await matriculasQuery.ToListAsync();
            ViewBag.Cursos = await _context.Cursos.ToListAsync();
            ViewBag.CursoSeleccionado = cursoId;

            return View(matriculas);
        }

        [HttpPost]
        public async Task<IActionResult> CambiarEstadoMatricula(int matriculaId, EstadoMatricula nuevoEstado)
        {
            var matricula = await _context.Matriculas
                .Include(m => m.Curso)
                .FirstOrDefaultAsync(m => m.Id == matriculaId);

            if (matricula == null)
            {
                return NotFound();
            }

            matricula.Estado = nuevoEstado;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Matrícula actualizada a: {nuevoEstado}";
            return RedirectToAction(nameof(Matriculas), new { cursoId = matricula.CursoId });
        }

        private bool CursoExists(int id)
        {
            return _context.Cursos.Any(e => e.Id == id);
        }
    }
}