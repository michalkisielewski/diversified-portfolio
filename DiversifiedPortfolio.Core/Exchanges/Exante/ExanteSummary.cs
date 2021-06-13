using System.Collections.Generic;

namespace DiversifiedPortfolio.Core.Exchanges.Exante
{
    internal class ExanteSummary
    {
        public double NetAssetValue { get; set; }
        public List<ExanteCurrency> Currencies { get; set; }
        public List<ExantePosition> Positions { get; set; }
    }
}
