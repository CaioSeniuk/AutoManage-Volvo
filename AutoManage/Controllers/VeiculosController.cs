using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoManage.Data;
using AutoManage.Models;
using AutoManage.Validation;
using Microsoft.AspNetCore.Http;

namespace AutoManage.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class VeiculosController : ControllerBase
    {
        private readonly AutoManageContext _context;

        public VeiculosController(AutoManageContext context)
        {
            _context = context;
        }

        /// <summary>
        /// lista todos os caminhoes volvo com filtros opcionais
        /// </summary>
        /// <param name="versaoMotor">filtro por versao do motor</param>
        /// <param name="page">numero da pagina (padrao 1)</param>
        /// <param name="limit">limite de itens por pagina (padrao 10)</param>
        /// <returns>lista de caminhoes ordenada por quilometragem</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<Veiculo>>> GetVeiculos(
            [FromQuery] string? versaoMotor = null,
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10)
        {
            try
            {
                // LINQ: Filtro opcional e ordenação por quilometragem
                var query = _context.Veiculos.AsQueryable();

                if (!string.IsNullOrEmpty(versaoMotor))
                {
                    query = query.Where(v => v.VersaoMotor == versaoMotor);
                }

                var veiculos = await query
                    .OrderBy(v => v.Quilometragem)
                    .Skip((page - 1) * limit)
                    .Take(limit)
                    .ToListAsync();

                return Ok(veiculos);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Erro interno ao processar a solicitação." });
            }
        }

        /// <summary>
        /// busca um caminhao volvo especifico por chassi (com dados do proprietario)
        /// </summary>
        /// <param name="chassi">chassi do caminhao</param>
        /// <returns>caminhao com dados completos do proprietario</returns>
        [HttpGet("{chassi}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Veiculo>> GetVeiculo(string chassi)
        {
            try
            {
                // LINQ: Include para buscar dados relacionados do proprietário
                var veiculo = await _context.Veiculos
                    .Include(v => v.Proprietario)
                    .FirstOrDefaultAsync(v => v.Chassi == chassi);

                if (veiculo == null)
                {
                    return NotFound(new { message = "Caminhão não encontrado" });
                }

                return Ok(veiculo);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Erro interno ao buscar veículo." });
            }
        }

        /// <summary>
        /// cria um novo veiculo
        /// </summary>
        /// <param name="veiculo">dados do veiculo</param>
        /// <returns>veiculo criado</returns>
        /// <response code="201">Veículo criado com sucesso</response>
        /// <response code="400">Dados inválidos ou chassi duplicado</response>
        /// <response code="500">Erro interno do servidor</response>
        [HttpPost]
        [ProducesResponseType(typeof(Veiculo), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Veiculo>> PostVeiculo(Veiculo veiculo)
        {
            try
            {
                // Chain of Responsibility: Configuração da Cadeia
                var chassiHandler = new ChassiUnicoHandler(_context);
                var proprietarioHandler = new ProprietarioExistenteHandler(_context);

                // Define a ordem: Primeiro verifica Chassi -> Depois verifica Proprietário
                chassiHandler.SetProximo(proprietarioHandler);

                // Executa a cadeia
                // Se algum falhar, uma exceção será lançada e capturada abaixo
                await chassiHandler.Validar(veiculo);

                _context.Veiculos.Add(veiculo);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetVeiculo), new { chassi = veiculo.Chassi }, veiculo);
            }
            catch (InvalidOperationException ex)
            {
                // Captura erros de validação de negócio da cadeia (ex: Chassi duplicado)
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Erro interno ao criar veículo." });
            }
        }

        /// <summary>
        /// atualiza um veiculo existente
        /// </summary>
        /// <param name="chassi">chassi do veiculo</param>
        /// <param name="veiculo">dados atualizados</param>
        /// <returns>sem conteudo</returns>
        [HttpPut("{chassi}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PutVeiculo(string chassi, Veiculo veiculo)
        {
            try
            {
                if (chassi != veiculo.Chassi)
                {
                    return BadRequest(new { message = "Chassi não corresponde" });
                }

                // Validar se o proprietário existe (se informado)
                if (veiculo.ProprietarioId.HasValue)
                {
                    var proprietarioExiste = await _context.Proprietarios
                        .AnyAsync(p => p.Id == veiculo.ProprietarioId.Value);

                    if (!proprietarioExiste)
                    {
                        return BadRequest(new { message = "Proprietário não encontrado" });
                    }
                }

                _context.Entry(veiculo).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await VeiculoExists(chassi))
                    {
                        return NotFound(new { message = "Veículo não encontrado" });
                    }
                    throw;
                }

                return NoContent();
            }
            catch (Exception ex) when (ex is not DbUpdateConcurrencyException)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Erro interno ao atualizar veículo." });
            }
        }

        /// <summary>
        /// deleta um veiculo
        /// </summary>
        /// <param name="chassi">chassi do veiculo</param>
        /// <returns>sem conteudo</returns>
        [HttpDelete("{chassi}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteVeiculo(string chassi)
        {
            try
            {
                var veiculo = await _context.Veiculos.FindAsync(chassi);
                if (veiculo == null)
                {
                    return NotFound(new { message = "Veículo não encontrado" });
                }

                // Verificar se há vendas associadas
                var temVendas = await _context.Vendas.AnyAsync(v => v.VeiculoId == chassi);
                if (temVendas)
                {
                    return BadRequest(new { message = "Não é possível deletar veículo com vendas registradas" });
                }

                _context.Veiculos.Remove(veiculo);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Erro interno ao deletar veículo." });
            }
        }

        private async Task<bool> VeiculoExists(string chassi)
        {
            return await _context.Veiculos.AnyAsync(e => e.Chassi == chassi);
        }
    }
}