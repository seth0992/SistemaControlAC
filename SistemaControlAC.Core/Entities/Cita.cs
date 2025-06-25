using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaControlAC.Core.Entities
{
    public class Cita
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public int? EquipoId { get; set; }
        public int? TecnicoAsignadoId { get; set; }
        public DateTime FechaProgramada { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan? HoraFin { get; set; }
        public string TipoServicio { get; set; } = string.Empty; // "Mantenimiento Preventivo", "Reparación", "Instalación"
        public string Estado { get; set; } = "Programada"; // "Programada", "En Proceso", "Completada", "Cancelada"
        public string? Descripcion { get; set; }
        public decimal? CostoEstimado { get; set; }
        public decimal? CostoFinal { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public int CreadoPorUsuarioId { get; set; }
        public bool NotificacionEnviada { get; set; } = false;

        // Relaciones
        public virtual Cliente? Cliente { get; set; }
        public virtual EquipoAireAcondicionado? Equipo { get; set; }
        public virtual Usuario? TecnicoAsignado { get; set; }
        public virtual Usuario? CreadoPor { get; set; }
        public virtual ICollection<NotaReparacion>? NotasReparacion { get; set; }
    }
}
