using System.Text.Json.Serialization;

namespace AppCompras.Models.ViewModels
{
    public class ItemCompraViewModel
    {
        [JsonPropertyName("Cod.")]
        public int Id { get; set; }

        [JsonPropertyName("descricao")]
        public string? Description { get; set; }

        [JsonPropertyName("LineNum")]
        public int? LineNum { get; set; }

        // Menor Valor (MV)
        [JsonPropertyName("mv_cod_pc")]
        public int? MvCodPc { get; set; }

        [JsonPropertyName("mv_preco")]
        public decimal? MvPreco { get; set; }

        [JsonPropertyName("mv_fornecedor")]
        public string? MvFornecedor { get; set; }

        [JsonPropertyName("mv_qtd")]
        public decimal? MvQuantidade { get; set; }

        [JsonPropertyName("mv_data")]
        public DateTime? MvData { get; set; }

        // Última Compra (UV)
        [JsonPropertyName("uv_cod_pc")]
        public int? UvCodPc { get; set; }

        [JsonPropertyName("uv_preco")]
        public decimal? UvPreco { get; set; }

        [JsonPropertyName("uv_fornecedor")]
        public string? UvFornecedor { get; set; }

        [JsonPropertyName("uv_data")]
        public DateTime? UvData { get; set; }

        [JsonPropertyName("uv_qtd")]
        public decimal? UvQuantidade { get; set; }

        // Média Ponderada
        [JsonPropertyName("media_ponderada")]
        public decimal? MediaPonderada { get; set; }
    }
}