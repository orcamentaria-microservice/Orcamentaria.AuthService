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

        [Authorize(Roles = "MASTER,SERVICE:READ")]
        [HttpGet("GetById/{id}", Name = "ServiceGetById")]
        public Response<ServiceResponseDTO> GetById(long id)
        {
            try
            {
                return _service.GetById(id);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [Authorize(Roles = "MASTER,SERVICE:CREATE")]
        [HttpPost(Name = "ServiceInsert")]
        public async Task<Response<ServiceResponseDTO>> Insert([FromBody] ServiceInsertDTO dto)
        {
            try
            {
                return await _service.Insert(dto);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [Authorize(Roles = "MASTER,SERVICE:UPDATE")]
        [HttpPut("{id}", Name = "ServiceUpdate")]
        public async Task<Response<ServiceResponseDTO>> Update(long id, [FromBody] ServiceUpdateDTO dto)
        {
            try
            {
                return await _service.Update(id, dto);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [Authorize(Roles = "MASTER,SERVICE:UPDATE")]
        [HttpPut("UpdateCredentials/{id}", Name = "ServiceUpdateCredentials")]
        public async Task<Response<ServiceResponseDTO>> UpdateCredentials(long id)
        {
            try
            {
                return await _service.UpdateCredentials(id);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
