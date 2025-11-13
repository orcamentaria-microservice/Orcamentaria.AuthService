using Orcamentaria.AuthService.Application.Providers;
using Orcamentaria.AuthService.Application.Services;
using Orcamentaria.AuthService.Application.Validators;
using Orcamentaria.AuthService.Domain.Mappers;
using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.AuthService.Domain.Repositories;
using Orcamentaria.AuthService.Domain.Services;
using Orcamentaria.AuthService.Infrastructure.Contexts;
using Orcamentaria.AuthService.Infrastructure.Repositories;
using Orcamentaria.Lib.Domain.Contexts;
using Orcamentaria.Lib.Domain.Providers;
using Orcamentaria.Lib.Domain.Validators;
using Orcamentaria.Lib.Infrastructure.Configures;
using Orcamentaria.Lib.Infrastructure.Contexts;

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

        public IConfiguration Configuration { get; set; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddServiceRegistryHosted(Configuration);

            services.ResolveCommonServicesWithMySql<MySqlContext>(
                configuration: Configuration, 
                serviceName: _serviceName, 
                apiVersion: _apiVersion, 
                customServices: () =>
            {
                services.AddScoped<IUserAuthContext, UserAuthContext>();

                services.AddAutoMapper(_ => { },
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
                services.AddScoped<IBootstrapRepository, BootstrapRepository>();

                services.AddScoped<IAuthenticationService, AuthenticationService>();
                services.AddScoped<IPasswordService, PasswordService>();
                services.AddScoped<IPermissionService, PermissionService>();
                services.AddScoped<IServiceService, ServiceService>();
                services.AddScoped<IUserService, UserService>();
                services.AddScoped<IBootstrapService, BootstrapService>();

                services.AddKeyedScoped<ITokenService<User>, UserTokenService>("userToken");
                services.AddKeyedScoped<ITokenService<User>, UserRefreshTokenService>("userRefreshToken");
                services.AddKeyedScoped<ITokenService<Service>, ServiceTokenService>("serviceToken");
                services.AddKeyedScoped<ITokenService<Service>, ClientIdTokenService>("clientIdToken");
                services.AddKeyedScoped<ITokenService<Service>, ClientSecretTokenService>("clientSecretToken");
                services.AddKeyedScoped<ITokenService<Bootstrap>, BootstrapSecretTokenService>("bootstrapSecretToken");
                services.AddKeyedScoped<ITokenService<Service>, BootstrapTokenService>("bootstrapToken");

                services.AddScoped<ITokenProvider, TokenProvider>();
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
            => app.ConfigureCommon(env, _serviceName, _apiVersion);
    }
}
