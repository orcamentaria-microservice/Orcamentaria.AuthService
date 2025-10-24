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

        [Authorize(Roles = "MASTER,USER:READ")]
        [HttpGet("GetByCompanyId", Name = "UserGetByCompanyId")]
        public Response<IEnumerable<UserResponseDTO>> GetByCompanyId(long id)
        {
            try
            {
                return _service.GetByCompanyId();
            }
            catch (Exception)
            {
                throw;
            }
        }

        [Authorize(Roles = "MASTER,USER:READ")]
        [HttpGet("GetByEmail/{email}", Name = "UserGetByEmail")]
        public Response<UserResponseDTO> GetByEmail(string email)
        {
            try
            {
                return _service.GetByEmail(email);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [Authorize(Roles = "MASTER,USER:CREATE")]
        [HttpPost(Name = "UserInsert")]
        public async Task<Response<UserResponseDTO>> Insert([FromBody] UserInsertDTO dto)
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

        [Authorize(Roles = "MASTER,USER:UPDATE")]
        [HttpPut("{id}", Name = "UserUpdate")]
        public async Task<Response<UserResponseDTO>> Update(long id, [FromBody] UserUpdateDTO dto)
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

        [Authorize(Roles = "MASTER,USER:UPDATE:ALTERPERMISSION")]
        [HttpPut("AddPermission/{id}", Name = "UserAddPermission")]
        public async Task<Response<UserResponseDTO>> AddPermission(long id, [FromBody] UserAddPermissionsDTO dto)
        {
            try
            {
                return await _service.AddPermission(id, dto);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [Authorize(Roles = "MASTER,USER:UPDATE:ALTERPERMISSION")]
        [HttpPut("RemovePermission/{id}", Name = "UserRemovePermission")]
        public async Task<Response<UserResponseDTO>> RemovePermission(long id, [FromBody] UserRemovePermissionsDTO dto)
        {
            try
            {
                return await _service.RemovePermission(id, dto);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [Authorize(Roles = "MASTER,USER:UPDATE")]
        [HttpPut("UpdatePassword/{id}", Name = "UserUpdatePassword")]
        public async Task<Response<UserResponseDTO>> UpdatePassword(long id, [FromBody] UserUpdatePasswordDTO dto)
        {
            try
            {
                return await _service.UpdatePassword(id, dto);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
