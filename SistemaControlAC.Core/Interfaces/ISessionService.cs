using SistemaControlAC.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaControlAC.Core.Interfaces
{
    public interface ISessionService
    {
        Usuario? CurrentUser { get; set; }
        bool IsAuthenticated { get; }
        void UpdateActivity();
        void EndSession();
        string GetUserFullName();
        string GetUserRole();
        bool HasRole(string role);
        bool IsAdmin();
        bool IsTechnician();
        bool IsReceptionist();
        event EventHandler? SessionExpired;
    }
}
