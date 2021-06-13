using DiversifiedPortfolio.Core.Constants;
using System.Threading.Tasks;

namespace DiversifiedPortfolio.Core.Exchanges
{
    public interface IExchange
    {
        Task<ExchangeData> GetSummaryAsync(string currency = Currency.USD);
    }
}
