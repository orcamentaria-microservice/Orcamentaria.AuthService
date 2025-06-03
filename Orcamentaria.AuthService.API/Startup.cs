using Orcamentaria.Lib.Domain.Contexts;
using Orcamentaria.AuthService.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;
using Orcamentaria.AuthService.Domain.Repositories;
using Orcamentaria.AuthService.Infrastructure.Repositories;
using Orcamentaria.AuthService.Domain.Mappers;
using Orcamentaria.Lib.Domain.Validators;
using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.AuthService.Application.Validators;
using Orcamentaria.AuthService.Domain.Services;
using Orcamentaria.AuthService.Application.Services;
using Orcamentaria.Lib.Infrastructure.Contexts;
using Orcamentaria.Lib.Infrastructure;

namespace Orcamentaria.AuthService.API
{
    public class Startup
    {
        private readonly string _serviceName = "Orcamentaria.AuthService";
        private readonly string _apiVersion = "v1";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            CommonDI.ResolveCommonServices(_serviceName, _apiVersion, services, Configuration);

            services.AddDbContext<MySqlContext>(options =>
                options.UseMySQL(Configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IUserAuthContext, UserAuthContext>();

            services.AddAutoMapper(
                typeof(PermissionMapper),
                typeof(ServiceMapper),
                typeof(UserMapper));

            services.AddScoped<IValidatorEntity<User>, UserValidator>();
            services.AddScoped<IValidatorEntity<Permission>, PermissionValidator>();
            services.AddScoped<IValidatorEntity<Service>, ServiceValidator>();
            services.AddScoped<IValidatorEntity<User>, UserValidator>();

            services.AddScoped<IPermissionRepository, PermissionRepository>();
            services.AddScoped<IServiceRepository, ServiceRepository>();
            services.AddScoped<IUserRepository, UserRepository>();

            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IPasswordService, PasswordService>();
            services.AddScoped<IPermissionService, PermissionService>();
            services.AddScoped<IServiceService, ServiceService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IUserService, UserService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
            => CommonDI.ConfigureCommon(_serviceName, _apiVersion, app, env);
    }
}
