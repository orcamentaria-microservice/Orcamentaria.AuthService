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
using Orcamentaria.Lib.Domain.Models.Responses;
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


        public async Task<User?> GetByIdAsync(long id)
        {
            try
            {
                return await _repository.GetByIdAsync(id);
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

        public User? GetByEmail(string email)
        {
            try
            {
                return _repository.GetByEmail(email);
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

        public async Task<Response<IEnumerable<UserResponseDTO>>?> GetAsync(GridParams gridParams)
        {
            try
            {
                var (data, pagination) = await _repository.GetAsync(gridParams);

                if (!data.Any())
                    throw new InfoException($"Nenhum dado foi encontrado.", ErrorCodeEnum.NotFound);

                return new Response<IEnumerable<UserResponseDTO>>(
                    data.Select(x => _mapper.Map<User, UserResponseDTO>(x)), pagination);
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

        public async Task<Response<UserResponseDTO>> InsertAsync(UserInsertDTO dto)
        {
            try
            {
                var user = _mapper.Map<UserInsertDTO, User>(dto);

                var result = _validator.ValidateBeforeInsert(user);

                if (!result.IsValid)
                    throw new ValidationException(result);

                user.Password = _passwordService.Encript(user.Password);
                user.CompanyId = _userAuthContext.UserId;
            
                var entity = await _repository.InsertAsync(user);

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

        public async Task<Response<UserResponseDTO>> UpdateAsync(long id, UserUpdateDTO dto)
        {
            var user = _mapper.Map<UserUpdateDTO, User>(dto);

            user.Id = id;

            var result = _validator.ValidateBeforeUpdate(user);

            if (!result.IsValid)
                throw new ValidationException(result);

            try
            {
                var entity = await _repository.UpdateAsync(id, user);

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

        public async Task<Response<UserResponseDTO>> UpdatePasswordAsync(long id, UserUpdatePasswordDTO dto)
        {
            try
            {
                if(_userAuthContext.UserId != id)
                    throw new InfoException("Voce nao possui permissao para executar essa acao.", ErrorCodeEnum.Unauthorized);

                if(await _repository.GetByIdAsync(id) is null)
                    throw new InfoException($"O {id} nao foi encontrado.", ErrorCodeEnum.NotFound);

                var result = _passwordService.ValidatePattern(dto.Password);

                if (!result.IsValid)
                    throw new ValidationException(result);

                var entity = await _repository.UpdatePasswordAsync(id, _passwordService.Encript(dto.Password));

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
        
        public async Task<Response<UserResponseDTO>> AddPermissionsAsync(long userId, UserAddPermissionsDTO dto)
        {
            try
            {
                if(await _repository.GetByIdAsync(userId) is null)
                    throw new InfoException($"O {userId} nao foi encontrado.", ErrorCodeEnum.NotFound);

                var addPermissions = new List<Permission>();

                foreach (var permissionId in dto.PermissionIds)
                {
                    var permission = await _permissionService.GetByIdAsync(permissionId);

                    if(permission is null)
                        throw new InfoException($"A permissao {permissionId} nao foi encontrada.", ErrorCodeEnum.NotFound);

                    addPermissions.Add(permission);
                }
            
                await _repository.AddPermissionsAsync(userId, addPermissions);

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

        public async Task<Response<UserResponseDTO>> RemovePermissionsAsync(long userId, UserRemovePermissionsDTO dto)
        {
            try
            {
                if (await _repository.GetByIdAsync(userId) is null)
                    throw new InfoException($"O {userId} nao foi encontrado.", ErrorCodeEnum.NotFound);

                var removePermissions = new List<Permission>();

                foreach (var permissionId in dto.PermissionIds)
                {
                    var permission = await _permissionService.GetByIdAsync(permissionId);
                
                    if (permission is null)
                        throw new InfoException($"A permissao {permissionId} nao foi encontrada.", ErrorCodeEnum.NotFound);

                    removePermissions.Add(permission);

                }
                    await _repository.RemovePermissionsAsync(userId, removePermissions);

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
