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

        public User GetByCredentials(string email, string password)
            => _repository.GetByCredentials(email, password);

        public Response<UserResponseDTO> GetByEmail(string email)
            => new Response<UserResponseDTO>(_mapper.Map<User, UserResponseDTO>(_repository.GetByEmail(email)));

        public async Task<Response<UserResponseDTO>> Insert(UserInsertDTO dto)
        {
            var user = _mapper.Map<UserInsertDTO, User>(dto);

            var result = _validator.ValidateBeforeInsert(user);

            if (!result.IsValid)
                return new Response<UserResponseDTO>(result);

            user.Password = _passwordService.Encript(user.Password);
            user.CompanyId = _userAuthContext.UserCompanyId;

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
            var result = _passwordService.Validate(dto.Password);

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

            foreach (var permissionId in permissionsId)
            {
                if (!_permissionService.GetById(permissionId).Success)
                    return new Response<UserResponseDTO>(ResponseErrorEnum.NotFound, $"Permissão {permissionId} inválida.");

                await _repository.AddPermission(userId, permissionId);
            }

            return new Response<UserResponseDTO>();
        }

        public async Task<Response<UserResponseDTO>> RemovePermission(long userId, IEnumerable<long> permissionsId)
        {
            if (_repository.GetById(userId) is null)
                return new Response<UserResponseDTO>(ResponseErrorEnum.NotFound, "Usuário inválido.");

            foreach (var permissionId in permissionsId)
            {
                if (!_permissionService.GetById(permissionId).Success)
                    return new Response<UserResponseDTO>(ResponseErrorEnum.NotFound, $"Permissão {permissionId} inválida.");

                await _repository.AddPermission(userId, permissionId);
            }

            return new Response<UserResponseDTO>();
        }
    }
}
