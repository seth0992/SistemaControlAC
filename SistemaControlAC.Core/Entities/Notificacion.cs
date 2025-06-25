using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaControlAC.Core.Entities
{
    public class Notificacion
    {
        public int Id { get; set; }
        public int? ClienteId { get; set; }
        public int? UsuarioId { get; set; }
        public int? CitaId { get; set; }
        public string TipoNotificacion { get; set; } = string.Empty; // "Email", "WhatsApp", "Sistema"
        public string TipoMensaje { get; set; } = string.Empty; // "Recordatorio", "Confirmación", "Cambio", "Cancelación"
        public string Destinatario { get; set; } = string.Empty; // Email o número de teléfono
        public string Asunto { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public string Estado { get; set; } = "Pendiente"; // "Pendiente", "Enviada", "Error"
        public string? MensajeError { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime? FechaEnvio { get; set; }
        public int Intentos { get; set; } = 0;

        // Relaciones
        public virtual Cliente? Cliente { get; set; }
        public virtual Usuario? Usuario { get; set; }
        public virtual Cita? Cita { get; set; }
    }
}
