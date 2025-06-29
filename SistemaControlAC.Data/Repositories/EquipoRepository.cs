using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SistemaControlAC.Core.Entities;
using SistemaControlAC.Core.Interfaces;
using SistemaControlAC.Data.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaControlAC.Data.Repositories
{
    public class EquipoRepository : IEquipoRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EquipoRepository> _logger;

        public EquipoRepository(ApplicationDbContext context, ILogger<EquipoRepository> logger = null)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<EquipoAireAcondicionado?> GetByIdAsync(int id)
        {
            try
            {
                return await _context.Equipos
                    .FirstOrDefaultAsync(e => e.Id == id);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al obtener equipo por ID: {EquipoId}", id);
                throw;
            }
        }

        public async Task<EquipoAireAcondicionado?> GetWithClienteAsync(int id)
        {
            try
            {
                return await _context.Equipos
                    .Include(e => e.Cliente)
                    .FirstOrDefaultAsync(e => e.Id == id);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al obtener equipo con cliente por ID: {EquipoId}", id);
                throw;
            }
        }

        public async Task<List<EquipoAireAcondicionado>> GetAllAsync()
        {
            try
            {
                return await _context.Equipos
                    .OrderBy(e => e.Cliente!.Nombre)
                    .ThenBy(e => e.Ubicacion)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al obtener todos los equipos");
                throw;
            }
        }

        public async Task<List<EquipoAireAcondicionado>> GetWithClienteAsync()
        {
            try
            {
                return await _context.Equipos
                    .Include(e => e.Cliente)
                    .OrderBy(e => e.Cliente!.Nombre)
                    .ThenBy(e => e.Ubicacion)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al obtener equipos con información de cliente");
                throw;
            }
        }

        public async Task<List<EquipoAireAcondicionado>> GetActiveAsync()
        {
            try
            {
                return await _context.Equipos
                    .Include(e => e.Cliente)
                    .Where(e => e.Activo)
                    .OrderBy(e => e.Cliente!.Nombre)
                    .ThenBy(e => e.Ubicacion)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al obtener equipos activos");
                throw;
            }
        }

        public async Task<List<EquipoAireAcondicionado>> GetByClienteAsync(int clienteId)
        {
            try
            {
                return await _context.Equipos
                    .Where(e => e.ClienteId == clienteId)
                    .OrderBy(e => e.Ubicacion)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al obtener equipos del cliente: {ClienteId}", clienteId);
                throw;
            }
        }

        public async Task<bool> CreateAsync(EquipoAireAcondicionado equipo)
        {
            try
            {
                _logger?.LogInformation("Intentando crear equipo para cliente {ClienteId}", equipo.ClienteId);

                // Verificar que el cliente existe
                var clienteExiste = await _context.Clientes
                    .AnyAsync(c => c.Id == equipo.ClienteId);

                if (!clienteExiste)
                {
                    _logger?.LogWarning("Cliente {ClienteId} no existe", equipo.ClienteId);
                    throw new InvalidOperationException($"El cliente con ID {equipo.ClienteId} no existe");
                }

                _context.Equipos.Add(equipo);
                var affectedRows = await _context.SaveChangesAsync();

                _logger?.LogInformation("Equipo creado exitosamente con ID: {EquipoId}", equipo.Id);
                return affectedRows > 0;
            }
            catch (DbUpdateException ex)
            {
                _logger?.LogError(ex, "Error de base de datos al crear equipo. Inner exception: {InnerException}",
                    ex.InnerException?.Message);

                // Lanzar la excepción con más información
                throw new InvalidOperationException(
                    $"Error al guardar en la base de datos: {ex.InnerException?.Message ?? ex.Message}", ex);
            }
            catch (InvalidOperationException)
            {
                // Re-lanzar excepciones de validación de negocio
                throw;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error inesperado al crear equipo");
                throw new InvalidOperationException($"Error inesperado: {ex.Message}", ex);
            }
        }

        public async Task<bool> UpdateAsync(EquipoAireAcondicionado equipo)
        {
            try
            {
                var existingEquipo = await _context.Equipos
                    .FirstOrDefaultAsync(e => e.Id == equipo.Id);

                if (existingEquipo == null)
                {
                    _logger?.LogWarning("Equipo {EquipoId} no encontrado para actualizar", equipo.Id);
                    return false;
                }

                // Actualizar propiedades
                existingEquipo.ClienteId = equipo.ClienteId;
                existingEquipo.Marca = equipo.Marca;
                existingEquipo.Modelo = equipo.Modelo;
                existingEquipo.NumeroSerie = equipo.NumeroSerie;
                existingEquipo.Tipo = equipo.Tipo;
                existingEquipo.Capacidad = equipo.Capacidad;
                existingEquipo.Ubicacion = equipo.Ubicacion;
                existingEquipo.FechaInstalacion = equipo.FechaInstalacion;
                existingEquipo.Activo = equipo.Activo;

                var affectedRows = await _context.SaveChangesAsync();
                _logger?.LogInformation("Equipo {EquipoId} actualizado exitosamente", equipo.Id);

                return affectedRows > 0;
            }
            catch (DbUpdateException ex)
            {
                _logger?.LogError(ex, "Error de base de datos al actualizar equipo {EquipoId}", equipo.Id);
                throw new InvalidOperationException(
                    $"Error al actualizar en la base de datos: {ex.InnerException?.Message ?? ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error inesperado al actualizar equipo {EquipoId}", equipo.Id);
                throw new InvalidOperationException($"Error inesperado: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var equipo = await _context.Equipos
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (equipo == null)
                {
                    _logger?.LogWarning("Equipo {EquipoId} no encontrado para eliminar", id);
                    return false;
                }

                // Soft delete - marcar como inactivo
                equipo.Activo = false;
                var affectedRows = await _context.SaveChangesAsync();

                _logger?.LogInformation("Equipo {EquipoId} marcado como inactivo", id);
                return affectedRows > 0;
            }
            catch (DbUpdateException ex)
            {
                _logger?.LogError(ex, "Error de base de datos al eliminar equipo {EquipoId}", id);
                throw new InvalidOperationException(
                    $"Error al eliminar en la base de datos: {ex.InnerException?.Message ?? ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error inesperado al eliminar equipo {EquipoId}", id);
                throw new InvalidOperationException($"Error inesperado: {ex.Message}", ex);
            }
        }

        public async Task<bool> ExistsAsync(string numeroSerie, int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(numeroSerie))
                    return false;

                var query = _context.Equipos.Where(e => e.NumeroSerie == numeroSerie);

                if (excludeId.HasValue)
                    query = query.Where(e => e.Id != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al verificar existencia de número de serie: {NumeroSerie}", numeroSerie);
                throw;
            }
        }

        public async Task<List<EquipoAireAcondicionado>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return new List<EquipoAireAcondicionado>();

                searchTerm = searchTerm.ToLower().Trim();

                return await _context.Equipos
                    .Include(e => e.Cliente)
                    .Where(e => e.Marca.ToLower().Contains(searchTerm) ||
                               e.Modelo.ToLower().Contains(searchTerm) ||
                               e.NumeroSerie!.ToLower().Contains(searchTerm) ||
                               e.Ubicacion.ToLower().Contains(searchTerm) ||
                               e.Cliente!.Nombre.ToLower().Contains(searchTerm) ||
                               e.Cliente!.Apellido.ToLower().Contains(searchTerm))
                    .OrderBy(e => e.Cliente!.Nombre)
                    .ThenBy(e => e.Ubicacion)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al buscar equipos con término: {SearchTerm}", searchTerm);
                throw;
            }
        }

        public async Task<int> CountAsync()
        {
            try
            {
                return await _context.Equipos.CountAsync();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al contar equipos");
                throw;
            }
        }

        public async Task<int> CountActiveAsync()
        {
            try
            {
                return await _context.Equipos.CountAsync(e => e.Activo);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al contar equipos activos");
                throw;
            }
        }

        public async Task<int> CountByClienteAsync(int clienteId)
        {
            try
            {
                return await _context.Equipos.CountAsync(e => e.ClienteId == clienteId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al contar equipos del cliente {ClienteId}", clienteId);
                throw;
            }
        }
    }
}