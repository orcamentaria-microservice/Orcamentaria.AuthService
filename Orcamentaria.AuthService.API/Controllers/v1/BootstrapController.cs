using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orcamentaria.AuthService.Domain.DTOs.Bootstrap;
using Orcamentaria.AuthService.Domain.Services;
using Orcamentaria.Lib.Domain.Models;
using Orcamentaria.Lib.Domain.Models.Responses;

namespace Orcamentaria.AuthService.API.Controllers.v1
{
    [ApiVersion("1.0")]
    [Route("api/v1/[controller]")]
    [ApiController]
    public class BootstrapController : Controller
    {
        private readonly IBootstrapService _service;

        public BootstrapController(IBootstrapService service)
        {
            _service = service;
        }

        [Authorize(Roles = "MASTER,BOOTSTRAP:CREATE")]
        [HttpGet("GenerateBootstrapSecret/{serviceId}", Name = "BootstrapGenerateBootstrapSecret")]
        public async Task<Response<BootstrapResponseDTO>> GenerateBootstrapSecretAsync(long serviceId)
        {
            try
            {
                return await _service.GenerateBootstrapSecretAsync(serviceId);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [Authorize(Roles = "MASTER,BOOTSTRAP:DELETE")]
        [HttpGet("RevokeBootstrapSecret/{serviceId}", Name = "BootstrapRevokeBootstrapSecret")]
        public async Task<Response<long>> RevokeBootstrapSecretAsync(long serviceId)
        {
            try
            {
                return await _service.RevokeBootstrapSecretAsync(serviceId);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
