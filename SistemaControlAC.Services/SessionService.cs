using SistemaControlAC.Core.Entities;
using SistemaControlAC.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaControlAC.Services
{
    public class SessionService : ISessionService
    {
        private Usuario? _currentUser;
        private DateTime _lastActivity;
        private readonly int _sessionTimeoutMinutes = 30;

        public event EventHandler? SessionExpired;

        public Usuario? CurrentUser
        {
            get
            {
                CheckSessionTimeout();
                return _currentUser;
            }
            set
            {
                _currentUser = value;
                _lastActivity = DateTime.Now;
            }
        }

        public bool IsAuthenticated => CurrentUser != null;

        public void UpdateActivity()
        {
            _lastActivity = DateTime.Now;
        }

        private void CheckSessionTimeout()
        {
            if (_currentUser != null && DateTime.Now.Subtract(_lastActivity).TotalMinutes > _sessionTimeoutMinutes)
            {
                EndSession();
                SessionExpired?.Invoke(this, EventArgs.Empty);
            }
        }

        public void EndSession()
        {
            _currentUser = null;
        }

        public string GetUserFullName()
        {
            return CurrentUser != null ? $"{CurrentUser.Nombre} {CurrentUser.Apellido}" : string.Empty;
        }

        public string GetUserRole()
        {
            return CurrentUser?.Rol ?? string.Empty;
        }

        public bool HasRole(string role)
        {
            return CurrentUser?.Rol.Equals(role, StringComparison.OrdinalIgnoreCase) ?? false;
        }

        public bool IsAdmin()
        {
            return HasRole("Admin") || HasRole("Administrador");
        }

        public bool IsTechnician()
        {
            return HasRole("Tecnico") || HasRole("Técnico");
        }

        public bool IsReceptionist()
        {
            return HasRole("Recepcionista");
        }
    }
}
