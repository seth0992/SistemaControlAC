using SistemaControlAC.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaControlAC.Core.Interfaces
{
    public interface IClienteRepository
    {
        Task<Cliente?> GetByIdAsync(int id);
        Task<List<Cliente>> GetAllAsync();
        Task<List<Cliente>> GetActiveAsync();
        Task<bool> CreateAsync(Cliente cliente);
        Task<bool> UpdateAsync(Cliente cliente);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(string telefono, int? excludeId = null);
        Task<bool> ExistsEmailAsync(string email, int? excludeId = null);
        Task<List<Cliente>> SearchAsync(string searchTerm);
        Task<Cliente?> GetWithEquiposAsync(int id);
        Task<List<Cliente>> GetWithEquiposAsync();
        Task<int> CountAsync();
        Task<int> CountActiveAsync();
    }
}
