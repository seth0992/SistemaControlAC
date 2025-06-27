using SistemaControlAC.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaControlAC.Core.Interfaces
{
    public interface ICitaService
    {
        Task<(bool Success, string Message, Cita? Cita)> CreateAsync(Cita cita);
        Task<(bool Success, string Message, Cita? Cita)> UpdateAsync(Cita cita);
        Task<(bool Success, string Message)> DeleteAsync(int id);
        Task<(bool Success, string Message)> CancelAsync(int id, string motivo);
        Task<Cita?> GetByIdAsync(int id);
        Task<List<Cita>> GetAllAsync();
        Task<List<Cita>> GetByClienteAsync(int clienteId);
        Task<List<Cita>> GetByTecnicoAsync(int tecnicoId);
        Task<List<Cita>> GetByFechaAsync(DateTime fecha);
        Task<List<Cita>> GetByEstadoAsync(string estado);
        Task<List<Cita>> GetCitasDelDiaAsync(DateTime fecha);
        Task<List<Cita>> GetCitasPendientesAsync();
        Task<List<Cita>> SearchAsync(string searchTerm);
        Task<bool> ValidateCitaAsync(Cita cita, bool isUpdate = false);
        Task<bool> IsTecnicoAvailableAsync(int tecnicoId, DateTime fechaInicio, DateTime fechaFin, int? excludeCitaId = null);
        Task<int> CountAsync();
        Task<int> CountByEstadoAsync(string estado);
        Task<List<Cita>> GetCitasEnRangoAsync(DateTime fechaInicio, DateTime fechaFin);
        Task<(bool Success, string Message)> CompletarCitaAsync(int citaId);
        Task<List<string>> GetTiposServicioAsync();
        Task<List<Usuario>> GetTecnicosDisponiblesAsync(DateTime fechaInicio, DateTime fechaFin);
    }
}
