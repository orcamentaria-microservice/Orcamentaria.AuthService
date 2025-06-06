using AutoMapper;
using Orcamentaria.AuthService.Domain.DTOs.Permissions;
using Orcamentaria.Lib.Domain.Enums;
using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.AuthService.Domain.Repositories;
using Orcamentaria.AuthService.Domain.Services;
using Orcamentaria.Lib.Domain.Models;
using Orcamentaria.Lib.Domain.Validators;
using Orcamentaria.Lib.Domain.Models.Exceptions;
using Orcamentaria.Lib.Domain.Exceptions;

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
        {
            try
            {
                var data = _repository.GetById(id);

                if (data is null)
                    throw new InfoException($"O {id} não foi encontrado", ErrorCodeEnum.NotFound);

                return new Response<PermissionResponseDTO>(
                    _mapper.Map<Permission, PermissionResponseDTO>(data));
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

        public Response<IEnumerable<PermissionResponseDTO>> GetByResource(ResourceEnum resource)
        {
            try
            {
                var data = _repository.GetByResource(resource);

                if (!data.Any())
                    throw new InfoException($"Nenhum dado foi encontrado", ErrorCodeEnum.NotFound);

                return new Response<IEnumerable<PermissionResponseDTO>>(
                    data.Select(x => _mapper.Map<Permission, PermissionResponseDTO>(x)));
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

        public Response<IEnumerable<PermissionResponseDTO>> GetByType(PermissionTypeEnum type)
        {
            try
            {
                var data = _repository.GetByType(type);

                if (!data.Any())
                    throw new InfoException($"Nenhum dado foi encontrado", ErrorCodeEnum.NotFound);

                return new Response<IEnumerable<PermissionResponseDTO>>(
                    data.Select(x => _mapper.Map<Permission, PermissionResponseDTO>(x)));
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

        public Permission? GetPermission(long id)
        {
            try
            {
                return _repository.GetById(id);
            }
            catch(DefaultException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new UnexpectedException(ex.Message, ex);
            }
        }

        public async Task<Response<PermissionResponseDTO>> Insert(PermissionInsertDTO dto)
        {
            try
            {
                var permission = _mapper.Map<PermissionInsertDTO, Permission>(dto);

                permission.IncrementalPermission = permission.IncrementalPermission.ToUpper();

                var result = _validator.ValidateBeforeInsert(permission);

                if(!result.IsValid)
                    throw new ValidationException(result);

                var entity = await _repository.Insert(permission);

                return new Response<PermissionResponseDTO>(_mapper.Map<Permission, PermissionResponseDTO>(entity));
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

        public async Task<Response<PermissionResponseDTO>> Update(long id, PermissionUpdateDTO dto)
        {
            try
            {
                if (_repository.GetById(id) is null)
                    throw new InfoException($"O {id} não foi encontrado", ErrorCodeEnum.NotFound);

                var permission = _mapper.Map<PermissionUpdateDTO, Permission>(dto);

                permission.IncrementalPermission = permission.IncrementalPermission.ToUpper();
                permission.Id = id;

                var result = _validator.ValidateBeforeUpdate(permission);

                if (!result.IsValid)
                    throw new ValidationException(result);

                var entity = await _repository.Update(id, permission);

                return new Response<PermissionResponseDTO>(_mapper.Map<Permission, PermissionResponseDTO>(entity));
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
