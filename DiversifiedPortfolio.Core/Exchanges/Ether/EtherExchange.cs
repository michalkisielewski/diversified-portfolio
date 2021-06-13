using DiversifiedPortfolio.Core.Constants;
using DiversifiedPortfolio.Core.Extensions;
using DiversifiedPortfolio.Core.Services;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace DiversifiedPortfolio.Core.Exchanges.Ether
{
    public class EtherExchange : IExchange
    {
        private readonly EtherSettings _settings;
        private readonly ICrossrateProvider _crossrateProvider;
        private readonly HttpClient _httpClient = new HttpClient();

        public EtherExchange(IOptions<EtherSettings> options, ICrossrateProvider crossrateProvider)
        {
            _settings = options.Value;
            _crossrateProvider = crossrateProvider;
            _httpClient.BaseAddress = new Uri(_settings.DataEndpoint);
        }

        public async Task<ExchangeData> GetSummaryAsync(string currency = Currency.USD)
        {
            const string holyheldContractAddress = "0x3FA729B4548beCBAd4EaB6EF18413470e6D5324C";
            const string optionRoomContractAddress = "0xad4f86a25bbc20ffb751f2fac312a0b4d8f88c64";
            
            var holyheldBalance = await RequestDataAsync($"?module=account&action=tokenbalance&contractaddress={holyheldContractAddress}");
            var optionRoomBalance = await RequestDataAsync($"?module=account&action=tokenbalance&contractaddress={optionRoomContractAddress}");
            //var etherBalance = await RequestDataAsync($"module=account&action=balance");

            //TODO:
            // get prices in usd
            // multiply balance with usd token price
            
            double rate = currency == Currency.USD ? 1 : await _crossrateProvider.GetCrossrateAsync(Currency.USD, currency);

            //mocked data
            var instruments = new List<InstrumentData>
            {
                new InstrumentData
                {
                    Name = "ROOM",
                    Value = 200 * rate,
                    SymbolType = SymbolType.Crypto
                },
                new InstrumentData
                {
                    Name = "HH",
                    Value = 500 * rate,
                    SymbolType = SymbolType.Crypto
                },
                //new InstrumentData
                //{
                //    Name = "ETH",
                //    Value = NormalizeBalance(etherBalance) * rate,
                //    SymbolType = SymbolType.Crypto
                //}
            };

            var exchangeData = new ExchangeData
            {
                Name = "Wallet",
                Instruments = instruments,
                NetAssetValue = instruments.Sum(i => i.Value)
            };

            return exchangeData;
        }

        public async Task<string> RequestDataAsync(string queryParams)
        {
            var httpResponse = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, queryParams + $"&address={_settings.AccountAddress}&tag=latest&apikey={_settings.ApiKey}"));
            var etherResponse = await HttpResponseMessageHelpers.GetResponseAsync<EtherHttpResponse>(httpResponse);

            return etherResponse.Result;
        }
    }

    internal class EtherHttpResponse
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public string Result { get; set; }
    }
}
