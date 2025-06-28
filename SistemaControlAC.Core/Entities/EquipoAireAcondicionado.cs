using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaControlAC.Core.Entities
{
    public class EquipoAireAcondicionado
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string? NumeroSerie { get; set; }
        public string Tipo { get; set; } = string.Empty; // "Split", "Window", "Central", etc.
        public string? Capacidad { get; set; } // BTU o Toneladas
        public DateTime? FechaInstalacion { get; set; }
        public string Ubicacion { get; set; } = string.Empty; // Habitación, oficina, etc.
        public bool Activo { get; set; } = true;

        // Relaciones
        public virtual Cliente? Cliente { get; set; }
        //public virtual ICollection<Cita>? Citas { get; set; }
        //public virtual ICollection<NotaReparacion>? NotasReparacion { get; set; }
    }
}
