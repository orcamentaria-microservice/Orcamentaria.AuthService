using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orcamentaria.AuthService.Domain.DTOs.Bootstrap;
using Orcamentaria.AuthService.Domain.Services;
using Orcamentaria.Lib.Domain.Models;

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
        [HttpGet("CreateBootstrapSecret/{serviceId}", Name = "BootstrapCreateBootstrapSecret")]
        public async Task<Response<BootstrapResponseDTO>> CreateBootstrapSecret(long serviceId)
        {
            try
            {
                return await _service.CreateBootstrapSecret(serviceId);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [Authorize(Roles = "MASTER,BOOTSTRAP:DELETE")]
        [HttpGet("RevokeBootstrapSecret/{serviceId}", Name = "BootstrapRevokeBootstrapSecret")]
        public Response<long> RevokeBootstrapSecret(long serviceId)
        {
            try
            {
                return _service.RevokeBootstrapSecret(serviceId);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
