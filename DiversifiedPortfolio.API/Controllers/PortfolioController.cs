using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DiversifiedPortfolio.Core;
using DiversifiedPortfolio.Core.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DiversifiedPortfolio.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PortfolioController : ControllerBase
    {
        private readonly Portfolio portfolio;

        public PortfolioController(Portfolio portfolio)
        {
            this.portfolio = portfolio;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string currency = "PLN")
        {
            try
            {
                return Ok(await portfolio.GetExchangesData(currency));
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
