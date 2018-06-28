using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Codat.Public.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Codat.Core
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            // 1. Setup global settings (using external configuration like appSettings.json is recommended)

            CodatGlobalClientSettings.ApiKey = "hqiiauzucvny4vg4tnjzxp417t6uht4g4r13ujxc"; // TODO - Add you own API key.
            CodatGlobalClientSettings.Environment = CodatEnvironment.Uat;

            // 2. The codat client will automatically pick up these global settings.

            services.AddSingleton<CodatClient>();
            
            /*
                         
            // 3. Alternative methods for setup

            // 4. Defaults to UAT environment
            services.AddSingleton<CodatClient>(c => new CodatClient("ADD_API_KEY"));
            
            // 5. Use a settings object
            
            var settingsWithProxy = new CodatClientSettings("ADD_API_KEY", "PROXY_BASE_URL");
            var settingsWithEnvironment = new CodatClientSettings("ADD_API_KEY", CodatEnvironment.Uat);

            services.AddSingleton<CodatClient>(c => new CodatClient(settingsWithProxy));

             */
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
