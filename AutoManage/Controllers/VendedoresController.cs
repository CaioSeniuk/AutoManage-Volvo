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
    public class VendedoresController : ControllerBase
    {
        private readonly AutoManageContext _context;

        public VendedoresController(AutoManageContext context)
        {
            _context = context;
        }

        /// <summary>
        /// lista todos os vendedores
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Vendedor>>> GetVendedores()
        {
            var vendedores = await _context.Vendedores.ToListAsync();
            return Ok(vendedores);
        }

        /// <summary>
        /// busca um vendedor especifico por id
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Vendedor>> GetVendedor(int id)
        {
            var vendedor = await _context.Vendedores.FindAsync(id);

            if (vendedor == null)
            {
                return NotFound(new { message = "Vendedor não encontrado" });
            }

            return Ok(vendedor);
        }

        /// <summary>
        /// calcula o salario final do vendedor (salario base + 1% sobre vendas do mes)
        /// </summary>
        /// <param name="id">id do vendedor</param>
        /// <param name="mes">mes (1-12)</param>
        /// <param name="ano">ano</param>
        /// <returns>dados de comissao do vendedor</returns>
        [HttpGet("{id}/comissoes")]
        public async Task<ActionResult> GetComissoes(int id, [FromQuery] int mes, [FromQuery] int ano)
        {
            var vendedor = await _context.Vendedores.FindAsync(id);
            if (vendedor == null)
            {
                return NotFound(new { message = "Vendedor não encontrado" });
            }

            // Validar mês e ano
            if (mes < 1 || mes > 12)
            {
                return BadRequest(new { message = "Mês inválido (1-12)" });
            }

            if (ano < 2000 || ano > DateTime.Now.Year)
            {
                return BadRequest(new { message = "ano inválido" });
            }

            // LINQ: Calcular total de vendas do mês
            var totalVendas = await _context.Vendas
                .Where(v => v.VendedorId == id &&
                           v.DataVenda.Month == mes &&
                           v.DataVenda.Year == ano)
                .SumAsync(v => v.ValorFinal);

            // Calcular comissão (1% sobre vendas)
            var comissao = totalVendas * 0.01m;

            // Salário final = Salário Base + Comissão
            var salarioFinal = vendedor.SalarioBase + comissao;

            var resultado = new
            {
                vendedorId = vendedor.Id,
                vendedorNome = vendedor.Nome,
                mes = mes,
                ano = ano,
                salarioBase = vendedor.SalarioBase,
                totalVendas = totalVendas,
                comissao = comissao,
                salarioFinal = salarioFinal
            };

            return Ok(resultado);
        }

        /// <summary>
        /// cria um novo vendedor
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Vendedor>> PostVendedor(Vendedor vendedor)
        {
            if (vendedor.SalarioBase <= 0)
            {
                return BadRequest(new { message = "Salário base deve ser maior que zero" });
            }

            _context.Vendedores.Add(vendedor);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetVendedor), new { id = vendedor.Id }, vendedor);
        }

        /// <summary>
        /// atualiza um vendedor existente
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutVendedor(int id, Vendedor vendedor)
        {
            if (id != vendedor.Id)
            {
                return BadRequest(new { message = "ID não corresponde" });
            }

            if (vendedor.SalarioBase <= 0)
            {
                return BadRequest(new { message = "Salário base deve ser maior que zero" });
            }

            _context.Entry(vendedor).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await VendedorExists(id))
                {
                    return NotFound(new { message = "Vendedor não encontrado" });
                }
                throw;
            }

            return NoContent();
        }

        /// <summary>
        /// deleta um vendedor
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVendedor(int id)
        {
            var vendedor = await _context.Vendedores.FindAsync(id);
            if (vendedor == null)
            {
                return NotFound(new { message = "Vendedor não encontrado" });
            }

            // Verificar se há vendas associadas
            var temVendas = await _context.Vendas.AnyAsync(v => v.VendedorId == id);
            if (temVendas)
            {
                return BadRequest(new { message = "Não é possível deletar vendedor com vendas registradas" });
            }

            _context.Vendedores.Remove(vendedor);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task<bool> VendedorExists(int id)
        {
            return await _context.Vendedores.AnyAsync(e => e.Id == id);
        }
    }
}
