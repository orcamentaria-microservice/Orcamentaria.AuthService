using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orcamentaria.AuthService.Domain.DTOs.User;
using Orcamentaria.AuthService.Domain.Services;
using Orcamentaria.Lib.Domain.Models;
using Orcamentaria.Lib.Domain.Models.Responses;

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

        [Authorize(Roles = "MASTER,USER:READ")]
        [HttpPost("Get", Name = "UserGet")]
        public async Task<Response<IEnumerable<UserResponseDTO>>?> GetAsync([FromBody] GridParams gridParams)
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

        [Authorize(Roles = "MASTER,USER:CREATE")]
        [HttpPost(Name = "UserInsert")]
        public async Task<Response<UserResponseDTO>> InsertAsync([FromBody] UserInsertDTO dto)
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

        [Authorize(Roles = "MASTER,USER:UPDATE")]
        [HttpPut("{id}", Name = "UserUpdate")]
        public async Task<Response<UserResponseDTO>> UpdateAsync(long id, [FromBody] UserUpdateDTO dto)
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

        [Authorize(Roles = "MASTER,USER:UPDATE:ALTERPERMISSION")]
        [HttpPut("AddPermission/{id}", Name = "UserAddPermission")]
        public async Task<Response<UserResponseDTO>> AddPermissionsAsync(long id, [FromBody] UserAddPermissionsDTO dto)
        {
            try
            {
                return await _service.AddPermissionsAsync(id, dto);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [Authorize(Roles = "MASTER,USER:UPDATE:ALTERPERMISSION")]
        [HttpPut("RemovePermission/{id}", Name = "UserRemovePermission")]
        public async Task<Response<UserResponseDTO>> RemovePermissionsAsync(long id, [FromBody] UserRemovePermissionsDTO dto)
        {
            try
            {
                return await _service.RemovePermissionsAsync(id, dto);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [Authorize(Roles = "MASTER,USER:UPDATE")]
        [HttpPut("UpdatePassword/{id}", Name = "UserUpdatePassword")]
        public async Task<Response<UserResponseDTO>> UpdatePasswordAsync(long id, [FromBody] UserUpdatePasswordDTO dto)
        {
            try
            {
                return await _service.UpdatePasswordAsync(id, dto);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
