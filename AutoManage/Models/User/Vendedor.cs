using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoManage.Models.User
{
    /// <summary>
    /// representa um vendedor da concessionaria
    /// </summary>
    public class Vendedor
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal SalarioBase { get; set; }

        // relacionamento: um vendedor pode ter varias vendas
        public ICollection<Venda> Vendas { get; set; } = new List<Venda>();
    }
}
