using Microsoft.EntityFrameworkCore;
using SistemaControlAC.Core.Entities;
using SistemaControlAC.Core.Interfaces;
using SistemaControlAC.Data.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaControlAC.Data.Repositories
{
    public class EquipoRepository : IEquipoRepository
    {
        private readonly ApplicationDbContext _context;

        public EquipoRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<EquipoAireAcondicionado?> GetByIdAsync(int id)
        {
            return await _context.Equipos
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<EquipoAireAcondicionado?> GetWithClienteAsync(int id)
        {
            return await _context.Equipos
                .Include(e => e.Cliente)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<List<EquipoAireAcondicionado>> GetAllAsync()
        {
            return await _context.Equipos
                .OrderBy(e => e.Cliente!.Nombre)
                .ThenBy(e => e.Ubicacion)
                .ToListAsync();
        }

        public async Task<List<EquipoAireAcondicionado>> GetWithClienteAsync()
        {
            return await _context.Equipos
                .Include(e => e.Cliente)
                .OrderBy(e => e.Cliente!.Nombre)
                .ThenBy(e => e.Ubicacion)
                .ToListAsync();
        }

        public async Task<List<EquipoAireAcondicionado>> GetActiveAsync()
        {
            return await _context.Equipos
                .Include(e => e.Cliente)
                .Where(e => e.Activo)
                .OrderBy(e => e.Cliente!.Nombre)
                .ThenBy(e => e.Ubicacion)
                .ToListAsync();
        }

        public async Task<List<EquipoAireAcondicionado>> GetByClienteAsync(int clienteId)
        {
            return await _context.Equipos
                .Where(e => e.ClienteId == clienteId)
                .OrderBy(e => e.Ubicacion)
                .ToListAsync();
        }

        public async Task<bool> CreateAsync(EquipoAireAcondicionado equipo)
        {
            try
            {
                _context.Equipos.Add(equipo);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> UpdateAsync(EquipoAireAcondicionado equipo)
        {
            try
            {
                var existingEquipo = await _context.Equipos
                    .FirstOrDefaultAsync(e => e.Id == equipo.Id);

                if (existingEquipo == null)
                    return false;

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

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var equipo = await _context.Equipos
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (equipo == null)
                    return false;

                // Soft delete - marcar como inactivo
                equipo.Activo = false;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> ExistsAsync(string numeroSerie, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(numeroSerie))
                return false;

            var query = _context.Equipos.Where(e => e.NumeroSerie == numeroSerie);

            if (excludeId.HasValue)
            {
                query = query.Where(e => e.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<List<EquipoAireAcondicionado>> SearchAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetActiveAsync();

            var term = searchTerm.ToLower().Trim();

            return await _context.Equipos
                .Include(e => e.Cliente)
                .Where(e => e.Activo &&
                    (e.Marca.ToLower().Contains(term) ||
                     e.Modelo.ToLower().Contains(term) ||
                     (e.NumeroSerie != null && e.NumeroSerie.ToLower().Contains(term)) ||
                     e.Ubicacion.ToLower().Contains(term) ||
                     e.Tipo.ToLower().Contains(term) ||
                     (e.Cliente != null &&
                      (e.Cliente.Nombre.ToLower().Contains(term) ||
                       e.Cliente.Apellido.ToLower().Contains(term)))))
                .OrderBy(e => e.Cliente!.Nombre)
                .ThenBy(e => e.Ubicacion)
                .ToListAsync();
        }

        public async Task<int> CountAsync()
        {
            return await _context.Equipos.CountAsync();
        }

        public async Task<int> CountActiveAsync()
        {
            return await _context.Equipos.CountAsync(e => e.Activo);
        }

        public async Task<int> CountByClienteAsync(int clienteId)
        {
            return await _context.Equipos.CountAsync(e => e.ClienteId == clienteId && e.Activo);
        }
    }
}