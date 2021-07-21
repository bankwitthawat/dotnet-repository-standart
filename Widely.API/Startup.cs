using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Widely.API.Extensions;
using Widely.API.Infrastructure.NLog;
using Widely.API.Infrastructure.Security;

namespace Widely.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            NLog.Config.ConfigurationItemFactory.Default.LayoutRenderers.RegisterDefinition("aspnet-request-ip", typeof(NLog.Web.LayoutRenderers.AspNetRequestIpLayoutRenderer));
            //NLog.Config.ConfigurationItemFactory.Default.LayoutRenderers.RegisterDefinition("aspnet-request-username", typeof(UserRequestLayoutRenderer));
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();
            services.AddMvc();

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });

            services
                .AddControllers()
                .AddJsonOptions(options =>
                {
                    //for PascalCase
                    //options.JsonSerializerOptions.PropertyNamingPolicy = null;

                    //for CamelCase
                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                });

            //add jwt validation
            services.AddJwtAuthentication(Configuration);

            //add swagger
            services.AddSwaggerCustom();

            //add application repositories
            services
               .AddHttpContext()
               .AddDatabase(Configuration)
               .AddUnitOfWork()
               .AddRepositories()
               .AddBusinessServices()
               .AddAutoMapper()
               ;

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, NLog.ILogger logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                //add swagger ui
                app.UseConfiguredSwagger();
                app.UseConfiguredSwaggerUI();
            }

            //add exception middleware
            app.ConfigureExceptionHandler(logger);

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseStatusCodePages();

            app.UseCors(x => x
              .SetIsOriginAllowed(origin => true)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials());


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
