using System.Threading.Tasks;

namespace SistemaControlAC.Core.Interfaces
{
    public interface IEmailService
    {
        Task<(bool Success, string Message)> SendEmailAsync(string to, string subject, string body, bool isHtml = true);
        Task<(bool Success, string Message)> SendCitaConfirmationAsync(int citaId);
        Task<(bool Success, string Message)> SendCitaReminderAsync(int citaId);
        Task<(bool Success, string Message)> SendCitaCancellationAsync(int citaId, string motivo);
        Task<(bool Success, string Message)> SendCitaRescheduleAsync(int citaId, DateTime fechaAnterior);
        bool IsConfigured();
    }
}