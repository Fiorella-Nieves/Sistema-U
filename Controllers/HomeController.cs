using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema_U.Data;
using Sistema_U.Models;

namespace PortalAcademico.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Mostrar cursos destacados en la página principal
            var cursosDestacados = await _context.Cursos
                .Where(c => c.Activo)
                .OrderByDescending(c => c.Id)
                .Take(3)
                .ToListAsync();

            return View(cursosDestacados);
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}