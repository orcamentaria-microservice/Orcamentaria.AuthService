using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orcamentaria.AuthService.Domain.DTOs.Service;
using Orcamentaria.AuthService.Domain.Services;
using Orcamentaria.Lib.Domain.Models;

namespace Orcamentaria.AuthService.API.Controllers.v1
{
    [ApiVersion("1.0")]
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ServiceController : Controller
    {
        private readonly IServiceService _service;

        public ServiceController(IServiceService service)
        {
            _service = service;
        }

        [Authorize(Roles = "SERVICE:READ")]
        [HttpGet("GetById/{id}", Name = "ServiceGetById")]
        public Response<ServiceResponseDTO> GetById(long id)
            => _service.GetById(id);

        [Authorize(Roles = "SERVICE:CREATE")]
        [HttpPost(Name = "ServiceInsert")]
        public async Task<Response<ServiceResponseDTO>> Insert([FromBody] ServiceInsertDTO dto)
            => await _service.Insert(dto);

        [Authorize(Roles = "SERVICE:UPDATE")]
        [HttpPut("{id}", Name = "ServiceUpdate")]
        public async Task<Response<ServiceResponseDTO>> Update(long id, [FromBody] ServiceUpdateDTO dto)
            => await _service.Update(id, dto);

        [Authorize(Roles = "SERVICE:UPDATE")]
        [HttpPut("UpdateCredentials/{id}", Name = "ServiceUpdateCredentials")]
        public async Task<Response<ServiceResponseDTO>> UpdateCredentials(long id)
            => await _service.UpdateCredentials(id);
    }
}
