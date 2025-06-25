using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaControlAC.Core.Entities
{
    public class Usuario
    {
        public int Id { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty; // "Administrador", "Técnico", "Recepcionista"
        public bool Activo { get; set; } = true;
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime? UltimoAcceso { get; set; }

        // Relaciones
        public virtual ICollection<Cita>? CitasAsignadas { get; set; }
        public virtual ICollection<NotaReparacion>? NotasReparacion { get; set; }
    }
}
