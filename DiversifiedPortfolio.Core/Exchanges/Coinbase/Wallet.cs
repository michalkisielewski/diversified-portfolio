namespace DiversifiedPortfolio.Core.Exchanges.Coinbase
{
    internal class Wallet
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public WalletBalance Balance { get; set; }
    }
}
