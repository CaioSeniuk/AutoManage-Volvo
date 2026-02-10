using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoManage.Models
{
    /// <summary>
    /// representa um pedido de pecas
    /// </summary>
    public class PedidoPeca
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime DataPedido { get; set; }

        [Required]
        [StringLength(100)]
        public string NomeCliente { get; set; } = string.Empty;

        [StringLength(18)]
        public string? CPF_CNPJ { get; set; }

        [StringLength(20)]
        public string? Telefone { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ValorTotal { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Pendente"; // pendente, aprovado, entregue, cancelado

        public int? VendedorId { get; set; }

        [ForeignKey("VendedorId")]
        public Vendedor? Vendedor { get; set; }

        // relacionamento: um pedido tem varios itens
        public ICollection<ItemPedidoPeca> Itens { get; set; } = new List<ItemPedidoPeca>();
    }
}
