using SistemaControlAC.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaControlAC.Core.Interfaces
{
    public interface ICitaRepository
    {
        Task<Cita?> GetByIdAsync(int id);
        Task<List<Cita>> GetAllAsync();
        Task<List<Cita>> GetByClienteAsync(int clienteId);
        Task<List<Cita>> GetByTecnicoAsync(int tecnicoId);
        Task<List<Cita>> GetByFechaAsync(DateTime fecha);
        Task<List<Cita>> GetByEstadoAsync(string estado);
        Task<List<Cita>> GetCitasDelDiaAsync(DateTime fecha);
        Task<List<Cita>> GetCitasPendientesAsync();
        Task<List<Cita>> GetCitasEnRangoAsync(DateTime fechaInicio, DateTime fechaFin);
        Task<bool> CreateAsync(Cita cita);
        Task<bool> UpdateAsync(Cita cita);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsConflictAsync(int tecnicoId, DateTime fechaInicio, DateTime fechaFin, int? excludeId = null);
        Task<List<Cita>> SearchAsync(string searchTerm);
        Task<int> CountAsync();
        Task<int> CountByEstadoAsync(string estado);
        Task<Cita?> GetWithRelationsAsync(int id);
        Task<List<Cita>> GetWithRelationsAsync();
    }
}
