using SistemaControlAC.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SistemaControlAC.Core.Interfaces
{
    public interface INotificationService
    {
        Task<(bool Success, string Message)> CreateNotificationAsync(Notificacion notificacion);
        Task<(bool Success, string Message)> ProcessPendingNotificationsAsync();
        Task<(bool Success, string Message)> SendCitaNotificationAsync(int citaId, string tipoMensaje);
        Task<List<Notificacion>> GetNotificacionesPendientesAsync();
        Task<List<Notificacion>> GetNotificacionesByCitaAsync(int citaId);
    }
}