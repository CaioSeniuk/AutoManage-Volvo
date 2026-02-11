namespace AutoManage.Models.Auth
{
    /// <summary>
    /// DTO para resposta de login contendo o token JWT
    /// </summary>
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}
