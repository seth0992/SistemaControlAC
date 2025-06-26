using SistemaControlAC.Core.Entities;
using SistemaControlAC.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaControlAC.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private Usuario? _currentUser;

        public AuthenticationService(IUsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }

        public Usuario? CurrentUser => _currentUser;
        public bool IsAuthenticated => _currentUser != null;

        public async Task<(bool Success, Usuario? Usuario, string Message)> LoginAsync(string username, string password)
        {
            try
            {
                // Validar entrada
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    return (false, null, "Usuario y contraseña son requeridos");
                }

                // Buscar usuario
                var usuario = await _usuarioRepository.GetByUsernameAsync(username);

                if (usuario == null)
                {
                    return (false, null, "Usuario o contraseña incorrectos");
                }

                // Verificar contraseña
                if (!VerifyPassword(password, usuario.PasswordHash))
                {
                    return (false, null, "Usuario o contraseña incorrectos");
                }

                // Verificar si el usuario está activo
                if (!usuario.Activo)
                {
                    return (false, null, "El usuario está desactivado. Contacte al administrador.");
                }

                // Login exitoso
                _currentUser = usuario;

                // Actualizar último acceso
                await _usuarioRepository.UpdateLastAccessAsync(usuario.Id);

                return (true, usuario, "Login exitoso");
            }
            catch (Exception ex)
            {
                return (false, null, $"Error al iniciar sesión: {ex.Message}");
            }
        }

        public async Task LogoutAsync()
        {
            _currentUser = null;
            await Task.CompletedTask;
        }

        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            try
            {
                // Si la contraseña almacenada no está hasheada (para usuarios de prueba)
                if (password == hashedPassword)
                {
                    return true;
                }

                // Verificar con BCrypt
                return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
            }
            catch
            {
                // Si falla la verificación de BCrypt, comparar directamente
                return password == hashedPassword;
            }
        }
    }

}
