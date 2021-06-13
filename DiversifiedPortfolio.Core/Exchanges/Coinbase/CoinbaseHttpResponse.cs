namespace DiversifiedPortfolio.Core.Exchanges.Coinbase
{
    internal class CoinbaseHttpResponse<T>
    {
        public Pagination Pagination { get; set; }
        public T Data { get; set; }
    }
}
