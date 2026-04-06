namespace AppCompras.Models.ViewModels
{
    public class ComprasDashboardViewModel
    {
        public string TipoBusca { get; set; } = string.Empty;
        public string ValorBusca { get; set; } = string.Empty;
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public List<MenorValorViewModel> MenorValorItems { get; set; } = new();
    }
}
