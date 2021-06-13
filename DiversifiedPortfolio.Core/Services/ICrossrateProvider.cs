using System.Threading.Tasks;

namespace DiversifiedPortfolio.Core.Services
{
    public interface ICrossrateProvider
    {
        Task<double> GetCrossrateAsync(string from, string to);
    }
}
