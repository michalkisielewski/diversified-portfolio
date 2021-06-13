using DiversifiedPortfolio.Core;
using DiversifiedPortfolio.Core.Exchanges;
using DiversifiedPortfolio.Core.Exchanges.Binance;
using DiversifiedPortfolio.Core.Exchanges.Coinbase;
using DiversifiedPortfolio.Core.Exchanges.Ether;
using DiversifiedPortfolio.Core.Exchanges.Exante;
using DiversifiedPortfolio.Core.Exchanges.Xtb;
using DiversifiedPortfolio.Core.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DiversifiedPortfolio.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<ExanteSettings>(Configuration.GetSection("Exchanges:Exante"));
            services.Configure<CoinbaseSettings>(Configuration.GetSection("Exchanges:Coinbase"));
            services.Configure<BinanceSettings>(Configuration.GetSection("Exchanges:Binance"));
            services.Configure<XtbSettings>(Configuration.GetSection("Exchanges:Xtb"));
            services.Configure<EtherSettings>(Configuration.GetSection("Exchanges:Ether"));

            services.AddScoped<ICrossrateProvider, ExanteExchange>();
            services.AddScoped<IExchange, ExanteExchange>();
            services.AddScoped<IExchange, CoinbaseExchange>();
            services.AddScoped<IExchange, BinanceExchange>();
            services.AddScoped<IExchange, EtherExchange>();
            services.AddScoped<IExchange, XtbExchange>();
            services.AddScoped<Portfolio>();

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors(policy => policy
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
