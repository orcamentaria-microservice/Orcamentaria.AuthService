using AutoMapper;
using Orcamentaria.AuthService.Domain.DTOs.User;
using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.AuthService.Domain.Repositories;
using Orcamentaria.AuthService.Domain.Services;
using Orcamentaria.Lib.Domain.Contexts;
using Orcamentaria.Lib.Domain.Enums;
using Orcamentaria.Lib.Domain.Exceptions;
using Orcamentaria.Lib.Domain.Models;
using Orcamentaria.Lib.Domain.Models.Exceptions;
using Orcamentaria.Lib.Domain.Validators;

namespace Orcamentaria.AuthService.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;
        private readonly IValidatorEntity<User> _validator;
        private readonly IPasswordService _passwordService;
        private readonly IPermissionService _permissionService;
        private readonly IMapper _mapper;
        private readonly IUserAuthContext _userAuthContext;

        public UserService(
            IUserRepository userRepository,
            IValidatorEntity<User> validator,
            IPasswordService passwordService,
            IPermissionService permissionService,
            IMapper mapper,
            IUserAuthContext userAuthContext)
        {
            _repository = userRepository;
            _validator = validator;
            _passwordService = passwordService;
            _permissionService = permissionService;
            _mapper = mapper;
            _userAuthContext = userAuthContext;
        }


        public User? GetById(long id)
        {
            try
            {
                return _repository.GetById(id);
            }
            catch (DefaultException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new UnexpectedException(ex.Message, ex);
            }
        }

        public Response<IEnumerable<UserResponseDTO>> GetByCompanyId()
        {
            try
            {
                var data = _repository.GetByCompanyId();

                if (!data.Any())
                    throw new InfoException($"Nenhum dado foi encontrado", ErrorCodeEnum.NotFound);

                return new Response<IEnumerable<UserResponseDTO>>(
                    _repository.GetByCompanyId()
                    .Select(x => _mapper.Map<User, UserResponseDTO>(x)));
            }
            catch (DefaultException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new UnexpectedException(ex.Message, ex);
            }
        }

        public User? GetUserByCredential(string email)
        {
            try
            {
                return _repository.GetByEmail(email); ;
            }
            catch (DefaultException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new UnexpectedException(ex.Message, ex);
            }
        }

        public Response<UserResponseDTO> GetByEmail(string email)
        {
            try
            {
                var data = _repository.GetByEmail(email);

                if(data is null)
                    throw new InfoException($"O {email} não foi encontrado", ErrorCodeEnum.NotFound);

                return new Response<UserResponseDTO>(_mapper.Map<User, UserResponseDTO>(data));
            }
            catch (DefaultException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new UnexpectedException(ex.Message, ex);
            }
        }

        public async Task<Response<UserResponseDTO>> Insert(UserInsertDTO dto)
        {
            try
            {
                var user = _mapper.Map<UserInsertDTO, User>(dto);

                var result = _validator.ValidateBeforeInsert(user);

                if (!result.IsValid)
                    throw new ValidationException(result);

                user.Password = _passwordService.Encript(user.Password);
                user.CompanyId = _userAuthContext.UserId;
            
                var entity = await _repository.Insert(user);

                return new Response<UserResponseDTO>(_mapper.Map<User, UserResponseDTO>(entity));
            }
            catch (DefaultException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new UnexpectedException(ex.Message, ex);
            }
        }

        public async Task<Response<UserResponseDTO>> Update(long id, UserUpdateDTO dto)
        {
            var user = _mapper.Map<UserUpdateDTO, User>(dto);

            user.Id = id;
            user.CompanyId = _userAuthContext.UserCompanyId;

            var result = _validator.ValidateBeforeUpdate(user);

            if (!result.IsValid)
                throw new ValidationException(result);

            try
            {
                var entity = await _repository.Update(id, user);

                return new Response<UserResponseDTO>(_mapper.Map<User, UserResponseDTO>(entity));
            }
            catch (DefaultException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new UnexpectedException(ex.Message, ex);
            }
        }

        public async Task<Response<UserResponseDTO>> UpdatePassword(long id, UserUpdatePasswordDTO dto)
        {
            try
            {
                if(_userAuthContext.UserId != id)
                    throw new UnauthorizedException("Você não possui permissão para executar essa ação.");

                if(_repository.GetById(id) is null)
                    throw new InfoException($"O {id} não foi encontrado", ErrorCodeEnum.NotFound);

                var result = _passwordService.ValidatePattern(dto.Password);

                if (!result.IsValid)
                    throw new ValidationException(result);

                var entity = await _repository.UpdatePassword(id, _passwordService.Encript(dto.Password));

                return new Response<UserResponseDTO>(_mapper.Map<User, UserResponseDTO>(entity));
            }
            catch (DefaultException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new UnexpectedException(ex.Message, ex);
            }
        }
        
        public async Task<Response<UserResponseDTO>> AddPermission(long userId, UserAddPermissionsDTO dto)
        {
            try
            {
                if(_repository.GetById(userId) is null)
                    throw new InfoException($"O {userId} não foi encontrado.", ErrorCodeEnum.NotFound);

                var addPermissions = new List<Permission>();

                foreach (var permissionId in dto.PermissionIds)
                {
                    var permission = _permissionService.GetPermission(permissionId);

                    if(permission is null)
                        throw new InfoException($"A permissão {permissionId} não foi encontrada.", ErrorCodeEnum.NotFound);

                    addPermissions.Add(permission);
                }
            
                await _repository.AddPermissions(userId, addPermissions);

                return new Response<UserResponseDTO>();
            }
            catch (DefaultException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new UnexpectedException(ex.Message, ex);
            }
        }

        public async Task<Response<UserResponseDTO>> RemovePermission(long userId, UserRemovePermissionsDTO dto)
        {
            try
            {
                if (_repository.GetById(userId) is null)
                    throw new InfoException($"O {userId} não foi encontrado.", ErrorCodeEnum.NotFound);

                var removePermissions = new List<Permission>();

                foreach (var permissionId in dto.PermissionIds)
                {
                    var permission = _permissionService.GetPermission(permissionId);
                
                    if (permission is null)
                        throw new InfoException($"A permissão {permissionId} não foi encontrada.", ErrorCodeEnum.NotFound);

                    removePermissions.Add(permission);

                }
                    await _repository.RemovePermissions(userId, removePermissions);

                return new Response<UserResponseDTO>();
            }
            catch (DefaultException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new UnexpectedException(ex.Message, ex);
            }
        }
    }
}
