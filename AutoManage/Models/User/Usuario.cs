using System.ComponentModel.DataAnnotations;

namespace AutoManage.Models.User
{
    /// <summary>
    /// Representa um usuário do sistema para fins de autenticação.
    /// </summary>
    public class Usuario
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string PasswordHash { get; set; } = string.Empty;

        [StringLength(20)]
        public string Role { get; set; } = "Vendedor"; // ex: Admin, Vendedor
    }
}
