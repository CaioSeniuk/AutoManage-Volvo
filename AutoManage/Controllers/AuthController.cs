using AutoManage.Models.Auth;
using AutoManage.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoManage.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [AllowAnonymous] // Permite acesso sem autenticação
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Realiza login e retorna token JWT
        /// </summary>
        /// <param name="request">Credenciais de login</param>
        /// <returns>Token JWT e informações do usuário</returns>
        /// <response code="200">Login realizado com sucesso</response>
        /// <response code="401">Credenciais inválidas</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _authService.LoginAsync(request);

            if (response == null)
            {
                return Unauthorized(new { message = "Usuário ou senha inválidos" });
            }

            return Ok(response);
        }

        /// <summary>
        /// Registra um novo usuário
        /// </summary>
        /// <param name="request">Dados do novo usuário</param>
        /// <returns>Confirmação de registro</returns>
        /// <response code="200">Usuário registrado com sucesso</response>
        /// <response code="400">Username já existe ou dados inválidos</response>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var sucesso = await _authService.RegisterAsync(request);

            if (!sucesso)
            {
                return BadRequest(new { message = "Username já existe" });
            }

            return Ok(new { message = "Usuário registrado com sucesso" });
        }
    }
}
