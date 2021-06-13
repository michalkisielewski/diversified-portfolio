using DiversifiedPortfolio.Core.Constants;
using DiversifiedPortfolio.Core.Extensions;
using DiversifiedPortfolio.Core.Services;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DiversifiedPortfolio.Core.Exchanges.Binance
{
    public class BinanceExchange : IExchange
    {
        private readonly BinanceSettings _settings;
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly ICrossrateProvider _crossrateProvider;

        public BinanceExchange(IOptions<BinanceSettings> settings, ICrossrateProvider crossrateProvider)
        {
            _settings = settings.Value;
            _httpClient.BaseAddress = new Uri(_settings.DataEndpoint);
            _httpClient.DefaultRequestHeaders.Add("X-MBX-APIKEY", _settings.ApiKey);
            _crossrateProvider = crossrateProvider;
        }

        public async Task<ExchangeData> GetSummaryAsync(string currency = Currency.USD)
        {
            var summary = await RequestAsync<BinanceSummary>(HttpMethod.Get, "/api/v3/account");
            var tickerPrices = (await RequestAsync<List<TickerPrice>>(HttpMethod.Get, "api/v3/ticker/price", addSignature: false))
                .ToDictionary((key) => key.Symbol, (value) => value.Price);

            double rate = currency == Currency.USD ? 1 : await _crossrateProvider.GetCrossrateAsync(Currency.USD, currency);

            var instruments = summary.Balances.Where(b => b.Free > 0.0)
                .ToList()
                .ConvertAll(balance =>
                {
                    var instrument = new InstrumentData
                    {
                        Name = balance.Asset,
                        Quantity = balance.Free,
                        CurrentPrice = GetCurrentPrice(balance, tickerPrices) * rate,
                        SymbolType = SymbolType.Crypto
                    };

                    instrument.Value = instrument.Quantity * instrument.CurrentPrice;
                    return instrument;
                })
                .ToList();

            return new ExchangeData
            {
                Name = "Binance",
                Instruments = instruments,
                NetAssetValue = instruments.Sum(i => i.Value)
            };

            static double GetCurrentPrice(Balance balance, Dictionary<string, double> tickerPrices)
            {
                string usdtPair = balance.Asset + "USDT";
                if (tickerPrices.ContainsKey(usdtPair))
                {
                    return tickerPrices[usdtPair];
                }

                // not all coins have pair with USDT, so calculate price based on BTC pair
                string btcPair = balance.Asset + "BTC";
                if (tickerPrices.ContainsKey(btcPair))
                {
                    string btcUsdtPair = "BTCUSDT";
                    return tickerPrices[btcPair] * tickerPrices[btcUsdtPair];
                }

                return 0d;
            }
        }

        private async Task<T> RequestAsync<T>(HttpMethod httpMethod, string path, bool addSignature = true)
        {
            string queryParams = "";

            if (addSignature)
            {
                var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                 queryParams = $"timestamp={timestamp}";

                var signature = CalculateSignature(queryParams);
                queryParams += $"&signature={signature}";

                queryParams = queryParams.Insert(0, "?");
            }

            var httpResponse = await _httpClient.SendAsync(
                new HttpRequestMessage(httpMethod, path + queryParams));
            var response = await HttpResponseMessageHelpers.GetResponseAsync<T>(httpResponse);

            return response;
        }

        private string CalculateSignature(string route)
        {
            var message = route;
            var hmac = new HMACSHA256(Encoding.ASCII.GetBytes(_settings.SecretKey));
            var hash = hmac.ComputeHash(Encoding.ASCII.GetBytes(message));
            var signature = BitConverter.ToString(hash).Replace("-", "").ToLower();

            return signature;
        }
    }
}
