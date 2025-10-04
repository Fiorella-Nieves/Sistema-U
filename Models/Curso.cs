using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PortalAcademico.Models
{
    public class Curso
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(20)]
        public string Codigo { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;
        
        [Range(1, 10)]
        public int Creditos { get; set; }
        
        [Range(1, 100)]
        public int CupoMaximo { get; set; }
        
        [DataType(DataType.Time)]
        public TimeSpan HorarioInicio { get; set; }
        
        [DataType(DataType.Time)]
        public TimeSpan HorarioFin { get; set; }
        
        public bool Activo { get; set; } = true;
        
        // Navigation properties
        public ICollection<Matricula> Matriculas { get; set; } = new List<Matricula>();
    }
}