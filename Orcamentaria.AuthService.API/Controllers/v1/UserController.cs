using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orcamentaria.AuthService.Domain.DTOs.User;
using Orcamentaria.AuthService.Domain.Services;
using Orcamentaria.Lib.Domain.Models;

namespace Orcamentaria.AuthService.API.Controllers.v1
{
    [ApiVersion("1.0")]
    [Route("api/v1/[controller]")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly IUserService _service;

        public UserController(IUserService service)
        {
            _service = service;
        }

        [Authorize(Roles = "USER:READ")]
        [HttpGet("GetByCompanyId", Name = "UserGetByCompanyId")]
        public Response<IEnumerable<UserResponseDTO>> GetByCompanyId(long id)
            => _service.GetByCompanyId();

        [Authorize(Roles = "USER:READ")]
        [HttpGet("GetByEmail/{email}", Name = "UserGetByEmail")]
        public Response<UserResponseDTO> GetByEmail(string email)
            => _service.GetByEmail(email);

        [Authorize(Roles = "SERVICE:CREATE")]
        [HttpPost(Name = "UserInsert")]
        public async Task<Response<UserResponseDTO>> Insert([FromBody] UserInsertDTO dto)
            => await _service.Insert(dto);

        [Authorize(Roles = "SERVICE:UPDATE")]
        [HttpPut("{id}", Name = "UserUpdate")]
        public async Task<Response<UserResponseDTO>> Update(long id, [FromBody] UserUpdateDTO dto)
            => await _service.Update(id, dto);

        [Authorize(Roles = "SERVICE:UPDATE")]
        [HttpPut("AddPermission/{id}", Name = "UserAddPermission")]
        public async Task<Response<UserResponseDTO>> AddPermission(long id, [FromBody] dynamic permissionsId)
            => await _service.AddPermission(id, permissionsId);

        [Authorize(Roles = "SERVICE:UPDATE")]
        [HttpPut("RemovePermission/{id}", Name = "UserRemovePermission")]
        public async Task<Response<UserResponseDTO>> RemovePermission(long id, [FromBody] dynamic permissionsId)
            => await _service.RemovePermission(id, permissionsId);

        [Authorize(Roles = "SERVICE:UPDATE")]
        [HttpPut("UpdatePassword/{id}", Name = "UserUpdatePassword")]
        public async Task<Response<UserResponseDTO>> UpdatePassword(long id, [FromBody] UserUpdatePasswordDTO dto)
            => await _service.UpdatePassword(id, dto);
    }
}
