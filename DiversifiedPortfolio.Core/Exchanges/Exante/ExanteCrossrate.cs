namespace DiversifiedPortfolio.Core.Exchanges.Exante
{
    public partial class ExanteExchange
    {
        internal class ExanteCrossrate
        {
            public string SymbolId { get; set; }
            public double Rate { get; set; }
            public string Pair { get; set; }
        }
    }
}
