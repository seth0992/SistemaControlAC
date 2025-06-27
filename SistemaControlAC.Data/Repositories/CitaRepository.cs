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
    public class CitaRepository : ICitaRepository
    {
        private readonly ApplicationDbContext _context;

        public CitaRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Cita?> GetByIdAsync(int id)
        {
            return await _context.Citas
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Cita?> GetWithRelationsAsync(int id)
        {
            return await _context.Citas
                .Include(c => c.Cliente)
                .Include(c => c.TecnicoAsignado)
                .Include(c => c.Equipo)
                .Include(c => c.CreadoPor)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<List<Cita>> GetAllAsync()
        {
            return await _context.Citas
                .OrderByDescending(c => c.FechaProgramada)
                .ToListAsync();
        }

        public async Task<List<Cita>> GetWithRelationsAsync()
        {
            return await _context.Citas
                .Include(c => c.Cliente)
                .Include(c => c.TecnicoAsignado)
                .Include(c => c.Equipo)
                .Include(c => c.CreadoPor)
                .OrderByDescending(c => c.FechaProgramada)
                .ToListAsync();
        }

        public async Task<List<Cita>> GetByClienteAsync(int clienteId)
        {
            return await _context.Citas
                .Include(c => c.TecnicoAsignado)
                .Include(c => c.Equipo)
                .Where(c => c.ClienteId == clienteId)
                .OrderByDescending(c => c.FechaProgramada)
                .ToListAsync();
        }

        public async Task<List<Cita>> GetByTecnicoAsync(int tecnicoId)
        {
            return await _context.Citas
                .Include(c => c.Cliente)
                .Include(c => c.Equipo)
                .Where(c => c.TecnicoAsignadoId == tecnicoId)
                .OrderByDescending(c => c.FechaProgramada)
                .ToListAsync();
        }

        public async Task<List<Cita>> GetByFechaAsync(DateTime fecha)
        {
            return await _context.Citas
                .Include(c => c.Cliente)
                .Include(c => c.TecnicoAsignado)
                .Include(c => c.Equipo)
                .Where(c => c.FechaProgramada.Date == fecha.Date)
                .OrderBy(c => c.HoraInicio)
                .ToListAsync();
        }

        public async Task<List<Cita>> GetByEstadoAsync(string estado)
        {
            return await _context.Citas
                .Include(c => c.Cliente)
                .Include(c => c.TecnicoAsignado)
                .Include(c => c.Equipo)
                .Where(c => c.Estado == estado)
                .OrderByDescending(c => c.FechaProgramada)
                .ToListAsync();
        }

        public async Task<List<Cita>> GetCitasDelDiaAsync(DateTime fecha)
        {
            return await _context.Citas
                .Include(c => c.Cliente)
                .Include(c => c.TecnicoAsignado)
                .Include(c => c.Equipo)
                .Where(c => c.FechaProgramada.Date == fecha.Date &&
                           (c.Estado == "Programada" || c.Estado == "En Proceso"))
                .OrderBy(c => c.HoraInicio)
                .ToListAsync();
        }

        public async Task<List<Cita>> GetCitasPendientesAsync()
        {
            var fechaLimite = DateTime.Now.AddDays(-1);
            return await _context.Citas
                .Include(c => c.Cliente)
                .Include(c => c.TecnicoAsignado)
                .Include(c => c.Equipo)
                .Where(c => (c.Estado == "Programada" || c.Estado == "En Proceso") &&
                           c.FechaProgramada >= fechaLimite)
                .OrderBy(c => c.FechaProgramada)
                .ThenBy(c => c.HoraInicio)
                .ToListAsync();
        }

        public async Task<List<Cita>> GetCitasEnRangoAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            return await _context.Citas
                .Include(c => c.Cliente)
                .Include(c => c.TecnicoAsignado)
                .Include(c => c.Equipo)
                .Where(c => c.FechaProgramada >= fechaInicio && c.FechaProgramada <= fechaFin)
                .OrderBy(c => c.FechaProgramada)
                .ThenBy(c => c.HoraInicio)
                .ToListAsync();
        }

        public async Task<bool> CreateAsync(Cita cita)
        {
            try
            {
                cita.FechaCreacion = DateTime.Now;
                _context.Citas.Add(cita);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> UpdateAsync(Cita cita)
        {
            try
            {
                var existingCita = await _context.Citas
                    .FirstOrDefaultAsync(c => c.Id == cita.Id);

                if (existingCita == null)
                    return false;

                // Actualizar propiedades
                existingCita.ClienteId = cita.ClienteId;
                existingCita.EquipoId = cita.EquipoId;
                existingCita.TecnicoAsignadoId = cita.TecnicoAsignadoId;
                existingCita.FechaProgramada = cita.FechaProgramada;
                existingCita.HoraInicio = cita.HoraInicio;
                existingCita.HoraFin = cita.HoraFin;
                existingCita.TipoServicio = cita.TipoServicio;
                existingCita.Estado = cita.Estado;
                existingCita.Descripcion = cita.Descripcion;
                existingCita.CostoEstimado = cita.CostoEstimado;
                existingCita.CostoFinal = cita.CostoFinal;
                existingCita.NotificacionEnviada = cita.NotificacionEnviada;

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
                var cita = await _context.Citas
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (cita == null)
                    return false;

                // Soft delete - cambiar estado a Cancelada
                cita.Estado = "Cancelada";
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> ExistsConflictAsync(int tecnicoId, DateTime fechaInicio, DateTime fechaFin, int? excludeId = null)
        {
            var fecha = fechaInicio.Date;
            var horaInicio = fechaInicio.TimeOfDay;
            var horaFin = fechaFin.TimeOfDay;

            var query = _context.Citas
                .Where(c => c.TecnicoAsignadoId == tecnicoId &&
                           c.FechaProgramada.Date == fecha &&
                           (c.Estado == "Programada" || c.Estado == "En Proceso") &&
                           ((c.HoraInicio < horaFin && c.HoraFin > horaInicio)));

            if (excludeId.HasValue)
            {
                query = query.Where(c => c.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<List<Cita>> SearchAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetWithRelationsAsync();

            var term = searchTerm.ToLower().Trim();

            return await _context.Citas
                .Include(c => c.Cliente)
                .Include(c => c.TecnicoAsignado)
                .Include(c => c.Equipo)
                .Where(c => c.Cliente!.Nombre.ToLower().Contains(term) ||
                           c.Cliente!.Apellido.ToLower().Contains(term) ||
                           c.TipoServicio.ToLower().Contains(term) ||
                           c.Estado.ToLower().Contains(term) ||
                           (c.Descripcion != null && c.Descripcion.ToLower().Contains(term)) ||
                           (c.TecnicoAsignado != null &&
                            (c.TecnicoAsignado.Nombre.ToLower().Contains(term) ||
                             c.TecnicoAsignado.Apellido.ToLower().Contains(term))))
                .OrderByDescending(c => c.FechaProgramada)
                .ToListAsync();
        }

        public async Task<int> CountAsync()
        {
            return await _context.Citas.CountAsync();
        }

        public async Task<int> CountByEstadoAsync(string estado)
        {
            return await _context.Citas.CountAsync(c => c.Estado == estado);
        }
    }
}
