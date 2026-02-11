using AutoManage.Data;
using AutoManage.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoManage.Validation
{

    /// Classe base para a Cadeia de Responsabilidade (Chain of Responsibility).
    /// Define o contrato para os validadores de Veículo.
    public abstract class VeiculoHandler
    {
        protected VeiculoHandler? _proximo;
        protected readonly AutoManageContext _context;

        public VeiculoHandler(AutoManageContext context)
        {
            _context = context;
        }

        /// Define o próximo elo da corrente.
        /// Retorna o próximo handler para permitir configuração fluente.
        public VeiculoHandler SetProximo(VeiculoHandler proximo)
        {
            _proximo = proximo;
            return proximo;
        }

       
        /// Executa a validação e passa para o próximo se tiver sucesso.
        public virtual async Task Validar(Veiculo veiculo)
        {
            if (_proximo != null)
            {
                await _proximo.Validar(veiculo);
            }
        }
    }
}
