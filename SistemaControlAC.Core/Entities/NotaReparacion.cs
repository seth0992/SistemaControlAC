using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaControlAC.Core.Entities
{
    public class NotaReparacion
    {
        public int Id { get; set; }
        public int CitaId { get; set; }
        public int TecnicoId { get; set; }
        public int? EquipoId { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Now;
        public string DiagnosticoInicial { get; set; } = string.Empty;
        public string TrabajoRealizado { get; set; } = string.Empty;
        public string? RepuestosUtilizados { get; set; }
        public string? RecomendacionesFuturas { get; set; }
        public string EstadoEquipo { get; set; } = string.Empty; // "Funcionando", "Requiere Seguimiento", "Fuera de Servicio"
        public TimeSpan TiempoServicio { get; set; }
        public decimal? CostoManoObra { get; set; }
        public decimal? CostoRepuestos { get; set; }
        public decimal? CostoTotal { get; set; }
        public bool RequiereSeguimiento { get; set; } = false;
        public DateTime? FechaSeguimiento { get; set; }

        // Relaciones
        public virtual Cita? Cita { get; set; }
        public virtual Usuario? Tecnico { get; set; }
        public virtual EquipoAireAcondicionado? Equipo { get; set; }
    }
}
