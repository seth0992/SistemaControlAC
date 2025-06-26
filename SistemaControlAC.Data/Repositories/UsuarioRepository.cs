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
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly ApplicationDbContext _context;

        public UsuarioRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Usuario?> GetByUsernameAsync(string username)
        {
            return await _context.Usuarios
                .FirstOrDefaultAsync(u => u.NombreUsuario == username && u.Activo);
        }

        public async Task<Usuario?> GetByIdAsync(int id)
        {
            return await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<bool> CreateAsync(Usuario usuario)
        {
            try
            {
                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateAsync(Usuario usuario)
        {
            try
            {
                _context.Usuarios.Update(usuario);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateLastAccessAsync(int userId)
        {
            try
            {
                var usuario = await GetByIdAsync(userId);
                if (usuario != null)
                {
                    usuario.UltimoAcceso = DateTime.Now;
                    return await UpdateAsync(usuario);
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ExistsAsync(string username)
        {
            return await _context.Usuarios
                .AnyAsync(u => u.NombreUsuario == username);
        }

        public async Task<List<Usuario>> GetAllAsync()
        {
            return await _context.Usuarios
                .Where(u => u.Activo)
                .ToListAsync();
        }
    }
}
