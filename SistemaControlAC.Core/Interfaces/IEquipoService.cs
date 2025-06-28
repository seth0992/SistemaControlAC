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





    }
}
