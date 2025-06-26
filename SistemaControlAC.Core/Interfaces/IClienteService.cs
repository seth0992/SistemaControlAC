using SistemaControlAC.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaControlAC.Core.Interfaces
{
    public interface IClienteService
    {
        Task<(bool Success, string Message, Cliente? Cliente)> CreateAsync(Cliente cliente);
        Task<(bool Success, string Message, Cliente? Cliente)> UpdateAsync(Cliente cliente);
        Task<(bool Success, string Message)> DeleteAsync(int id);
        Task<Cliente?> GetByIdAsync(int id);
        Task<List<Cliente>> GetAllAsync();
        Task<List<Cliente>> GetActiveAsync();
        Task<List<Cliente>> SearchAsync(string searchTerm);
        Task<Cliente?> GetWithEquiposAsync(int id);
        Task<List<Cliente>> GetWithEquiposAsync();
        Task<bool> ValidateClienteAsync(Cliente cliente, bool isUpdate = false);
        Task<string> GenerateClienteCodeAsync();
        Task<int> CountAsync();
        Task<int> CountActiveAsync();
    }
}
