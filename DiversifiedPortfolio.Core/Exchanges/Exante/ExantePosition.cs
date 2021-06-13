namespace DiversifiedPortfolio.Core.Exchanges.Exante
{
    internal class ExantePosition
    {
        public string Id { get; set; }
        public string SymbolType { get; set; }
        public string Currency { get; set; }
        public double Value { get; set; }
        public double Price { get; set; }
        public double? AveragePrice { get; set; }
        public double ConvertedPnl { get; set; }
        public double Quantity { get; set; }
        public double Pnl { get; set; }
        public double ConvertedValue { get; set; }
    }
}
