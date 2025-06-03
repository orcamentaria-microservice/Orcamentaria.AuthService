using AutoMapper;
using Orcamentaria.AuthService.Domain.DTOs.User;
using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.AuthService.Domain.Repositories;
using Orcamentaria.AuthService.Domain.Services;
using Orcamentaria.Lib.Domain.Contexts;
using Orcamentaria.Lib.Domain.Enums;
using Orcamentaria.Lib.Domain.Models;
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

        
        public User GetById(long id)
            => _repository.GetById(id);

        public Response<IEnumerable<UserResponseDTO>> GetByCompanyId()
            => new Response<IEnumerable<UserResponseDTO>>(
                _repository.GetByCompanyId()
                .Select(x => _mapper.Map<User, UserResponseDTO>(x)));

        public User GetUserByCredential(string email)
            => _repository.GetByEmail(email);

        public Response<UserResponseDTO> GetByEmail(string email)
            => new Response<UserResponseDTO>(_mapper.Map<User, UserResponseDTO>(_repository.GetByEmail(email)));

        public async Task<Response<UserResponseDTO>> Insert(UserInsertDTO dto)
        {
            var user = _mapper.Map<UserInsertDTO, User>(dto);

            var result = _validator.ValidateBeforeInsert(user);

            if (!result.IsValid)
                return new Response<UserResponseDTO>(result);

            user.Password = _passwordService.Encript(user.Password);
            user.CompanyId = 10;
            
            try
            {
                var entity = await _repository.Insert(user);

                return new Response<UserResponseDTO>(_mapper.Map<User, UserResponseDTO>(entity));
            }
            catch (Exception ex)
            {
                return new Response<UserResponseDTO>(ResponseErrorEnum.DatabaseError, ex.Message); ;
            }
        }

        public async Task<Response<UserResponseDTO>> Update(long id, UserUpdateDTO dto)
        {
            var user = _mapper.Map<UserUpdateDTO, User>(dto);

            user.Id = id;
            user.CompanyId = _userAuthContext.UserCompanyId;

            var result = _validator.ValidateBeforeUpdate(user);

            if (!result.IsValid)
                return new Response<UserResponseDTO>(result);

            try
            {
                var entity = await _repository.Update(id, user);

                return new Response<UserResponseDTO>(_mapper.Map<User, UserResponseDTO>(entity));
            }
            catch (Exception ex)
            {
                return new Response<UserResponseDTO>(ResponseErrorEnum.DatabaseError, ex.Message);
            }
        }

        public async Task<Response<UserResponseDTO>> UpdatePassword(long id, UserUpdatePasswordDTO dto)
        {
            if(_userAuthContext.UserId != id)
                return new Response<UserResponseDTO>(
                    ResponseErrorEnum.BusinessRuleViolation, "Você não possui permissão para executar essa ação.");

            var result = _passwordService.ValidatePattern(dto.Password);

            if (!result.IsValid)
                return new Response<UserResponseDTO>(result);

            try
            {
                var entity = await _repository.UpdatePassword(id, _passwordService.Encript(dto.Password));

                return new Response<UserResponseDTO>(_mapper.Map<User, UserResponseDTO>(entity));
            }
            catch (Exception ex)
            {
                return new Response<UserResponseDTO>(ResponseErrorEnum.DatabaseError, ex.Message);
            }
        }
        
        public async Task<Response<UserResponseDTO>> AddPermission(long userId, IEnumerable<long> permissionsId)
        {
            if(_repository.GetById(userId) is null)
                return new Response<UserResponseDTO>(ResponseErrorEnum.NotFound, "Usuário inválido.");

            var addPermissions = new List<Permission>();

            foreach (var permissionId in permissionsId)
            {
                var permission = _permissionService.GetPermission(permissionId);
                if (permission is null)
                    return new Response<UserResponseDTO>(ResponseErrorEnum.NotFound, $"Permissão {permissionId} inválida.");

                addPermissions.Add(permission);
            }
            
            await _repository.AddPermissions(userId, addPermissions);

            return new Response<UserResponseDTO>();
        }

        public async Task<Response<UserResponseDTO>> RemovePermission(long userId, IEnumerable<long> permissionsId)
        {
            if (_repository.GetById(userId) is null)
                return new Response<UserResponseDTO>(ResponseErrorEnum.NotFound, "Usuário inválido.");

            var removePermissions = new List<Permission>();

            foreach (var permissionId in permissionsId)
            {
                var permission = _permissionService.GetPermission(permissionId);
                if (permission is null)
                    return new Response<UserResponseDTO>(ResponseErrorEnum.NotFound, $"Permissão {permissionId} inválida.");

                removePermissions.Add(permission);

            }
                await _repository.RemovePermissions(userId, removePermissions);

            return new Response<UserResponseDTO>();
        }
    }
}
