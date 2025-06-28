using System.Threading.Tasks;

namespace SistemaControlAC.Core.Interfaces
{
    public interface ICredentialService
    {
        Task SaveCredentialsAsync(string username, string password);
        Task<(string Username, string Password)?> GetSavedCredentialsAsync();
        Task ClearCredentialsAsync();
        Task<bool> HasSavedCredentialsAsync();
    }
}