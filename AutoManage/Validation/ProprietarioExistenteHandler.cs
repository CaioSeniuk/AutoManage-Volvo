using AutoManage.Data;
using AutoManage.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoManage.Validation
{

    /// Elo da corrente responsável por verificar a integridade do Proprietário.
    public class ProprietarioExistenteHandler : VeiculoHandler
    {
        public ProprietarioExistenteHandler(AutoManageContext context) : base(context) { }

        public override async Task Validar(Veiculo veiculo)
        {
            // Só valida se um proprietário foi informado
            if (veiculo.ProprietarioId.HasValue)
            {
                var existe = await _context.Proprietarios
                    .AnyAsync(p => p.Id == veiculo.ProprietarioId.Value);

                if (!existe)
                {
                    throw new InvalidOperationException("O Proprietário informado não foi encontrado.");
                }
            }

            await base.Validar(veiculo);
        }
    }
}
