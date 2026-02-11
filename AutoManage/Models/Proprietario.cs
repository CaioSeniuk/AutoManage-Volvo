using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AutoManage.Models
{
    /// <summary>
    /// representa um proprietario de veiculo
    /// </summary>
    public class Proprietario
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [StringLength(18)]
        [JsonPropertyName("cpfCnpj")]
        public string CPF_CNPJ { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Endereco { get; set; }

        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(20)]
        public string? Telefone { get; set; }

        [StringLength(500)]
        public string? DadosPessoais { get; set; }

        // relacionamento: um proprietario pode ter varios veiculos
        public ICollection<Veiculo> Veiculos { get; set; } = new List<Veiculo>();
    }
}
