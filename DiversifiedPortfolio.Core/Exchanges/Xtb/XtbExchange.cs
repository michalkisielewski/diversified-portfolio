using DiversifiedPortfolio.Core.Constants;
using DiversifiedPortfolio.Core.Services;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiversifiedPortfolio.Core.Exchanges.Xtb
{
    public class XtbExchange : IExchange
    {
        private readonly XtbSettings _xtbSettings;
        private readonly ICrossrateProvider _crossrateProvider;

        public XtbExchange(IOptions<XtbSettings> xtbSettings, ICrossrateProvider crossrateProvider)
        {

            _xtbSettings = xtbSettings.Value;
            _crossrateProvider = crossrateProvider;
        }

        public async Task<ExchangeData> GetSummaryAsync(string currency = Currency.USD)
        {
            double rate = currency == Currency.USD ? 1 : await _crossrateProvider.GetCrossrateAsync(Currency.USD, currency);

            var instruments = new List<InstrumentData>
            {
                
            };

            var exchangeData = new ExchangeData
            {
                Name = "XTB",
                Instruments = instruments,
                NetAssetValue = 2915d * rate
            };

            return await Task.Run(() => exchangeData);
        }
    }
}
