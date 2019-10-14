using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace weather
{
    public class Startup
    {
        bool isStableVersion = true;
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddHealthChecks()
                    .AddCheck("live", 
                        () => isStableVersion ? 
                                HealthCheckResult.Healthy() :
                                HealthCheckResult.Unhealthy())
                    .AddRedis("redis", tags: new[] {"dependencies"})
                    .AddSqlServer(
                        Configuration["ConnectionStrings:DefaultConnection"], 
                        tags: new[] {"dependencies"})
                    ;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseHealthChecks("/live", new HealthCheckOptions
            {
                Predicate = r => r.Name.Contains("live")
            });

            app.UseHealthChecks("/ready", new HealthCheckOptions
            {
                Predicate = r => r.Tags.Contains("dependencies")
            });
            
            app.Map("/chaos", appBuilder =>
            {
                appBuilder.Run(async context =>
                {
                    isStableVersion = !isStableVersion;
                    await context.Response.WriteAsync($"Is {Environment.MachineName} stable? {isStableVersion}");
                });
            });
        }
    }
}
