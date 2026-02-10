using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoManage.Data;
using AutoManage.Models;

namespace AutoManage.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class PecasController : ControllerBase
    {
        private readonly AutoManageContext _context;

        public PecasController(AutoManageContext context)
        {
            _context = context;
        }

        /// <summary>
        /// lista todas as pecas genuinas volvo
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Peca>>> GetPecas([FromQuery] string? categoria = null, [FromQuery] bool? estoqueBaixo = null)
        {
            var query = _context.Pecas.AsQueryable();

            if (!string.IsNullOrEmpty(categoria))
            {
                query = query.Where(p => p.Categoria == categoria);
            }

            if (estoqueBaixo == true)
            {
                query = query.Where(p => p.EstoqueAtual <= p.EstoqueMinimo);
            }

            var pecas = await query.OrderBy(p => p.Nome).ToListAsync();
            return Ok(pecas);
        }

        /// <summary>
        /// busca uma peca por id
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Peca>> GetPeca(int id)
        {
            var peca = await _context.Pecas.FindAsync(id);

            if (peca == null)
            {
                return NotFound(new { message = "Peça não encontrada" });
            }

            return Ok(peca);
        }

        /// <summary>
        /// busca pecas compativeis com um modelo de caminhao
        /// </summary>
        [HttpGet("compativeis/{modelo}")]
        public async Task<ActionResult<IEnumerable<Peca>>> GetPecasCompativeis(string modelo)
        {
            var pecas = await _context.Pecas
                .Where(p => p.ModelosCompativeis != null && p.ModelosCompativeis.Contains(modelo))
                .OrderBy(p => p.Categoria)
                .ThenBy(p => p.Nome)
                .ToListAsync();

            return Ok(pecas);
        }

        /// <summary>
        /// cria uma nova peca
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Peca>> PostPeca(Peca peca)
        {
            // Verificar se código já existe
            var codigoExiste = await _context.Pecas
                .AnyAsync(p => p.CodigoPeca == peca.CodigoPeca);

            if (codigoExiste)
            {
                return BadRequest(new { message = "Código de peça já cadastrado" });
            }

            _context.Pecas.Add(peca);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPeca), new { id = peca.Id }, peca);
        }

        /// <summary>
        /// atualiza uma peca
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPeca(int id, Peca peca)
        {
            if (id != peca.Id)
            {
                return BadRequest(new { message = "ID não corresponde" });
            }

            _context.Entry(peca).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await PecaExists(id))
                {
                    return NotFound(new { message = "Peça não encontrada" });
                }
                throw;
            }

            return NoContent();
        }

        /// <summary>
        /// atualiza estoque de uma peca
        /// </summary>
        [HttpPatch("{id}/estoque")]
        public async Task<IActionResult> AtualizarEstoque(int id, [FromBody] int quantidade)
        {
            var peca = await _context.Pecas.FindAsync(id);
            if (peca == null)
            {
                return NotFound(new { message = "Peça não encontrada" });
            }

            peca.EstoqueAtual += quantidade;

            if (peca.EstoqueAtual < 0)
            {
                return BadRequest(new { message = "Estoque não pode ser negativo" });
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                pecaId = peca.Id,
                nome = peca.Nome,
                estoqueAtual = peca.EstoqueAtual,
                estoqueMinimo = peca.EstoqueMinimo,
                alerta = peca.EstoqueAtual <= peca.EstoqueMinimo ? "Estoque baixo!" : null
            });
        }

        /// <summary>
        /// deleta uma peca
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePeca(int id)
        {
            var peca = await _context.Pecas.FindAsync(id);
            if (peca == null)
            {
                return NotFound(new { message = "Peça não encontrada" });
            }

            _context.Pecas.Remove(peca);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task<bool> PecaExists(int id)
        {
            return await _context.Pecas.AnyAsync(e => e.Id == id);
        }
    }
}
