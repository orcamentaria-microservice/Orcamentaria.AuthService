using AutoMapper;
using Orcamentaria.AuthService.Domain.DTOs.Permissions;
using Orcamentaria.Lib.Domain.Enums;
using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.AuthService.Domain.Repositories;
using Orcamentaria.AuthService.Domain.Services;
using Orcamentaria.Lib.Domain.Enums;
using Orcamentaria.Lib.Domain.Models;
using Orcamentaria.Lib.Domain.Validators;
using System.Linq.Expressions;

namespace Orcamentaria.AuthService.Application.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly IPermissionRepository _repository;
        private readonly IMapper _mapper;
        private readonly IValidatorEntity<Permission> _validator;

        public PermissionService(
            IPermissionRepository repository, 
            IMapper mapper,
            IValidatorEntity<Permission> validator)
        {
            _repository = repository;
            _mapper = mapper;
            _validator = validator;
        }

        public Response<PermissionResponseDTO> GetById(long id)
            => new Response<PermissionResponseDTO>(
                _mapper.Map<Permission, PermissionResponseDTO>(_repository.GetById(id)));

        public Response<IEnumerable<PermissionResponseDTO>> GetByResource(ResourceEnum resource)
            => new Response<IEnumerable<PermissionResponseDTO>>(
                _repository.GetByResource(resource)
                .Select(x => _mapper.Map<Permission, PermissionResponseDTO>(x)));

        public Response<IEnumerable<PermissionResponseDTO>> GetByType(PermissionTypeEnum type)
            => new Response<IEnumerable<PermissionResponseDTO>>(
                    _repository.GetByType(type)
                    .Select(x => _mapper.Map<Permission, PermissionResponseDTO>(x)));

        public async Task<Response<PermissionResponseDTO>> Insert(PermissionInsertDTO dto)
        {
            var permission = _mapper.Map<PermissionInsertDTO, Permission>(dto);

            var result = _validator.ValidateBeforeInsert(permission);

            if(!result.IsValid)
                return new Response<PermissionResponseDTO>(result);

            try
            {
                var entity = await _repository.Insert(permission);

                return new Response<PermissionResponseDTO>(_mapper.Map<Permission, PermissionResponseDTO>(entity));
            }
            catch (Exception ex)
            {
                return new Response<PermissionResponseDTO>(ResponseErrorEnum.DatabaseError, ex.Message);
            }
        }

        public async Task<Response<PermissionResponseDTO>> Update(long id, PermissionUpdateDTO dto)
        {
            var permission = _mapper.Map<PermissionUpdateDTO, Permission>(dto);

            permission.Id = id;

            var result = _validator.ValidateBeforeUpdate(permission);

            if (!result.IsValid)
                return new Response<PermissionResponseDTO>(result);

            try
            {
                var entity = await _repository.Update(id, permission);

                return new Response<PermissionResponseDTO>(_mapper.Map<Permission, PermissionResponseDTO>(entity));
            }
            catch (Exception ex)
            {
                return new Response<PermissionResponseDTO>(ResponseErrorEnum.DatabaseError, ex.Message);
            }
        }
    }
}
