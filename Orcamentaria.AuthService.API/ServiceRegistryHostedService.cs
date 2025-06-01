
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text;
using Orcamentaria.AuthService.Domain;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Caching.Memory;
using Orcamentaria.Lib.Domain.Models;
using Orcamentaria.Lib.Domain.Enums;
using Microsoft.EntityFrameworkCore.Update;
using MySqlX.XDevAPI.Common;
using System.Linq;
using System.Net.Http;

namespace Orcamentaria.AuthService.API
{
    public class ServiceRegistryHostedService : IHostedService, IDisposable
    {
        private static string TOKEN_KEY = "_tokenRegistry_";

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IServer _server;
        private readonly IHostApplicationLifetime _lifetime;
        private readonly IOptions<ServiceRegistryOptions> _options;
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _memoryCache;
        private Timer _heartbeatTimer;

        public ServiceRegistryHostedService(
            IServiceScopeFactory scopeFactory,
            IServer server,
            IHostApplicationLifetime lifetime,
            IOptions<ServiceRegistryOptions> options,
            IHttpClientFactory httpClientFactory,
            IMemoryCache memoryCache)
        {
            _scopeFactory = scopeFactory;
            _server = server;
            _lifetime = lifetime;
            _options = options;
            _httpClient = httpClientFactory.CreateClient();
            _memoryCache = memoryCache;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _lifetime.ApplicationStarted.Register(OnStarted);
            return Task.CompletedTask;
        }

        public async void OnStarted()
        {
            await SendRegistry();

            new Timer(SendHeartbeat, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
        }

        private async Task SendRegistry()
        {
            var addressesFeature = _server.Features.Get<IServerAddressesFeature>();
            var address = addressesFeature?.Addresses?.FirstOrDefault();

            var swaggerUrl = $"{address}swagger/v1/swagger.json";

            var endpoints = await GetEndpointsFromSwaggerAsync(swaggerUrl);

            var payload = new ServiceRegisterPayload
            {
                Name = _options.Value.ServiceName,
                BaseUrl = address!,
                Endpoints = endpoints
            };

            try
            {
                var requestFailed = false;
                var attemptNumber = 1;

                do
                {
                    var result = await SendAsyncServiceRegistry(
                        url: $"{_options.Value.BaseUrl}{_options.Value.RegistryServiceRoute}",
                        content: payload,
                        method: HttpMethod.Post);

                    requestFailed = !result.Success;
                    attemptNumber++;

                    await Task.Delay(TimeSpan.FromSeconds(30));

                } while (requestFailed && attemptNumber <= 6);

                if(requestFailed)
                {
                    //Fazer log para salvar erro das tentantivas
                }
                
            }
            catch (Exception ex)
            {

            }
        }

        private async void SendHeartbeat(object state)
        {
            try
            {
                await SendAsyncServiceRegistry(
                    url: $"{_options.Value.BaseUrl}{_options.Value.HeartbeatRoute}",
                    content: null,
                    method: HttpMethod.Put
                    );
            }
            catch (Exception ex)
            {
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _heartbeatTimer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _heartbeatTimer?.Dispose();
        }

        private async Task<Response<dynamic>> SendAsyncServiceRegistry(
            string url, dynamic content, HttpMethod method, bool forceTokenGeneration = false)
        {
            if (forceTokenGeneration || !GetMemoryCache(TOKEN_KEY, out string? bearerTokenServiceRegistry))
            {
                using var scope = _scopeFactory.CreateScope();
                var tokenProvider = scope.ServiceProvider.GetRequiredService<ITokenProvider>();
                bearerTokenServiceRegistry = await tokenProvider.GetTokenAsync();
                SetMemoryCache(TOKEN_KEY, bearerTokenServiceRegistry);

                _httpClient.DefaultRequestHeaders.Remove("Authorization");
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {bearerTokenServiceRegistry}");
            }

            var requestMessage = new HttpRequestMessage
            {
                Method = method,
                RequestUri = new Uri(url),
            };

            if (content is not null)
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                };

                requestMessage.Content = new StringContent(JsonSerializer.Serialize(content, options), Encoding.UTF8, "application/json");
            }

            try
            {
                var response = await _httpClient.SendAsync(requestMessage);

                if (!response.EnsureSuccessStatusCode().IsSuccessStatusCode)
                    throw new Exception();

                var contentResponse = JsonSerializer.Deserialize<Response<dynamic>>(await response.Content.ReadAsStringAsync());

                if (!contentResponse.Success && contentResponse.Error.ErrorCode == ResponseErrorEnum.AccessDenied)
                    return SendAsyncServiceRegistry(url, content, method, true);
                        
                return contentResponse;
            }
            catch (Exception ex)
            {
                return new Response<dynamic>(ResponseErrorEnum.ExternalServiceFailure, ex.Message);
            }


        }

        private async Task<List<ServiceEndpoint>> GetEndpointsFromSwaggerAsync(string swaggerUrl)
        {
            var endpoints = new List<ServiceEndpoint>();

            var response = await _httpClient.GetAsync(swaggerUrl);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var paths = root.GetProperty("paths");

            foreach (var path in paths.EnumerateObject())
            {
                foreach (var method in path.Value.EnumerateObject())
                {
                    var route = path.Name.TrimStart('/');
                    var methodName = method.Name.ToUpperInvariant();

                    method.Value.TryGetProperty("operationId", out var operationId);

                    endpoints.Add(new ServiceEndpoint
                    {
                        Name = operationId.ToString(),
                        Method = methodName,
                        Route = route
                    });
                }
            }

            return endpoints;
        }

        private bool GetMemoryCache(string cacheKey, out string? returnValue)
        {
            if (_memoryCache.TryGetValue(cacheKey, out string? tokenCache))
            {
                returnValue = tokenCache;
                return tokenCache != null;
            }

            returnValue = null;
            return false;
        }

        private void SetMemoryCache(string cacheKey, string value)
            => _memoryCache.Set(cacheKey, value);
    }
}
