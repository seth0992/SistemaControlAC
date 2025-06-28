using SistemaControlAC.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaControlAC.Core.Interfaces
{
    public interface IEquipoRepository
    {
        Task<EquipoAireAcondicionado?> GetByIdAsync(int id);
        Task<List<EquipoAireAcondicionado>> GetAllAsync();
        Task<List<EquipoAireAcondicionado>> GetActiveAsync();
        Task<List<EquipoAireAcondicionado>> GetByClienteAsync(int clienteId);
        Task<bool> CreateAsync(EquipoAireAcondicionado equipo);
        Task<bool> UpdateAsync(EquipoAireAcondicionado equipo);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(string numeroSerie, int? excludeId = null);
        Task<List<EquipoAireAcondicionado>> SearchAsync(string searchTerm);
        Task<EquipoAireAcondicionado?> GetWithRelationsAsync(int id);
        Task<List<EquipoAireAcondicionado>> GetWithRelationsAsync();
        Task<int> CountAsync();
        Task<int> CountActiveAsync();
        Task<int> CountByClienteAsync(int clienteId);
    }
}