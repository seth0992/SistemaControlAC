using SistemaControlAC.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaControlAC.Core.Interfaces
{
    public interface IAuthenticationService
    {
        Task<(bool Success, Usuario? Usuario, string Message)> LoginAsync(string username, string password);
        Task LogoutAsync();
        string HashPassword(string password);
        bool VerifyPassword(string password, string hashedPassword);
        Usuario? CurrentUser { get; }
        bool IsAuthenticated { get; }
    }
}
