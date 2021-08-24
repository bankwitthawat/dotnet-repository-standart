using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Widely.BusinessLogic.Services;
using Widely.BusinessLogic.Services.Auth;
using Widely.BusinessLogic.Services.Base;
using Widely.BusinessLogic.Utilities;
using Widely.DataAccess.DataContext;
using Widely.DataAccess.Repositories.Approles;
using Widely.DataAccess.Repositories.Appusers;
using Widely.DataAccess.Repositories.Auth;
using Widely.DataAccess.Repositories.Base;
using Widely.DataAccess.Repositories.UnitOfWork;
using Widely.Infrastructure.AutoMapper;

namespace Widely.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            return services
                .AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>))
                .AddScoped<IAuthRepository, AuthRepository>()
                .AddScoped<IAppusersRepository, AppusersRepository>()
                .AddScoped<IApprolesRepository, ApprolesRepository>()
                //add new repository here.
                ;
        }

        public static IServiceCollection AddUnitOfWork(this IServiceCollection services)
        {
            return services
                .AddScoped<IUnitOfWork, UnitOfWork>();
        }

        public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            return services.AddDbContext<WidelystandartContext>(options =>
            {
                var connetionString = configuration.GetConnectionString("DefaultConnection");
                options.UseMySql(connetionString, ServerVersion.AutoDetect(connetionString));
            });
        }

        public static IServiceCollection AddBusinessServices(this IServiceCollection services)
        {
            return services
                .AddScoped<JwtManager>()
                .AddScoped<BaseService>()
                .AddScoped<AuthService>()
                .AddScoped<ApprolesService>()
                // add new service here.
                ;
        }

        public static IServiceCollection AddHttpContext(this IServiceCollection services)
        {
            return services
                .AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }

        public static IServiceCollection AddAutoMapper(this IServiceCollection services)
        {
            return services.AddAutoMapper(typeof(AutoMapperProfile));
        }
    }
}
