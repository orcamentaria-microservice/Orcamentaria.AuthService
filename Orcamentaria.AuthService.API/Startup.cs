
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Orcamentaria.Lib.Infrastructure.Middlewares;
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
using Orcamentaria.AuthService.Domain;
using Orcamentaria.AuthService.Application;

namespace Orcamentaria.AuthService.API
{
    public class Startup
    {
        readonly string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
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

            //PASSAR PARA ORCAMENTARIA.LIB
            services.AddMemoryCache();

            services.AddApiVersioning(opt =>
            {
                opt.AssumeDefaultVersionWhenUnspecified = true;
                opt.DefaultApiVersion = new ApiVersion(1, 0);
                opt.ReportApiVersions = true;
                opt.ApiVersionReader = new UrlSegmentApiVersionReader();
            });

            services.AddControllers()
                .AddJsonOptions(opts => opts.JsonSerializerOptions.PropertyNamingPolicy = null);

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Auth MS", Version = "v1" });
            });

            services.AddControllers()
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.SuppressConsumesConstraintForFormFileParameters = false;
                    options.SuppressInferBindingSourcesForParameters = true;
                    options.SuppressModelStateInvalidFilter = false;
                    options.SuppressMapClientErrors = false;
                    options.ClientErrorMapping[StatusCodes.Status404NotFound].Link =
                        "https://httpstatuses.com/404";
                });

            services.AddCors(options =>
            {
                options.AddPolicy(name: MyAllowSpecificOrigins,
                    builder =>
                    {
                        builder
                        .AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                    });
            });

            services.AddHttpClient();
            services.AddHostedService<ServiceRegistryHostedService>();
            services.Configure<ServiceRegistryOptions>(Configuration.GetSection("ServiceRegistry"));
            services.Configure<AuthenticationSecretsOptions>(Configuration.GetSection("Secrets"));
            services.AddScoped<ITokenProvider, TokenProvider>();

        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Auth MS v1"));
                app.UseDeveloperExceptionPage();
            }

            //app.UseMiddleware<UserAuthMiddleware>();

            app.UseRouting();

            app.UseCors(MyAllowSpecificOrigins);

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
