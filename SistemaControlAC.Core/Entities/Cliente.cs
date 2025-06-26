using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaControlAC.Core.Entities
{

 public class Cliente
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string? Ciudad { get; set; }
        public string? CodigoPostal { get; set; }
        public bool RecibeNotificacionesEmail { get; set; } = true;
        public bool RecibeNotificacionesWhatsApp { get; set; } = true;
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
        public bool Activo { get; set; } = true;
        public string Notas { get; set; } = string.Empty; // Notas adicionales sobre el cliente

        // Relaciones
        public virtual ICollection<EquipoAireAcondicionado>? Equipos { get; set; }
        public virtual ICollection<Cita>? Citas { get; set; }
    }
}
