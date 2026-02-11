using AutoManage.Models;
using AutoManage.Models.Auth;

namespace AutoManage.Services
{
    /// <summary>
    /// Interface do serviço de autenticação
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Realiza login e retorna token JWT
        /// </summary>
        Task<LoginResponse?> LoginAsync(LoginRequest request);

        /// <summary>
        /// Registra um novo usuário
        /// </summary>
        Task<bool> RegisterAsync(RegisterRequest request);

        /// <summary>
        /// Gera token JWT para um usuário
        /// </summary>
        string GenerateJwtToken(Usuario usuario);
    }
}
