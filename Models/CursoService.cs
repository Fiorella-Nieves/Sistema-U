using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Sistema_U.Data;
using Sistema_U.Models;
using System.Text.Json;

namespace Sistema_U.Services
{
    public interface ICursoService
    {
        Task<List<Curso>> GetCursosActivosAsync();
        Task InvalidarCacheAsync();
    }

    public class CursoService : ICursoService
    {
        private readonly ApplicationDbContext _context;
        private readonly IDistributedCache _cache;
        private readonly string CACHE_KEY = "cursos_activos";

        public CursoService(ApplicationDbContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<List<Curso>> GetCursosActivosAsync()
        {
            var cachedCursos = await _cache.GetStringAsync(CACHE_KEY);
            
            if (!string.IsNullOrEmpty(cachedCursos))
            {
                return JsonSerializer.Deserialize<List<Curso>>(cachedCursos)!;
            }

            var cursos = await _context.Cursos
                .Where(c => c.Activo)
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            // Cachear por 60 segundos
            var options = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(60));
                
            await _cache.SetStringAsync(CACHE_KEY, JsonSerializer.Serialize(cursos), options);

            return cursos;
        }

        public async Task InvalidarCacheAsync()
        {
            await _cache.RemoveAsync(CACHE_KEY);
        }
    }
}