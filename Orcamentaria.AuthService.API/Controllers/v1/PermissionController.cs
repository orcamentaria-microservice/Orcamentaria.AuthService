using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orcamentaria.AuthService.Domain.DTOs.Permission;
using Orcamentaria.AuthService.Domain.Services;
using Orcamentaria.Lib.Domain.Models;
using Orcamentaria.Lib.Domain.Models.Responses;

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

        [Authorize(Roles = "MASTER,PERMISSION:READ")]
        [HttpPost("Get", Name = "PermissionGet")]
        public async Task<Response<IEnumerable<PermissionResponseDTO>>?> GetAsync([FromBody] GridParams gridParams)
        {
            try
            {
                return await _service.GetAsync(gridParams);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [Authorize(Roles = "MASTER,PERMISSION:CREATE")]
        [HttpPost(Name = "PermissionInsert")]
        public async Task<Response<PermissionResponseDTO>> InsertAsync([FromBody] PermissionInsertDTO dto)
        {
            try
            {
                return await _service.InsertAsync(dto);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [Authorize(Roles = "MASTER,PERMISSION:UPDATE")]
        [HttpPut("{id}", Name = "PermissionUpdate")]
        public async Task<Response<PermissionResponseDTO>> UpdateAsync(long id, [FromBody] PermissionUpdateDTO dto)
        {
            try
            {
                return await _service.UpdateAsync(id, dto);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
