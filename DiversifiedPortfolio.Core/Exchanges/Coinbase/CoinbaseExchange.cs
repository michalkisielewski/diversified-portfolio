using DiversifiedPortfolio.Core.Constants;
using DiversifiedPortfolio.Core.Extensions;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DiversifiedPortfolio.Core.Exchanges.Coinbase
{
    public class CoinbaseExchange : IExchange
    {
        private readonly CoinbaseSettings _settings;
        private readonly HttpClient _httpClient = new HttpClient();

        public CoinbaseExchange(IOptions<CoinbaseSettings> settings)
        {
            _settings = settings.Value;
            _httpClient.BaseAddress = new Uri(_settings.DataEndpoint);
            _httpClient.DefaultRequestHeaders.Add("CB-ACCESS-KEY", _settings.ApiKey);
        }

        public async Task<ExchangeData> GetSummaryAsync(string currency = Currency.USD)
        {
            var accounts = await RequestAsync<List<Wallet>>(HttpMethod.Get, "accounts?limit=100");
            var exchangeRates = await RequestAsync<ExchangeRates>(HttpMethod.Get, $"exchange-rates?currency={currency}");

            return ConvertSummary(accounts, exchangeRates);
        }

        private static ExchangeData ConvertSummary(List<Wallet> wallets, ExchangeRates exchangeRates)
        {
            var instruments = wallets.ConvertAll(wallet =>
            {
                var instrumentData = new InstrumentData
                {
                    Name = wallet.Balance.Currency,
                    Quantity = wallet.Balance.Amount,
                    SymbolType = SymbolType.Crypto
                };

                if (exchangeRates.Rates.ContainsKey(wallet.Balance.Currency))
                {
                    var exchangeRate = exchangeRates.Rates[wallet.Balance.Currency];
                    instrumentData.CurrentPrice = 1d / exchangeRate;
                    instrumentData.Value = Math.Round(instrumentData.Quantity * instrumentData.CurrentPrice, 2);
                }

                return instrumentData;
            }).Where(i => i.Quantity != 0).ToList();

            return new ExchangeData
            {
                Name = "Coinbase",
                NetAssetValue = instruments.Sum(i => i.Value),
                Instruments = instruments
            };
        }

        private async Task<T> RequestAsync<T>(HttpMethod httpMethod, string path)
        {
            RefreshAuthHeaders(httpMethod.ToString(), $"/v2/{path}");

            var httpResponse = await _httpClient.SendAsync(new HttpRequestMessage(httpMethod, path));
            var coinbaseResponse = await HttpResponseMessageHelpers.GetResponseAsync<CoinbaseHttpResponse<T>>(httpResponse);

            return coinbaseResponse.Data;
        }

        private void RefreshAuthHeaders(string httpMethod, string route)
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var signature = CalculateSignature(timestamp, httpMethod, route);

            _httpClient.DefaultRequestHeaders.Add("CB-ACCESS-SIGN", signature);
            _httpClient.DefaultRequestHeaders.Add("CB-ACCESS-TIMESTAMP", timestamp.ToString());
        }

        private string CalculateSignature(long timestamp, string httpMethod, string route)
        {
            var message = timestamp + httpMethod + route;
            var hmac = new HMACSHA256(Encoding.ASCII.GetBytes(_settings.SecretKey));
            var hash = hmac.ComputeHash(Encoding.ASCII.GetBytes(message));
            var signature = BitConverter.ToString(hash).Replace("-", "").ToLower();

            return signature;
        }
    }
}
