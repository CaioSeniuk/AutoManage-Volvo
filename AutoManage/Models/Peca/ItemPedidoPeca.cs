using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoManage.Models
{
    /// <summary>
    /// representa um item de um pedido de pecas
    /// </summary>
    public class ItemPedidoPeca
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PedidoPecaId { get; set; }

        [Required]
        public int PecaId { get; set; }

        [Required]
        public int Quantidade { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecoUnitario { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }

        // navegacao
        [ForeignKey("PedidoPecaId")]
        public PedidoPeca? PedidoPeca { get; set; }

        [ForeignKey("PecaId")]
        public Peca? Peca { get; set; }
    }
}
