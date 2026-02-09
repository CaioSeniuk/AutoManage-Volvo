using AutoManage.Data;
using AutoManage.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoManage.Validation
{
    /// <summary>
    /// Classe base para a Cadeia de Responsabilidade (Chain of Responsibility).
    /// Define o contrato para os validadores de Veículo.
    /// </summary>
    public abstract class VeiculoHandler
    {
        protected VeiculoHandler? _proximo;
        protected readonly AutoManageContext _context;

        public VeiculoHandler(AutoManageContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Define o próximo elo da corrente.
        /// Retorna o próximo handler para permitir configuração fluente.
        /// </summary>
        public VeiculoHandler SetProximo(VeiculoHandler proximo)
        {
            _proximo = proximo;
            return proximo;
        }

        /// <summary>
        /// Executa a validação e passa para o próximo se tiver sucesso.
        /// </summary>
        public virtual async Task Validar(Veiculo veiculo)
        {
            if (_proximo != null)
            {
                await _proximo.Validar(veiculo);
            }
        }
    }
}
