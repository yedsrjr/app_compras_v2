namespace AppCompras.Models.ViewModels
{
    public class ComprasViewModel
    {
        public List<ItemCompraViewModel> UltimaCompraItems { get; set; } = new();
        public List<MenorValorViewModel> MenorValorItems { get; set; } = new();
    }
}
