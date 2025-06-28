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
    public class EquipoService : IEquipoService
    {
        private readonly IEquipoRepository _equipoRepository;

        public EquipoService(IEquipoRepository equipoRepository)
        {
            _equipoRepository = equipoRepository;
        }

        public async Task<(bool Success, string Message, EquipoAireAcondicionado? Equipo)> CreateAsync(EquipoAireAcondicionado equipo)
        {
            try
            {
                // Validar equipo
                if (!await ValidateEquipoAsync(equipo))
                {
                    return (false, "Los datos del equipo no son válidos", null);
                }

                // Verificar si ya existe el número de serie (si se proporciona)
                if (!string.IsNullOrWhiteSpace(equipo.NumeroSerie) &&
                    await _equipoRepository.ExistsAsync(equipo.NumeroSerie))
                {
                    return (false, "Ya existe un equipo registrado con ese número de serie", null);
                }

                // Normalizar datos
                NormalizeEquipoData(equipo);

                // Crear equipo
                var success = await _equipoRepository.CreateAsync(equipo);

                if (success)
                {
                    return (true, "Equipo creado exitosamente", equipo);
                }
                else
                {
                    return (false, "Error al crear el equipo en la base de datos", null);
                }
            }
            catch (Exception ex)
            {
                return (false, $"Error inesperado: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Message, EquipoAireAcondicionado? Equipo)> UpdateAsync(EquipoAireAcondicionado equipo)
        {
            try
            {
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
                    return (true, "Equipo actualizado exitosamente", equipo);
                }
                else
                {
                    return (false, "Error al actualizar el equipo en la base de datos", null);
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
                var equipo = await _equipoRepository.GetByIdAsync(id);
                if (equipo == null)
                {
                    return (false, "El equipo no existe");
                }

                var success = await _equipoRepository.DeleteAsync(id);

                if (success)
                {
                    return (true, "Equipo eliminado exitosamente");
                }
                else
                {
                    return (false, "Error al eliminar el equipo");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Error inesperado: {ex.Message}");
            }
        }

        public async Task<EquipoAireAcondicionado?> GetByIdAsync(int id)
        {
            return await _equipoRepository.GetByIdAsync(id);
        }

        public async Task<List<EquipoAireAcondicionado>> GetAllAsync()
        {
            return await _equipoRepository.GetAllAsync();
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

        public async Task<EquipoAireAcondicionado?> GetWithRelationsAsync(int id)
        {
            return await _equipoRepository.GetWithRelationsAsync(id);
        }

        public async Task<List<EquipoAireAcondicionado>> GetWithRelationsAsync()
        {
            return await _equipoRepository.GetWithRelationsAsync();
        }

        public async Task<bool> ValidateEquipoAsync(EquipoAireAcondicionado equipo, bool isUpdate = false)
        {
            if (equipo == null)
                return false;

            // Validar campos requeridos
            if (equipo.ClienteId <= 0)
                return false;

            if (string.IsNullOrWhiteSpace(equipo.Marca))
                return false;

            if (string.IsNullOrWhiteSpace(equipo.Modelo))
                return false;

            if (string.IsNullOrWhiteSpace(equipo.Tipo))
                return false;

            if (string.IsNullOrWhiteSpace(equipo.Ubicacion))
                return false;

            // Validar longitudes según el esquema de BD
            if (equipo.Marca.Length > 50 || equipo.Modelo.Length > 50)
                return false;

            if (!string.IsNullOrWhiteSpace(equipo.NumeroSerie) && equipo.NumeroSerie.Length > 100)
                return false;

            if (equipo.Tipo.Length > 30)
                return false;

            if (!string.IsNullOrWhiteSpace(equipo.Capacidad) && equipo.Capacidad.Length > 20)
                return false;

            if (equipo.Ubicacion.Length > 100)
                return false;

            // Validar tipos de equipo válidos
            var tiposValidos = await GetTiposEquipoAsync();
            if (!tiposValidos.Contains(equipo.Tipo))
                return false;

            // Validar fecha de instalación
            if (equipo.FechaInstalacion.HasValue && equipo.FechaInstalacion.Value > DateTime.Now)
                return false;

            return true;
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
                "Piso-Techo",
                "Conductos"
            };
        }

        public async Task<List<string>> GetMarcasPopularesAsync()
        {
            await Task.CompletedTask;
            return new List<string>
            {
                "Samsung",
                "LG",
                "Panasonic",
                "Daikin",
                "Carrier",
                "Trane",
                "Mitsubishi",
                "York",
                "Lennox",
                "Rheem",
                "Goodman",
                "American Standard"
            };
        }

        #region Métodos Privados

        private void NormalizeEquipoData(EquipoAireAcondicionado equipo)
        {
            // Normalizar marca y modelo
            equipo.Marca = NormalizeText(equipo.Marca);
            equipo.Modelo = NormalizeText(equipo.Modelo);

            // Normalizar tipo
            equipo.Tipo = NormalizeText(equipo.Tipo);

            // Normalizar ubicación
            equipo.Ubicacion = NormalizeText(equipo.Ubicacion);

            // Normalizar número de serie
            if (!string.IsNullOrWhiteSpace(equipo.NumeroSerie))
            {
                equipo.NumeroSerie = equipo.NumeroSerie.Trim().ToUpper();
            }

            // Normalizar capacidad
            if (!string.IsNullOrWhiteSpace(equipo.Capacidad))
            {
                equipo.Capacidad = equipo.Capacidad.Trim();
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

        #endregion
    }
}