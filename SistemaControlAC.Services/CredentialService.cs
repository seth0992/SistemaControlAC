using Microsoft.Win32;
using SistemaControlAC.Core.Interfaces;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SistemaControlAC.Services
{
    public class CredentialService : ICredentialService
    {
        private const string REGISTRY_KEY = @"SOFTWARE\SistemaControlAC";
        private const string USERNAME_VALUE = "SavedUsername";
        private const string PASSWORD_VALUE = "SavedPassword";
        private const string ENTROPY_VALUE = "Entropy";

        public async Task SaveCredentialsAsync(string username, string password)
        {
            try
            {
                // Generar entropía aleatoria para cifrado
                var entropy = new byte[20];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(entropy);
                }

                // Cifrar la contraseña
                var passwordBytes = Encoding.UTF8.GetBytes(password);
                var encryptedPassword = ProtectedData.Protect(passwordBytes, entropy, DataProtectionScope.CurrentUser);

                // Guardar en el registro
                using (var key = Registry.CurrentUser.CreateSubKey(REGISTRY_KEY))
                {
                    if (key != null)
                    {
                        key.SetValue(USERNAME_VALUE, username);
                        key.SetValue(PASSWORD_VALUE, Convert.ToBase64String(encryptedPassword));
                        key.SetValue(ENTROPY_VALUE, Convert.ToBase64String(entropy));
                    }
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                // Log del error
                System.Diagnostics.Debug.WriteLine($"Error al guardar credenciales: {ex.Message}");
                throw;
            }
        }

        public async Task<(string Username, string Password)?> GetSavedCredentialsAsync()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(REGISTRY_KEY))
                {
                    if (key == null) return null;

                    var username = key.GetValue(USERNAME_VALUE) as string;
                    var encryptedPasswordBase64 = key.GetValue(PASSWORD_VALUE) as string;
                    var entropyBase64 = key.GetValue(ENTROPY_VALUE) as string;

                    if (string.IsNullOrEmpty(username) ||
                        string.IsNullOrEmpty(encryptedPasswordBase64) ||
                        string.IsNullOrEmpty(entropyBase64))
                    {
                        return null;
                    }

                    // Descifrar la contraseña
                    var encryptedPassword = Convert.FromBase64String(encryptedPasswordBase64);
                    var entropy = Convert.FromBase64String(entropyBase64);
                    var decryptedPasswordBytes = ProtectedData.Unprotect(encryptedPassword, entropy, DataProtectionScope.CurrentUser);
                    var password = Encoding.UTF8.GetString(decryptedPasswordBytes);

                    await Task.CompletedTask;
                    return (username, password);
                }
            }
            catch (Exception ex)
            {
                // Log del error y limpiar credenciales corruptas
                System.Diagnostics.Debug.WriteLine($"Error al recuperar credenciales: {ex.Message}");
                await ClearCredentialsAsync();
                return null;
            }
        }

        public async Task ClearCredentialsAsync()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(REGISTRY_KEY, writable: true))
                {
                    if (key != null)
                    {
                        key.DeleteValue(USERNAME_VALUE, false);
                        key.DeleteValue(PASSWORD_VALUE, false);
                        key.DeleteValue(ENTROPY_VALUE, false);
                    }
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al limpiar credenciales: {ex.Message}");
            }
        }

        public async Task<bool> HasSavedCredentialsAsync()
        {
            var credentials = await GetSavedCredentialsAsync();
            return credentials.HasValue;
        }
    }
}