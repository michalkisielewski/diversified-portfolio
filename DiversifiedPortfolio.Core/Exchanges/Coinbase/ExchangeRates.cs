using System.Collections.Generic;

namespace DiversifiedPortfolio.Core.Exchanges.Coinbase
{
    internal class ExchangeRates
    {
        public string Currency { get; set; }
        public Dictionary<string, double> Rates { get; set; }
    }
}
