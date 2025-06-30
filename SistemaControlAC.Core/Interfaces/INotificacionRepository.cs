using SistemaControlAC.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SistemaControlAC.Core.Interfaces
{
    public interface INotificacionRepository
    {
        Task<Notificacion?> GetByIdAsync(int id);
        Task<List<Notificacion>> GetAllAsync();
        Task<List<Notificacion>> GetPendientesAsync();
        Task<List<Notificacion>> GetByCitaAsync(int citaId);
        Task<List<Notificacion>> GetByClienteAsync(int clienteId);
        Task<bool> CreateAsync(Notificacion notificacion);
        Task<bool> UpdateAsync(Notificacion notificacion);
        Task<bool> MarcarComoEnviadaAsync(int id, DateTime fechaEnvio);
        Task<bool> MarcarComoErrorAsync(int id, string mensajeError);
        Task<int> CountPendientesAsync();
    }
}