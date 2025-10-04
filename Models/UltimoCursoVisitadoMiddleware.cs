namespace Sistema_U.Middlewares
{
    public class UltimoCursoVisitadoMiddleware
    {
        private readonly RequestDelegate _next;

        public UltimoCursoVisitadoMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Solo procesar para rutas de cursos/details
            if (context.Request.Path.StartsWithSegments("/Cursos/Details") && 
                context.Request.Method == "GET")
            {
                var cursoId = context.Request.RouteValues["id"]?.ToString();
                if (!string.IsNullOrEmpty(cursoId) && int.TryParse(cursoId, out int id))
                {
                    context.Session.SetString("UltimoCursoVisitadoId", cursoId);
                    
                    // También podemos guardar el nombre si lo tenemos disponible
                    // Esto requeriría una consulta a la base de datos o pasar el nombre por ViewData
                }
            }

            await _next(context);
        }
    }
}