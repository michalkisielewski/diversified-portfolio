namespace DiversifiedPortfolio.Core.Exchanges.Binance
{
    internal class Balance
    {
        public string Asset { get; set; }
        public double Free { get; set; }
        public double Locked { get; set; }
    }
}
