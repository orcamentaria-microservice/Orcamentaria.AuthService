using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orcamentaria.AuthService.Domain.DTOs.Permissions;
using Orcamentaria.Lib.Domain.Enums;
using Orcamentaria.AuthService.Domain.Services;
using Orcamentaria.Lib.Domain.Models;

namespace Orcamentaria.AuthService.API.Controllers.v1
{
    [ApiVersion("1.0")]
    [Route("api/v1/[controller]")]
    [ApiController]
    public class PermissionController : Controller
    {
        private readonly IPermissionService _service;

        public PermissionController(IPermissionService service)
        {
            _service = service;
        }

        [Authorize(Roles = "PERMISSION:READ")]
        [HttpGet("GetById/{id}", Name = "PermissionGetById")]
        public Response<PermissionResponseDTO> GetById(long id)
            => _service.GetById(id);

        [Authorize(Roles = "PERMISSION:READ")]
        [HttpGet("GetByResource/{id}", Name = "PermissionGetByResource")]
        public Response<IEnumerable<PermissionResponseDTO>> GetByResource(ResourceEnum resource)
            => _service.GetByResource(resource);

        [Authorize(Roles = "PERMISSION:READ")]
        [HttpGet("GetByType/{id}", Name = "PermissionGetByType")]
        public Response<IEnumerable<PermissionResponseDTO>> GetByType(PermissionTypeEnum type)
            => _service.GetByType(type);

        [Authorize(Roles = "PERMISSION:CREATE")]
        [HttpPost(Name = "PermissionInsert")]
        public async Task<Response<PermissionResponseDTO>> Insert([FromBody] PermissionInsertDTO dto)
            => await _service.Insert(dto);

        [Authorize(Roles = "PERMISSION:UPDATE")]
        [HttpPut("{id}", Name = "PermissionUpdate")]
        public async Task<Response<PermissionResponseDTO>> Update(long id, [FromBody] PermissionUpdateDTO dto)
            => await _service.Update(id, dto);
    }
}
