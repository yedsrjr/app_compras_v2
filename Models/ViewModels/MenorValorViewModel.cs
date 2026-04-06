using System.Text.Json.Serialization;

namespace AppCompras.Models.ViewModels
{
    public class MenorValorViewModel
    {
        [JsonPropertyName("Cod_PC")]
        public int CodPc { get; set; }

        [JsonPropertyName("descricao")]
        public string? Description { get; set; }

        [JsonPropertyName("mv_fornecedor")]
        public string? Fornecedor { get; set; }

        [JsonPropertyName("data_pc")]
        public DateTime? DataPc { get; set; }

        [JsonPropertyName("qtd_pc")]
        public decimal? Quantidade { get; set; }

        [JsonPropertyName("preco_unit")]
        public decimal? PrecoUnitario { get; set; }

        [JsonPropertyName("valor_total_item")]
        public decimal? ValorTotalItem { get; set; }
    }
}
