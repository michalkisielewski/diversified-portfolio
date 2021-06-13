using DiversifiedPortfolio.Core.Constants;
using DiversifiedPortfolio.Core.Extensions;
using DiversifiedPortfolio.Core.Services;
using JWT.Algorithms;
using JWT.Builder;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DiversifiedPortfolio.Core.Exchanges.Exante
{
    public partial class ExanteExchange : IExchange, ICrossrateProvider
    {
        private readonly ExanteSettings _settings;
        private readonly HttpClient _httpClient = new HttpClient();

        public ExanteExchange(IOptions<ExanteSettings> settings)
        {
            _settings = settings.Value;

            string token = GenerateAccessToken();

            _httpClient.BaseAddress = new Uri(_settings.DataEndpoint);
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        }

        public async Task<double> GetCrossrateAsync(string from, string to)
        {
            var httpResponse = await _httpClient.GetAsync($"crossrates/{from}/{to}");
            var crossrate = await HttpResponseMessageHelpers.GetResponseAsync<ExanteCrossrate>(httpResponse);

            return crossrate.Rate;
        }

        public async Task<ExchangeData> GetSummaryAsync(string currency = Currency.USD)
        {
            var liveAccountId = await GetLiveAccountId();

            var httpResponseMessage = await _httpClient.GetAsync($"summary/{liveAccountId}/{currency}");
            var summary = await HttpResponseMessageHelpers.GetResponseAsync<ExanteSummary>(httpResponseMessage);

            return ConvertSummary(summary);
        }

        private static ExchangeData ConvertSummary(ExanteSummary summary)
        {
            var currencies = summary.Currencies.ConvertAll(currency =>
                new InstrumentData
                {
                    Name = currency.Code,
                    Value = currency.ConvertedValue,
                    CurrentPrice = currency.Value == 0 ? 0 : Math.Round(currency.ConvertedValue / currency.Value, 2),
                    SymbolType = SymbolType.Fiat
                });

            var positions = summary.Positions.ConvertAll((position) =>
            {
                return new InstrumentData
                {
                    Name = position.Id,
                    AveragePrice = position.AveragePrice.GetValueOrDefault(),
                    CurrentPrice = position.Price,
                    Quantity = position.Quantity,
                    Value = position.ConvertedValue,
                    SymbolType = position.SymbolType
                };
            });

            return new ExchangeData
            {
                Name = "Exante",
                NetAssetValue = summary.NetAssetValue,
                Instruments = positions.Concat(currencies).ToList()
            };
        }

        private async Task<string> GetLiveAccountId()
        {
            var httpResponseMessage = await _httpClient.GetAsync("accounts");
            var accounts = await HttpResponseMessageHelpers.GetResponseAsync<List<ExanteAccount>>(httpResponseMessage);

            var liveAccount = accounts.FirstOrDefault(a => a.Status == "Full");

            return liveAccount.AccountId;
        }

        public string GenerateAccessToken()
        {
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            return new JwtBuilder()
                .WithAlgorithm(new HMACSHA256Algorithm())
                .WithSecret(Encoding.ASCII.GetBytes(_settings.Secret))
                .AddClaim("iss", _settings.ClientId)
                .AddClaim("sub", _settings.AppId)
                .AddClaim("iat", timestamp)
                .AddClaim("exp", timestamp + 3600)
                .AddClaim("aud", new List<string> 
                    { 
                        "symbols", 
                        "ohlc" , 
                        "feed", 
                        "change", 
                        "crossrates", 
                        "summary", 
                        "accounts"
                    })
                .Encode();
        }
    }
}
