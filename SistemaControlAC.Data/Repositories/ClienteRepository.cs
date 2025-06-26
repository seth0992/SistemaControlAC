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
    public class ClienteRepository : IClienteRepository
    {
        private readonly ApplicationDbContext _context;

        public ClienteRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Cliente?> GetByIdAsync(int id)
        {
            return await _context.Clientes
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<List<Cliente>> GetAllAsync()
        {
            return await _context.Clientes
                .OrderBy(c => c.Nombre)
                .ThenBy(c => c.Apellido)
                .ToListAsync();
        }

        public async Task<List<Cliente>> GetActiveAsync()
        {
            return await _context.Clientes
                .Where(c => c.Activo)
                .OrderBy(c => c.Nombre)
                .ThenBy(c => c.Apellido)
                .ToListAsync();
        }

        public async Task<bool> CreateAsync(Cliente cliente)
        {
            try
            {
                cliente.FechaRegistro = DateTime.Now;
                _context.Clientes.Add(cliente);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> UpdateAsync(Cliente cliente)
        {
            try
            {
                var existingCliente = await _context.Clientes
                    .FirstOrDefaultAsync(c => c.Id == cliente.Id);

                if (existingCliente == null)
                    return false;

                // Actualizar propiedades que existen en la BD
                existingCliente.Nombre = cliente.Nombre;
                existingCliente.Apellido = cliente.Apellido;
                existingCliente.Telefono = cliente.Telefono;
                existingCliente.Email = cliente.Email;
                existingCliente.Direccion = cliente.Direccion;
                existingCliente.Ciudad = cliente.Ciudad;
                existingCliente.CodigoPostal = cliente.CodigoPostal;
                existingCliente.RecibeNotificacionesWhatsApp = cliente.RecibeNotificacionesWhatsApp;
                existingCliente.RecibeNotificacionesEmail = cliente.RecibeNotificacionesEmail;
                existingCliente.Activo = cliente.Activo;
                existingCliente.Notas = cliente.Notas;

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
                var cliente = await _context.Clientes
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (cliente == null)
                    return false;

                // Soft delete - marcar como inactivo
                cliente.Activo = false;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> ExistsAsync(string telefono, int? excludeId = null)
        {
            var query = _context.Clientes.Where(c => c.Telefono == telefono);

            if (excludeId.HasValue)
            {
                query = query.Where(c => c.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<bool> ExistsEmailAsync(string email, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            var query = _context.Clientes.Where(c => c.Email == email);

            if (excludeId.HasValue)
            {
                query = query.Where(c => c.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<List<Cliente>> SearchAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetActiveAsync();

            var term = searchTerm.ToLower().Trim();

            return await _context.Clientes
                .Where(c => c.Activo &&
                    (c.Nombre.ToLower().Contains(term) ||
                     c.Apellido.ToLower().Contains(term) ||
                     c.Telefono.Contains(term) ||
                     (c.Email != null && c.Email.ToLower().Contains(term)) ||
                     c.Direccion.ToLower().Contains(term)))
                .OrderBy(c => c.Nombre)
                .ThenBy(c => c.Apellido)
                .ToListAsync();
        }

        public async Task<Cliente?> GetWithEquiposAsync(int id)
        {
            return await _context.Clientes
                .Include(c => c.Equipos)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<List<Cliente>> GetWithEquiposAsync()
        {
            return await _context.Clientes
                .Include(c => c.Equipos)
                .Where(c => c.Activo)
                .OrderBy(c => c.Nombre)
                .ThenBy(c => c.Apellido)
                .ToListAsync();
        }

        public async Task<int> CountAsync()
        {
            return await _context.Clientes.CountAsync();
        }

        public async Task<int> CountActiveAsync()
        {
            return await _context.Clientes.CountAsync(c => c.Activo);
        }
    }
}
