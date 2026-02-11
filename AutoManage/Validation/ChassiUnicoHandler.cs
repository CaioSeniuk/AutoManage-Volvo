using AutoManage.Data;
using AutoManage.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoManage.Validation
{
    
    /// Elo da corrente responsável por verificar se o Chassi é único no banco de dados.
    public class ChassiUnicoHandler : VeiculoHandler
    {
        public ChassiUnicoHandler(AutoManageContext context) : base(context) { }

        public override async Task Validar(Veiculo veiculo)
        {
            // Verifica se existe algum veículo com este chassi
            var existe = await _context.Veiculos
                .AnyAsync(v => v.Chassi == veiculo.Chassi);

            if (existe)
            {
                // Quebra a corrente com uma exceção de negócio
                throw new InvalidOperationException("Chassi já cadastrado no sistema.");
            }

            // Se passou, chama o base para continuar a corrente
            await base.Validar(veiculo);
        }
    }
}
