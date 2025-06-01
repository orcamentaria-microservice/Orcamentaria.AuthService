namespace Orcamentaria.AuthService.Domain
{
    public class ServiceRegisterPayload
    {
        public string Name { get; set; }
        public string BaseUrl { get; set; }
        public List<ServiceEndpoint> Endpoints { get; set; }
    }
}
