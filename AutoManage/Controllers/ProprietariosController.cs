using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoManage.Data;
using AutoManage.Models;

namespace AutoManage.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ProprietariosController : ControllerBase
    {
        private readonly AutoManageContext _context;

        public ProprietariosController(AutoManageContext context)
        {
            _context = context;
        }

        /// <summary>
        /// lista todos os proprietarios
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Proprietario>>> GetProprietarios()
        {
            var proprietarios = await _context.Proprietarios.ToListAsync();
            return Ok(proprietarios);
        }

        /// <summary>
        /// busca um proprietario especifico por id
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Proprietario>> GetProprietario(int id)
        {
            var proprietario = await _context.Proprietarios
                .Include(p => p.Veiculos)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (proprietario == null)
            {
                return NotFound(new { message = "Proprietário não encontrado" });
            }

            return Ok(proprietario);
        }

        /// <summary>
        /// cria um novo proprietario
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Proprietario>> PostProprietario(Proprietario proprietario)
        {
            // Verificar se CPF/CNPJ já existe
            var cpfExiste = await _context.Proprietarios
                .AnyAsync(p => p.CPF_CNPJ == proprietario.CPF_CNPJ);

            if (cpfExiste)
            {
                return BadRequest(new { message = "CPF/CNPJ já cadastrado" });
            }

            _context.Proprietarios.Add(proprietario);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProprietario), new { id = proprietario.Id }, proprietario);
        }

        /// <summary>
        /// atualiza um proprietario existente
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProprietario(int id, Proprietario proprietario)
        {
            if (id != proprietario.Id)
            {
                return BadRequest(new { message = "ID não corresponde" });
            }

            // Verificar se CPF/CNPJ já existe em outro registro
            var cpfExiste = await _context.Proprietarios
                .AnyAsync(p => p.CPF_CNPJ == proprietario.CPF_CNPJ && p.Id != id);

            if (cpfExiste)
            {
                return BadRequest(new { message = "CPF/CNPJ já cadastrado para outro proprietário" });
            }

            _context.Entry(proprietario).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ProprietarioExists(id))
                {
                    return NotFound(new { message = "Proprietário não encontrado" });
                }
                throw;
            }

            return NoContent();
        }

        /// <summary>
        /// deleta um proprietario
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProprietario(int id)
        {
            var proprietario = await _context.Proprietarios.FindAsync(id);
            if (proprietario == null)
            {
                return NotFound(new { message = "Proprietário não encontrado" });
            }

            _context.Proprietarios.Remove(proprietario);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task<bool> ProprietarioExists(int id)
        {
            return await _context.Proprietarios.AnyAsync(e => e.Id == id);
        }
    }
}
