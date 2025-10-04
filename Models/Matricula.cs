using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace PortalAcademico.Models
{
    public enum EstadoMatricula
    {
        Pendiente,
        Confirmada,
        Cancelada
    }

    public class Matricula
    {
        public int Id { get; set; }
        
        public int CursoId { get; set; }
        public Curso Curso { get; set; } = null!;
        
        public string UsuarioId { get; set; } = string.Empty;
        public IdentityUser Usuario { get; set; } = null!;
        
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
        
        public EstadoMatricula Estado { get; set; } = EstadoMatricula.Pendiente;
    }
}