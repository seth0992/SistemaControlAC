using SistemaControlAC.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaControlAC.Core.Interfaces
{
    public interface IEquipoService
    {
        Task<(bool Success, string Message, EquipoAireAcondicionado? Equipo)> CreateAsync(EquipoAireAcondicionado equipo);
        Task<(bool Success, string Message, EquipoAireAcondicionado? Equipo)> UpdateAsync(EquipoAireAcondicionado equipo);
        Task<(bool Success, string Message)> DeleteAsync(int id);
        Task<EquipoAireAcondicionado?> GetByIdAsync(int id);
        Task<List<EquipoAireAcondicionado>> GetAllAsync();
        Task<List<EquipoAireAcondicionado>> GetActiveAsync();
        Task<List<EquipoAireAcondicionado>> GetByClienteAsync(int clienteId);
        Task<List<EquipoAireAcondicionado>> SearchAsync(string searchTerm);
        Task<EquipoAireAcondicionado?> GetWithClienteAsync(int id);
        Task<List<EquipoAireAcondicionado>> GetWithClienteAsync();
        Task<bool> ValidateEquipoAsync(EquipoAireAcondicionado equipo, bool isUpdate = false);
        Task<string> GenerateEquipoCodeAsync();
        Task<int> CountAsync();
        Task<int> CountActiveAsync();
        Task<int> CountByClienteAsync(int clienteId);
        Task<List<string>> GetTiposEquipoAsync();
        Task<List<string>> GetMarcasAsync();
    }
}