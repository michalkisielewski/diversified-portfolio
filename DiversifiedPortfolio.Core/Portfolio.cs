using DiversifiedPortfolio.Core.Constants;
using DiversifiedPortfolio.Core.Exchanges;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiversifiedPortfolio.Core
{
    public class Portfolio
    {
        private readonly IEnumerable<IExchange> exchanges;

        public Portfolio(IEnumerable<IExchange> exchanges)
        {
            this.exchanges = exchanges;
        }

        public async Task<List<ExchangeData>> GetExchangesData(string currency = Currency.USD)
        {
            var exchangeDatas = new List<ExchangeData>();
            foreach (var exchange in exchanges)
            {
                exchangeDatas.Add(await exchange.GetSummaryAsync(currency));
            }

            return exchangeDatas;
        }
    }

    public class ExchangeData
    {
        public string Name { get; set; }
        public double NetAssetValue { get; set; }
        public List<InstrumentData> Instruments { get; set; }
    }

    public class InstrumentData
    {
        public string Name { get; set; }
        public double Value { get; set; }
        public double CurrentPrice { get; set; }
        public double AveragePrice { get; set; }
        public double Quantity { get; set; }
        public string SymbolType { get; set; }
    }
}
