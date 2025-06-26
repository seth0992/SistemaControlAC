using SistemaControlAC.Core.Entities;
using SistemaControlAC.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SistemaControlAC.Services
{
    public class ClienteService : IClienteService
    {
        private readonly IClienteRepository _clienteRepository;

        public ClienteService(IClienteRepository clienteRepository)
        {
            _clienteRepository = clienteRepository;
        }

        public async Task<(bool Success, string Message, Cliente? Cliente)> CreateAsync(Cliente cliente)
        {
            try
            {
                // Validar cliente
                if (!await ValidateClienteAsync(cliente))
                {
                    return (false, "Los datos del cliente no son válidos", null);
                }

                // Verificar si ya existe el teléfono
                if (await _clienteRepository.ExistsAsync(cliente.Telefono))
                {
                    return (false, "Ya existe un cliente registrado con ese número de teléfono", null);
                }

                // Verificar si ya existe el email (si se proporciona)
                if (!string.IsNullOrWhiteSpace(cliente.Email) &&
                    await _clienteRepository.ExistsEmailAsync(cliente.Email))
                {
                    return (false, "Ya existe un cliente registrado con ese correo electrónico", null);
                }

                // Normalizar datos
                NormalizeClienteData(cliente);

                // Crear cliente
                var success = await _clienteRepository.CreateAsync(cliente);

                if (success)
                {
                    return (true, "Cliente creado exitosamente", cliente);
                }
                else
                {
                    return (false, "Error al crear el cliente en la base de datos", null);
                }
            }
            catch (Exception ex)
            {
                return (false, $"Error inesperado: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Message, Cliente? Cliente)> UpdateAsync(Cliente cliente)
        {
            try
            {
                // Validar cliente
                if (!await ValidateClienteAsync(cliente, true))
                {
                    return (false, "Los datos del cliente no son válidos", null);
                }

                // Verificar si ya existe el teléfono (excluyendo el cliente actual)
                if (await _clienteRepository.ExistsAsync(cliente.Telefono, cliente.Id))
                {
                    return (false, "Ya existe otro cliente registrado con ese número de teléfono", null);
                }

                // Verificar si ya existe el email (si se proporciona y excluyendo el cliente actual)
                if (!string.IsNullOrWhiteSpace(cliente.Email) &&
                    await _clienteRepository.ExistsEmailAsync(cliente.Email, cliente.Id))
                {
                    return (false, "Ya existe otro cliente registrado con ese correo electrónico", null);
                }

                // Normalizar datos
                NormalizeClienteData(cliente);

                // Actualizar cliente
                var success = await _clienteRepository.UpdateAsync(cliente);

                if (success)
                {
                    return (true, "Cliente actualizado exitosamente", cliente);
                }
                else
                {
                    return (false, "Error al actualizar el cliente en la base de datos", null);
                }
            }
            catch (Exception ex)
            {
                return (false, $"Error inesperado: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Message)> DeleteAsync(int id)
        {
            try
            {
                var cliente = await _clienteRepository.GetByIdAsync(id);
                if (cliente == null)
                {
                    return (false, "El cliente no existe");
                }

                // Verificar si el cliente tiene equipos activos
                var clienteConEquipos = await _clienteRepository.GetWithEquiposAsync(id);
                if (clienteConEquipos?.Equipos?.Any(e => e.Activo) == true)
                {
                    return (false, "No se puede eliminar el cliente porque tiene equipos activos asociados");
                }

                var success = await _clienteRepository.DeleteAsync(id);

                if (success)
                {
                    return (true, "Cliente eliminado exitosamente");
                }
                else
                {
                    return (false, "Error al eliminar el cliente");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Error inesperado: {ex.Message}");
            }
        }

        public async Task<Cliente?> GetByIdAsync(int id)
        {
            return await _clienteRepository.GetByIdAsync(id);
        }

        public async Task<List<Cliente>> GetAllAsync()
        {
            return await _clienteRepository.GetAllAsync();
        }

        public async Task<List<Cliente>> GetActiveAsync()
        {
            return await _clienteRepository.GetActiveAsync();
        }

        public async Task<List<Cliente>> SearchAsync(string searchTerm)
        {
            return await _clienteRepository.SearchAsync(searchTerm);
        }

        public async Task<Cliente?> GetWithEquiposAsync(int id)
        {
            return await _clienteRepository.GetWithEquiposAsync(id);
        }

        public async Task<List<Cliente>> GetWithEquiposAsync()
        {
            return await _clienteRepository.GetWithEquiposAsync();
        }

        public async Task<bool> ValidateClienteAsync(Cliente cliente, bool isUpdate = false)
        {
            if (cliente == null)
                return false;

            // Validar campos requeridos según la estructura de BD
            if (string.IsNullOrWhiteSpace(cliente.Nombre))
                return false;

            if (string.IsNullOrWhiteSpace(cliente.Apellido))
                return false;

            if (string.IsNullOrWhiteSpace(cliente.Telefono))
                return false;

            if (string.IsNullOrWhiteSpace(cliente.Direccion))
                return false;

            if (string.IsNullOrWhiteSpace(cliente.Ciudad))
                return false;

            // Validar formato de teléfono (números, espacios, guiones, paréntesis)
            if (!IsValidPhoneNumber(cliente.Telefono))
                return false;

            // Validar email si se proporciona
            if (!string.IsNullOrWhiteSpace(cliente.Email) && !IsValidEmail(cliente.Email))
                return false;

            // Validar longitudes según el esquema de BD
            if (cliente.Nombre.Length > 100 || cliente.Apellido.Length > 100)
                return false;

            if (cliente.Telefono.Length > 20)
                return false;

            if (!string.IsNullOrWhiteSpace(cliente.Email) && cliente.Email.Length > 100)
                return false;

            if (cliente.Direccion.Length > 255)
                return false;

            if (cliente.Ciudad.Length > 50)
                return false;

            if (!string.IsNullOrWhiteSpace(cliente.CodigoPostal) && cliente.CodigoPostal.Length > 10)
                return false;

            return true;
        }

        public async Task<string> GenerateClienteCodeAsync()
        {
            var count = await _clienteRepository.CountAsync();
            return $"CLI-{(count + 1):D6}";
        }

        public async Task<int> CountAsync()
        {
            return await _clienteRepository.CountAsync();
        }

        public async Task<int> CountActiveAsync()
        {
            return await _clienteRepository.CountActiveAsync();
        }

        #region Métodos Privados

        private void NormalizeClienteData(Cliente cliente)
        {
            // Normalizar nombres y apellidos
            cliente.Nombre = NormalizeText(cliente.Nombre);
            cliente.Apellido = NormalizeText(cliente.Apellido);

            // Normalizar email
            if (!string.IsNullOrWhiteSpace(cliente.Email))
            {
                cliente.Email = cliente.Email.Trim().ToLower();
            }

            // Normalizar teléfono
            cliente.Telefono = NormalizePhoneNumber(cliente.Telefono);

            // Normalizar ciudad
            if (!string.IsNullOrWhiteSpace(cliente.Ciudad))
            {
                cliente.Ciudad = NormalizeText(cliente.Ciudad);
            }

            // Normalizar código postal
            if (!string.IsNullOrWhiteSpace(cliente.CodigoPostal))
            {
                cliente.CodigoPostal = cliente.CodigoPostal.Trim().ToUpper();
            }

            // Normalizar notas
            if (!string.IsNullOrWhiteSpace(cliente.Notas))
            {
                cliente.Notas = cliente.Notas.Trim();
            }
        }

        private string NormalizeText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            // Capitalizar primera letra de cada palabra
            var words = text.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length > 0)
                {
                    words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1).ToLower();
                }
            }

            return string.Join(" ", words);
        }

        private string NormalizePhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return string.Empty;

            // Remover caracteres no numéricos excepto espacios y guiones
            return phoneNumber.Trim();
        }

        private bool IsValidPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return false;

            // Permitir números, espacios, guiones, paréntesis y el símbolo +
            var pattern = @"^[\d\s\-\(\)\+]+$";
            var regex = new Regex(pattern);

            // Verificar que tenga al menos 8 dígitos
            var digitCount = phoneNumber.Count(char.IsDigit);

            return regex.IsMatch(phoneNumber) && digitCount >= 8 && digitCount <= 15;
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
                var regex = new Regex(pattern);
                return regex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}
