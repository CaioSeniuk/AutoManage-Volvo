using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoManage.Models
{
    /// <summary>
    /// representa um caminhao volvo no inventario da concessionaria
    /// </summary>
    public class Veiculo
    {
        [Key]
        [StringLength(17)]
        public string Chassi { get; set; } = Guid.NewGuid().ToString("N").Substring(0, 17).ToUpper();

        [Required]
        [StringLength(100)]
        public string Modelo { get; set; } = string.Empty; // ex: fh16, fm, fmx, vnl, etc.

        [Required]
        public int Ano { get; set; }

        [StringLength(30)]
        public string? Cor { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Valor { get; set; } = 0;

        public int Quilometragem { get; set; }

        [StringLength(500)]
        public string? Equipamentos { get; set; } // ex: "i-shift, freio motor veb+, cruise control adaptativo"

        [StringLength(50)]
        public string? VersaoMotor { get; set; } // ex: "d13k 500cv", "d11k 450cv"

        [StringLength(100)]
        public string? Aplicacao { get; set; } // ex: "longa distancia", "construcao", "distribuicao urbana"

        // chave estrangeira para proprietario
        public int? ProprietarioId { get; set; }

        // navegacao para o proprietario
        [ForeignKey("ProprietarioId")]
        public Proprietario? Proprietario { get; set; }

        // relacionamento: um veiculo pode ter varias vendas (historico)
        public ICollection<Venda> Vendas { get; set; } = new List<Venda>();
    }
}
