namespace Orcamentaria.AuthService.Domain
{
    public class ServiceRegistryOptions
    {
        public string ServiceName { get; set; }
        public string BaseUrl { get; set; }
        public string RegistryServiceRoute { get; set; }
        public string HeartbeatRoute { get; set; }
    }
}
