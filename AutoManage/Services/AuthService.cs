using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoManage.Data;
using AutoManage.Models;
using AutoManage.Models.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace AutoManage.Services
{
    /// <summary>
    /// Implementação do serviço de autenticação com JWT e BCrypt
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly AutoManageContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(AutoManageContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        /// <summary>
        /// Realiza login validando credenciais e retorna token JWT
        /// </summary>
        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            // Buscar usuário pelo username
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (usuario == null)
            {
                return null; // Usuário não encontrado
            }

            // Verificar senha usando BCrypt
            bool senhaValida = BCrypt.Net.BCrypt.Verify(request.Password, usuario.PasswordHash);
            
            if (!senhaValida)
            {
                return null; // Senha incorreta
            }

            // Gerar token JWT
            var token = GenerateJwtToken(usuario);
            var expirationHours = _configuration.GetValue<int>("JwtSettings:ExpirationHours", 24);

            return new LoginResponse
            {
                Token = token,
                Username = usuario.Username,
                Role = usuario.Role,
                ExpiresAt = DateTime.UtcNow.AddHours(expirationHours)
            };
        }

        /// <summary>
        /// Registra um novo usuário com senha criptografada
        /// </summary>
        public async Task<bool> RegisterAsync(RegisterRequest request)
        {
            // Verificar se username já existe
            var usuarioExiste = await _context.Usuarios
                .AnyAsync(u => u.Username == request.Username);

            if (usuarioExiste)
            {
                return false; // Username já cadastrado
            }

            // Criar hash da senha usando BCrypt
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var novoUsuario = new Usuario
            {
                Username = request.Username,
                PasswordHash = passwordHash,
                Role = request.Role
            };

            _context.Usuarios.Add(novoUsuario);
            await _context.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// Gera token JWT com claims do usuário
        /// </summary>
        public string GenerateJwtToken(Usuario usuario)
        {
            var secretKey = _configuration["JwtSettings:SecretKey"] 
                ?? throw new InvalidOperationException("JWT SecretKey não configurada");
            var issuer = _configuration["JwtSettings:Issuer"];
            var audience = _configuration["JwtSettings:Audience"];
            var expirationHours = _configuration.GetValue<int>("JwtSettings:ExpirationHours", 24);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Username),
                new Claim(ClaimTypes.Role, usuario.Role)
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(expirationHours),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
