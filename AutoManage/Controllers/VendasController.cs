using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoManage.Data;
using AutoManage.Models;
using Microsoft.AspNetCore.Authorization;

namespace AutoManage.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize] // Requer autenticação JWT
    public class VendasController : ControllerBase
    {
        private readonly AutoManageContext _context;

        public VendasController(AutoManageContext context)
        {
            _context = context;
        }

        /// <summary>
        /// lista todas as vendas
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Venda>>> GetVendas()
        {
            var vendas = await _context.Vendas
                .Include(v => v.Veiculo)
                .Include(v => v.Vendedor)
                .OrderByDescending(v => v.DataVenda)
                .ToListAsync();

            return Ok(vendas);
        }

        /// <summary>
        /// busca uma venda especifica por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Venda>> GetVenda(int id)
        {
            var venda = await _context.Vendas
                .Include(v => v.Veiculo)
                    .ThenInclude(ve => ve.Proprietario)
                .Include(v => v.Vendedor)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (venda == null)
            {
                return NotFound(new { message = "Venda não encontrada" });
            }

            return Ok(venda);
        }

        /// <summary>
        /// registra uma nova venda (associa veiculo ao vendedor)
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Venda>> PostVenda(Venda venda)
        {
            // Validar se o veículo existe
            var veiculo = await _context.Veiculos.FindAsync(venda.VeiculoId);
            if (veiculo == null)
            {
                return BadRequest(new { message = "Veículo não encontrado" });
            }

            // Validar se o vendedor existe
            var vendedor = await _context.Vendedores.FindAsync(venda.VendedorId);
            if (vendedor == null)
            {
                return BadRequest(new { message = "Vendedor não encontrado" });
            }

            // Validar se o veículo já foi vendido
            var veiculoJaVendido = await _context.Vendas
                .AnyAsync(v => v.VeiculoId == venda.VeiculoId);

            if (veiculoJaVendido)
            {
                return BadRequest(new { message = "Este veículo já foi vendido" });
            }

            // Validar valor final
            if (venda.ValorFinal <= 0)
            {
                return BadRequest(new { message = "Valor final deve ser maior que zero" });
            }

            // Definir data da venda se não informada
            if (venda.DataVenda == default)
            {
                venda.DataVenda = DateTime.Now;
            }

            _context.Vendas.Add(venda);
            await _context.SaveChangesAsync();

            // Retornar venda com dados completos
            var vendaCriada = await _context.Vendas
                .Include(v => v.Veiculo)
                .Include(v => v.Vendedor)
                .FirstOrDefaultAsync(v => v.Id == venda.Id);

            return CreatedAtAction(nameof(GetVenda), new { id = venda.Id }, vendaCriada);
        }

        /// <summary>
        /// deleta uma venda
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVenda(int id)
        {
            var venda = await _context.Vendas.FindAsync(id);
            if (venda == null)
            {
                return NotFound(new { message = "Venda não encontrada" });
            }

            _context.Vendas.Remove(venda);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
