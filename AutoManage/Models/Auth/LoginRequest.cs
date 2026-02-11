using System.ComponentModel.DataAnnotations;

namespace AutoManage.Models.Auth
{
    /// <summary>
    /// DTO para requisição de login
    /// </summary>
    public class LoginRequest
    {
        [Required(ErrorMessage = "Username é obrigatório")]
        [StringLength(50, ErrorMessage = "Username deve ter no máximo 50 caracteres")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password é obrigatório")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password deve ter entre 6 e 100 caracteres")]
        public string Password { get; set; } = string.Empty;
    }
}
