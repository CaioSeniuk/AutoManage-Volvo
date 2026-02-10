using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoManage.Models
{
    /// <summary>
    /// representa uma peca genuina volvo
    /// </summary>
    public class Peca
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string CodigoPeca { get; set; } = string.Empty; // ex: "voe12345678"

        [Required]
        [StringLength(200)]
        public string Nome { get; set; } = string.Empty; // ex: "filtro de oleo d13"

        [StringLength(500)]
        public string? Descricao { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Preco { get; set; }

        [Required]
        public int EstoqueAtual { get; set; }

        public int EstoqueMinimo { get; set; } = 5;

        [StringLength(100)]
        public string? Categoria { get; set; } // ex: "filtros", "freios", "motor"

        [StringLength(500)]
        public string? ModelosCompativeis { get; set; } // ex: "fh16,fm,fmx" (separado por virgula)

        // relacionamento: uma peca pode estar em varios pedidos
        public ICollection<ItemPedidoPeca> ItensPedido { get; set; } = new List<ItemPedidoPeca>();
    }
}
