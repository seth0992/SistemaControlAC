using SistemaControlAC.Core.Entities;
using SistemaControlAC.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaControlAC.Services
{
    public class CitaService : ICitaService
    {
        private readonly ICitaRepository _citaRepository;
        private readonly IUsuarioRepository _usuarioRepository;

        public CitaService(ICitaRepository citaRepository, IUsuarioRepository usuarioRepository)
        {
            _citaRepository = citaRepository;
            _usuarioRepository = usuarioRepository;
        }

        public async Task<(bool Success, string Message, Cita? Cita)> CreateAsync(Cita cita)
        {
            try
            {
                // Validar cita
                if (!await ValidateCitaAsync(cita))
                {
                    return (false, "Los datos de la cita no son válidos", null);
                }

                // Verificar disponibilidad del técnico
                var fechaInicio = cita.FechaProgramada.Date.Add(cita.HoraInicio);
                var fechaFin = cita.HoraFin.HasValue ?
                    cita.FechaProgramada.Date.Add(cita.HoraFin.Value) :
                    fechaInicio.AddHours(2); // Duración predeterminada de 2 horas

                if (cita.TecnicoAsignadoId.HasValue &&
                    await _citaRepository.ExistsConflictAsync(cita.TecnicoAsignadoId.Value, fechaInicio, fechaFin))
                {
                    return (false, "El técnico no está disponible en el horario seleccionado", null);
                }

                // Establecer valores por defecto
                cita.Estado = "Programada";
                cita.NotificacionEnviada = false;

                if (!cita.HoraFin.HasValue)
                {
                    cita.HoraFin = cita.HoraInicio.Add(TimeSpan.FromHours(2));
                }

                // Crear cita
                var success = await _citaRepository.CreateAsync(cita);

                if (success)
                {
                    return (true, "Cita creada exitosamente", cita);
                }
                else
                {
                    return (false, "Error al crear la cita en la base de datos", null);
                }
            }
            catch (Exception ex)
            {
                return (false, $"Error inesperado: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Message, Cita? Cita)> UpdateAsync(Cita cita)
        {
            try
            {
                // Validar cita
                if (!await ValidateCitaAsync(cita, true))
                {
                    return (false, "Los datos de la cita no son válidos", null);
                }

                // Verificar disponibilidad del técnico (excluyendo la cita actual)
                var fechaInicio = cita.FechaProgramada.Date.Add(cita.HoraInicio);
                var fechaFin = cita.HoraFin.HasValue ?
                    cita.FechaProgramada.Date.Add(cita.HoraFin.Value) :
                    fechaInicio.AddHours(2);

                if (cita.TecnicoAsignadoId.HasValue &&
                    await _citaRepository.ExistsConflictAsync(cita.TecnicoAsignadoId.Value, fechaInicio, fechaFin, cita.Id))
                {
                    return (false, "El técnico no está disponible en el nuevo horario seleccionado", null);
                }

                // Actualizar cita
                var success = await _citaRepository.UpdateAsync(cita);

                if (success)
                {
                    return (true, "Cita actualizada exitosamente", cita);
                }
                else
                {
                    return (false, "Error al actualizar la cita en la base de datos", null);
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
                var cita = await _citaRepository.GetByIdAsync(id);
                if (cita == null)
                {
                    return (false, "La cita no existe");
                }

                // Verificar si la cita se puede eliminar
                if (cita.Estado == "Completada")
                {
                    return (false, "No se puede eliminar una cita completada");
                }

                var success = await _citaRepository.DeleteAsync(id);

                if (success)
                {
                    return (true, "Cita eliminada exitosamente");
                }
                else
                {
                    return (false, "Error al eliminar la cita");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Error inesperado: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> CancelAsync(int id, string motivo)
        {
            try
            {
                var cita = await _citaRepository.GetByIdAsync(id);
                if (cita == null)
                {
                    return (false, "La cita no existe");
                }

                if (cita.Estado == "Completada")
                {
                    return (false, "No se puede cancelar una cita completada");
                }

                if (cita.Estado == "Cancelada")
                {
                    return (false, "La cita ya está cancelada");
                }

                cita.Estado = "Cancelada";
                if (!string.IsNullOrWhiteSpace(motivo))
                {
                    cita.Descripcion = (cita.Descripcion ?? "") + $"\n\nMotivo de cancelación: {motivo}";
                }

                var success = await _citaRepository.UpdateAsync(cita);

                if (success)
                {
                    return (true, "Cita cancelada exitosamente");
                }
                else
                {
                    return (false, "Error al cancelar la cita");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Error inesperado: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> CompletarCitaAsync(int citaId)
        {
            try
            {
                var cita = await _citaRepository.GetByIdAsync(citaId);
                if (cita == null)
                {
                    return (false, "La cita no existe");
                }

                if (cita.Estado == "Completada")
                {
                    return (false, "La cita ya está completada");
                }

                if (cita.Estado == "Cancelada")
                {
                    return (false, "No se puede completar una cita cancelada");
                }

                cita.Estado = "Completada";
                var success = await _citaRepository.UpdateAsync(cita);

                if (success)
                {
                    return (true, "Cita completada exitosamente");
                }
                else
                {
                    return (false, "Error al completar la cita");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Error inesperado: {ex.Message}");
            }
        }

        public async Task<Cita?> GetByIdAsync(int id)
        {
            return await _citaRepository.GetWithRelationsAsync(id);
        }

        public async Task<List<Cita>> GetAllAsync()
        {
            return await _citaRepository.GetWithRelationsAsync();
        }

        public async Task<List<Cita>> GetByClienteAsync(int clienteId)
        {
            return await _citaRepository.GetByClienteAsync(clienteId);
        }

        public async Task<List<Cita>> GetByTecnicoAsync(int tecnicoId)
        {
            return await _citaRepository.GetByTecnicoAsync(tecnicoId);
        }

        public async Task<List<Cita>> GetByFechaAsync(DateTime fecha)
        {
            return await _citaRepository.GetByFechaAsync(fecha);
        }

        public async Task<List<Cita>> GetByEstadoAsync(string estado)
        {
            return await _citaRepository.GetByEstadoAsync(estado);
        }

        public async Task<List<Cita>> GetCitasDelDiaAsync(DateTime fecha)
        {
            return await _citaRepository.GetCitasDelDiaAsync(fecha);
        }

        public async Task<List<Cita>> GetCitasPendientesAsync()
        {
            return await _citaRepository.GetCitasPendientesAsync();
        }

        public async Task<List<Cita>> GetCitasEnRangoAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            return await _citaRepository.GetCitasEnRangoAsync(fechaInicio, fechaFin);
        }

        public async Task<List<Cita>> SearchAsync(string searchTerm)
        {
            return await _citaRepository.SearchAsync(searchTerm);
        }

        public async Task<bool> ValidateCitaAsync(Cita cita, bool isUpdate = false)
        {
            if (cita == null)
                return false;

            // Validar campos requeridos
            if (cita.ClienteId <= 0)
                return false;

            if (cita.FechaProgramada == default)
                return false;

            if (cita.HoraInicio == default)
                return false;

            if (string.IsNullOrWhiteSpace(cita.TipoServicio))
                return false;

            // Validar que la fecha no sea en el pasado (solo para citas nuevas)
            if (!isUpdate && cita.FechaProgramada.Date < DateTime.Now.Date)
                return false;

            // Validar horario laboral (8:00 AM - 6:00 PM)
            if (cita.HoraInicio < TimeSpan.FromHours(8) || cita.HoraInicio > TimeSpan.FromHours(18))
                return false;

            // Validar que hora fin sea mayor que hora inicio
            if (cita.HoraFin.HasValue && cita.HoraFin <= cita.HoraInicio)
                return false;

            // Validar tipos de servicio válidos
            var tiposValidos = new[] { "Mantenimiento Preventivo", "Reparación", "Instalación", "Revisión" };
            if (!tiposValidos.Contains(cita.TipoServicio))
                return false;

            return true;
        }

        public async Task<bool> IsTecnicoAvailableAsync(int tecnicoId, DateTime fechaInicio, DateTime fechaFin, int? excludeCitaId = null)
        {
            return !await _citaRepository.ExistsConflictAsync(tecnicoId, fechaInicio, fechaFin, excludeCitaId);
        }

        public async Task<int> CountAsync()
        {
            return await _citaRepository.CountAsync();
        }

        public async Task<int> CountByEstadoAsync(string estado)
        {
            return await _citaRepository.CountByEstadoAsync(estado);
        }

        public async Task<List<string>> GetTiposServicioAsync()
        {
            return new List<string>
            {
                "Mantenimiento Preventivo",
                "Reparación",
                "Instalación",
                "Revisión"
            };
        }

        public async Task<List<Usuario>> GetTecnicosDisponiblesAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            var todosTecnicos = await _usuarioRepository.GetAllAsync();
            var tecnicos = todosTecnicos.Where(u => u.Rol.Contains("Tecnico") || u.Rol.Contains("Técnico")).ToList();

            var tecnicosDisponibles = new List<Usuario>();

            foreach (var tecnico in tecnicos)
            {
                var disponible = await IsTecnicoAvailableAsync(tecnico.Id, fechaInicio, fechaFin);
                if (disponible)
                {
                    tecnicosDisponibles.Add(tecnico);
                }
            }

            return tecnicosDisponibles;
        }
    }
}