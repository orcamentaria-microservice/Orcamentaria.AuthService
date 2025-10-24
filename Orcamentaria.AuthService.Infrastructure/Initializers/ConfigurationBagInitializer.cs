using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Orcamentaria.AuthService.Application.Services;
using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.AuthService.Infrastructure.Contexts;
using Orcamentaria.AuthService.Infrastructure.Repositories;
using Orcamentaria.Lib.Application.Services;
using Orcamentaria.Lib.Domain.DTOs.ConfigurationBag;
using Orcamentaria.Lib.Domain.Enums;
using Orcamentaria.Lib.Domain.Exceptions;
using Orcamentaria.Lib.Domain.Models.Configurations;
using Orcamentaria.Lib.Domain.Models.Exceptions;

namespace Orcamentaria.AuthService.Infrastructure.Initializers
{
    public class ConfigurationBagInitializer
    {
        private readonly string _serviceName;

        public ConfigurationBagInitializer(string serviceName)
        {
            _serviceName = serviceName;
        }

        public async Task<IConfigurationRoot> InitializeAsync(IConfiguration configuration)
        {
            try
            {
                var httpClient = new HttpClient();
                var httpContextAccessor = new HttpContextAccessor();
                var httpClientService = new HttpClientService(httpClient, httpContextAccessor);

                var baseUrlApiGetaway = configuration.GetSection("BaseUrlApiGetaway").Value;

                if (string.IsNullOrEmpty(baseUrlApiGetaway))
                    throw new ConfigurationException("BaseUrlApiGetaway não configurado.");

                var config = new ApiGetawayConfiguration
                {
                    BaseUrl = baseUrlApiGetaway,
                };

                var options = Options.Create(config);

                var client = new ApiGetawayService(httpClientService, options);

                var token = GenerateBootstrapToken(_serviceName, configuration);

                var resource = new ResourceConfiguration
                {
                    ServiceName = "ConfigBagService",
                    EndpointName = "ConfigurationBagGetByServiceName",
                };

                IDictionary<string, string> @params = new Dictionary<string, string>();

                @params.Add("serviceName", _serviceName);

                var response = await client.Routing<ConfigurationBagResponseDTO>(
                        baseUrlApiGetaway,
                        resource.ServiceName,
                        resource.EndpointName,
                        token,
                        @params,
                        null);

                if (!response.Success)
                    throw new Lib.Domain.Exceptions.ConfigurationException($"Erro ao buscar configurações do serviço {_serviceName}");

                var dict = ToAppsettingsDictionary(response.Data);

                var newConfig = new ConfigurationBuilder()
                    .AddConfiguration(configuration)
                    .AddInMemoryCollection(dict)
                    .Build();

                return newConfig;
            }
            catch (DefaultException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new UnexpectedException(ex.Message, ex);
            }
        }

        #region private methods

        private static string GenerateBootstrapToken(string serviceName, IConfiguration configuration)
        {
            var rsa = new RsaService();
            var bootstrapTokenService = new BootstrapTokenService(rsa);

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            var builder = new DbContextOptionsBuilder<MySqlContext>().UseMySQL(connectionString);

            if(builder is null)
                throw new InfoException("Ocorreu um erro ao conectar com o banco de dados.", ErrorCodeEnum.DatabaseError);

            var dbContext = new MySqlContext(builder.Options);

            var repository = new ServiceRepository(dbContext);

            var service = repository.GetByName(serviceName);
            if (service is null)
                throw new InfoException($"Ocorreu um erro ao buscar serviço de autenticação.", ErrorCodeEnum.NotFound);

            var token = bootstrapTokenService.Generate(new Service { Id = service.Id, Name = service.Name });

            return token;
        }
        private static Dictionary<string, string> ToAppsettingsDictionary(ConfigurationBagResponseDTO bag)
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (!string.IsNullOrWhiteSpace(bag.ServiceName))
                dict["ConfigurationBag:ServiceName"] = bag.ServiceName;
            dict["ConfigurationBag:UpdateAt"] = bag.UpdateAt.ToString("O");

            if (bag.ConnectionStrings is not null)
            {
                foreach (var row in bag.ConnectionStrings)
                {
                    if (row is null) continue;

                    if (row.TryGetValue("Name", out var name) &&
                        (row.TryGetValue("ConnectionString", out var cs) || row.TryGetValue("Value", out cs)))
                    {
                        if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(cs))
                            dict[$"ConnectionStrings:{name}"] = cs;
                        continue;
                    }

                    foreach (var kv in row)
                    {
                        if (!string.IsNullOrWhiteSpace(kv.Key) && !string.IsNullOrWhiteSpace(kv.Value))
                            dict[$"ConnectionStrings:{kv.Key}"] = kv.Value;
                    }
                }
            }
            if (bag.Configurations is not null)
            {
                foreach (var item in bag.Configurations)
                {
                    if (item is null) continue;

                    foreach (var (sectionName, sectionValue) in item)
                    {
                        FlattenObject(dict, sectionValue, sectionName);
                    }
                }
            }

            return dict;
        }

        private static void FlattenObject(Dictionary<string, string> bag, object? value, string prefix)
        {
            if (value is null) return;

            switch (value)
            {
                case string s:
                    bag[prefix] = s;
                    return;

                case bool b:
                    bag[prefix] = b.ToString();
                    return;

                case int or long or short or byte or double or float or decimal:
                    bag[prefix] = Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture)!;
                    return;

                case DateTime dt:
                    bag[prefix] = dt.ToString("O");
                    return;

                case IDictionary<string, object> dictObj:
                    foreach (var (k, v) in dictObj)
                        FlattenObject(bag, v, $"{prefix}:{k}");
                    return;

                case IEnumerable<object> list:
                    int i = 0;
                    foreach (var item in list)
                        FlattenObject(bag, item, $"{prefix}:{i++}");
                    return;

                default:
                    var json = System.Text.Json.JsonSerializer.Serialize(value);
                    var el = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(json);
                    FlattenJsonElement(bag, el, prefix);
                    return;
            }
        }

        private static void FlattenJsonElement(Dictionary<string, string> bag, System.Text.Json.JsonElement el, string prefix)
        {
            switch (el.ValueKind)
            {
                case System.Text.Json.JsonValueKind.Object:
                    foreach (var p in el.EnumerateObject())
                        FlattenJsonElement(bag, p.Value, $"{prefix}:{p.Name}");
                    break;

                case System.Text.Json.JsonValueKind.Array:
                    int i = 0;
                    foreach (var item in el.EnumerateArray())
                        FlattenJsonElement(bag, item, $"{prefix}:{i++}");
                    break;

                case System.Text.Json.JsonValueKind.String:
                    bag[prefix] = el.GetString()!;
                    break;

                case System.Text.Json.JsonValueKind.Number:
                    bag[prefix] = el.GetRawText();
                    break;

                case System.Text.Json.JsonValueKind.True:
                case System.Text.Json.JsonValueKind.False:
                    bag[prefix] = el.GetBoolean().ToString();
                    break;
            }
        }

        #endregion
    }
}
