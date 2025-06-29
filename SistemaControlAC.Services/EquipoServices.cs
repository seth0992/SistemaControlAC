using SistemaControlAC.Core.Entities;
using SistemaControlAC.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SistemaControlAC.Services
{
    public class EquipoService : IEquipoService
    {
        private readonly IEquipoRepository _equipoRepository;
        private readonly ILogger<EquipoService> _logger;

        public EquipoService(IEquipoRepository equipoRepository, ILogger<EquipoService> logger = null)
        {
            _equipoRepository = equipoRepository;
            _logger = logger;
        }

        public async Task<(bool Success, string Message, EquipoAireAcondicionado? Equipo)> CreateAsync(EquipoAireAcondicionado equipo)
        {
            try
            {
                _logger?.LogInformation("Iniciando creación de equipo para cliente {ClienteId}", equipo.ClienteId);

                // Validar equipo
                if (!await ValidateEquipoAsync(equipo))
                {
                    _logger?.LogWarning("Validación de equipo falló para cliente {ClienteId}", equipo.ClienteId);
                    return (false, "Los datos del equipo no son válidos", null);
                }

                // Verificar si ya existe el número de serie (si se proporciona)
                if (!string.IsNullOrWhiteSpace(equipo.NumeroSerie) &&
                    await _equipoRepository.ExistsAsync(equipo.NumeroSerie))
                {
                    _logger?.LogWarning("Número de serie {NumeroSerie} ya existe", equipo.NumeroSerie);
                    return (false, "Ya existe un equipo registrado con ese número de serie", null);
                }

                // Normalizar datos
                NormalizeEquipoData(equipo);

                // Crear equipo
                var success = await _equipoRepository.CreateAsync(equipo);

                if (success)
                {
                    _logger?.LogInformation("Equipo creado exitosamente con ID {EquipoId}", equipo.Id);
                    return (true, "Equipo creado exitosamente", equipo);
                }
                else
                {
                    _logger?.LogError("CreateAsync retornó false sin excepción");
                    return (false, "Error al crear el equipo en la base de datos", null);
                }
            }
            catch (InvalidOperationException ex)
            {
                // Errores específicos de validación o base de datos
                _logger?.LogError(ex, "Error de operación al crear equipo");

                string message = ex.Message.Contains("no existe") ?
                    "El cliente seleccionado no existe" :
                    ex.Message.Contains("tabla") || ex.Message.Contains("Invalid object name") ?
                        "Error de configuración de base de datos. Contacte al administrador." :
                        ex.Message;

                return (false, message, null);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error inesperado al crear equipo");
                return (false, $"Error inesperado: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Message, EquipoAireAcondicionado? Equipo)> UpdateAsync(EquipoAireAcondicionado equipo)
        {
            try
            {
                _logger?.LogInformation("Iniciando actualización de equipo {EquipoId}", equipo.Id);

                // Validar equipo
                if (!await ValidateEquipoAsync(equipo, true))
                {
                    return (false, "Los datos del equipo no son válidos", null);
                }

                // Verificar si ya existe el número de serie (excluyendo el equipo actual)
                if (!string.IsNullOrWhiteSpace(equipo.NumeroSerie) &&
                    await _equipoRepository.ExistsAsync(equipo.NumeroSerie, equipo.Id))
                {
                    return (false, "Ya existe otro equipo registrado con ese número de serie", null);
                }

                // Normalizar datos
                NormalizeEquipoData(equipo);

                // Actualizar equipo
                var success = await _equipoRepository.UpdateAsync(equipo);

                if (success)
                {
                    _logger?.LogInformation("Equipo {EquipoId} actualizado exitosamente", equipo.Id);
                    return (true, "Equipo actualizado exitosamente", equipo);
                }
                else
                {
                    return (false, "Error al actualizar el equipo en la base de datos", null);
                }
            }
            catch (InvalidOperationException ex)
            {
                _logger?.LogError(ex, "Error de operación al actualizar equipo {EquipoId}", equipo.Id);
                return (false, ex.Message, null);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error inesperado al actualizar equipo {EquipoId}", equipo.Id);
                return (false, $"Error inesperado: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Message)> DeleteAsync(int id)
        {
            try
            {
                var equipo = await _equipoRepository.GetByIdAsync(id);
                if (equipo == null)
                {
                    return (false, "El equipo no existe");
                }

                var success = await _equipoRepository.DeleteAsync(id);

                if (success)
                {
                    _logger?.LogInformation("Equipo {EquipoId} eliminado exitosamente", id);
                    return (true, "Equipo eliminado exitosamente");
                }
                else
                {
                    return (false, "Error al eliminar el equipo");
                }
            }
            catch (InvalidOperationException ex)
            {
                _logger?.LogError(ex, "Error de operación al eliminar equipo {EquipoId}", id);
                return (false, ex.Message);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error inesperado al eliminar equipo {EquipoId}", id);
                return (false, $"Error inesperado: {ex.Message}");
            }
        }

        // Métodos de consulta (sin cambios sustanciales, solo logging)
        public async Task<EquipoAireAcondicionado?> GetByIdAsync(int id)
        {
            try
            {
                return await _equipoRepository.GetWithClienteAsync(id);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al obtener equipo {EquipoId}", id);
                throw;
            }
        }

        public async Task<List<EquipoAireAcondicionado>> GetAllAsync()
        {
            return await _equipoRepository.GetWithClienteAsync();
        }

        public async Task<List<EquipoAireAcondicionado>> GetActiveAsync()
        {
            return await _equipoRepository.GetActiveAsync();
        }

        public async Task<List<EquipoAireAcondicionado>> GetByClienteAsync(int clienteId)
        {
            return await _equipoRepository.GetByClienteAsync(clienteId);
        }

        public async Task<List<EquipoAireAcondicionado>> SearchAsync(string searchTerm)
        {
            return await _equipoRepository.SearchAsync(searchTerm);
        }

        public async Task<EquipoAireAcondicionado?> GetWithClienteAsync(int id)
        {
            return await _equipoRepository.GetWithClienteAsync(id);
        }

        public async Task<List<EquipoAireAcondicionado>> GetWithClienteAsync()
        {
            return await _equipoRepository.GetWithClienteAsync();
        }

        // Métodos de validación y utilidad
        public async Task<bool> ValidateEquipoAsync(EquipoAireAcondicionado equipo, bool isUpdate = false)
        {
            if (equipo == null)
            {
                _logger?.LogWarning("Equipo es null en validación");
                return false;
            }

            // Validar campos requeridos
            if (equipo.ClienteId <= 0)
            {
                _logger?.LogWarning("ClienteId inválido: {ClienteId}", equipo.ClienteId);
                return false;
            }

            if (string.IsNullOrWhiteSpace(equipo.Marca))
            {
                _logger?.LogWarning("Marca está vacía");
                return false;
            }

            if (string.IsNullOrWhiteSpace(equipo.Modelo))
            {
                _logger?.LogWarning("Modelo está vacío");
                return false;
            }

            if (string.IsNullOrWhiteSpace(equipo.Tipo))
            {
                _logger?.LogWarning("Tipo está vacío");
                return false;
            }

            if (string.IsNullOrWhiteSpace(equipo.Ubicacion))
            {
                _logger?.LogWarning("Ubicación está vacía");
                return false;
            }

            // Validar longitudes
            if (equipo.Marca.Length > 50 || equipo.Modelo.Length > 50)
            {
                _logger?.LogWarning("Marca o modelo exceden 50 caracteres");
                return false;
            }

            if (!string.IsNullOrWhiteSpace(equipo.NumeroSerie) && equipo.NumeroSerie.Length > 100)
            {
                _logger?.LogWarning("Número de serie excede 100 caracteres");
                return false;
            }

            if (equipo.Tipo.Length > 30)
            {
                _logger?.LogWarning("Tipo excede 30 caracteres");
                return false;
            }

            if (!string.IsNullOrWhiteSpace(equipo.Capacidad) && equipo.Capacidad.Length > 20)
            {
                _logger?.LogWarning("Capacidad excede 20 caracteres");
                return false;
            }

            if (equipo.Ubicacion.Length > 100)
            {
                _logger?.LogWarning("Ubicación excede 100 caracteres");
                return false;
            }

            // Validar tipos válidos
            var tiposValidos = await GetTiposEquipoAsync();
            if (!tiposValidos.Contains(equipo.Tipo))
            {
                _logger?.LogWarning("Tipo de equipo inválido: {Tipo}", equipo.Tipo);
                return false;
            }

            return true;
        }

        private void NormalizeEquipoData(EquipoAireAcondicionado equipo)
        {
            // Normalizar strings
            equipo.Marca = equipo.Marca?.Trim();
            equipo.Modelo = equipo.Modelo?.Trim();
            equipo.NumeroSerie = string.IsNullOrWhiteSpace(equipo.NumeroSerie) ? null : equipo.NumeroSerie.Trim();
            equipo.Tipo = equipo.Tipo?.Trim();
            equipo.Capacidad = string.IsNullOrWhiteSpace(equipo.Capacidad) ? null : equipo.Capacidad.Trim();
            equipo.Ubicacion = equipo.Ubicacion?.Trim();

            // Capitalizar primera letra
            if (!string.IsNullOrEmpty(equipo.Marca))
                equipo.Marca = CapitalizeFirstLetter(equipo.Marca);

            if (!string.IsNullOrEmpty(equipo.Modelo))
                equipo.Modelo = CapitalizeFirstLetter(equipo.Modelo);

            if (!string.IsNullOrEmpty(equipo.Ubicacion))
                equipo.Ubicacion = CapitalizeFirstLetter(equipo.Ubicacion);
        }

        private string CapitalizeFirstLetter(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return char.ToUpper(input[0]) + input.Substring(1).ToLower();
        }

        public async Task<string> GenerateEquipoCodeAsync()
        {
            var count = await _equipoRepository.CountAsync();
            return $"EQ-{(count + 1):D6}";
        }

        public async Task<int> CountAsync()
        {
            return await _equipoRepository.CountAsync();
        }

        public async Task<int> CountActiveAsync()
        {
            return await _equipoRepository.CountActiveAsync();
        }

        public async Task<int> CountByClienteAsync(int clienteId)
        {
            return await _equipoRepository.CountByClienteAsync(clienteId);
        }

        public async Task<List<string>> GetTiposEquipoAsync()
        {
            await Task.CompletedTask;
            return new List<string>
            {
                "Split",
                "Central",
                "Ventana",
                "Portatil",
                "Cassette",
                "Piso Techo",
                "Mini Split"
            };
        }

        public async Task<List<string>> GetMarcasAsync()
        {
            await Task.CompletedTask;
            return new List<string>
            {
                "LG",
                "Samsung",
                "Daikin",
                "Carrier",
                "Trane",
                "Mitsubishi",
                "Panasonic",
                "York",
                "Rheem",
                "Goodman",
                "Lennox",
                "Fujitsu",
                "Otro"
            };
        }
    }
}