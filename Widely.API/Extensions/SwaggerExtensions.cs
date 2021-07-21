using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Widely.API.Extensions
{
    public static class SwaggerExtensions
    {
        public static IServiceCollection AddSwaggerCustom(this IServiceCollection services)
        {
            return services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Widely.API", Version = "v1" });

               // c.EnableAnnotations();

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);


                // Include 'SecurityScheme' to use JWT Authentication
                var jwtSecurityScheme = new OpenApiSecurityScheme
                {
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Name = "JWT Authentication",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Description = "Please enter JWT with Bearer into field",

                    Reference = new OpenApiReference
                    {
                        Id = JwtBearerDefaults.AuthenticationScheme,
                        Type = ReferenceType.SecurityScheme
                    }
                };

                c.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        jwtSecurityScheme, Array.Empty<string>()
                    }
                }); 
            });
        }

        public static IApplicationBuilder UseConfiguredSwagger(this IApplicationBuilder app)
        {
            return app.UseSwagger();
        }

        public static IApplicationBuilder UseConfiguredSwaggerUI(this IApplicationBuilder app)
        {
            return app.UseSwaggerUI(c =>
            {
                c.DefaultModelsExpandDepth(-1);
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Widely.API v1");
            });
        }
    }
}
