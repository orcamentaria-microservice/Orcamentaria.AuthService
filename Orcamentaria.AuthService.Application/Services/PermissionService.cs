using AutoMapper;
using Orcamentaria.AuthService.Domain.DTOs.Permission;
using Orcamentaria.Lib.Domain.Enums;
using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.AuthService.Domain.Repositories;
using Orcamentaria.AuthService.Domain.Services;
using Orcamentaria.Lib.Domain.Models;
using Orcamentaria.Lib.Domain.Validators;
using Orcamentaria.Lib.Domain.Models.Exceptions;
using Orcamentaria.Lib.Domain.Exceptions;
using Orcamentaria.Lib.Domain.Models.Responses;

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
        public async Task<Permission?> GetByIdAsync(long id)
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

        public async Task<Response<IEnumerable<PermissionResponseDTO>>?> GetAsync(GridParams gridParams)
        {
            try
            {
                var (data, pagination) = await _repository.GetAsync(gridParams);

                if (!data.Any())
                    throw new InfoException($"Nenhum dado foi encontrado.", ErrorCodeEnum.NotFound);

                return new Response<IEnumerable<PermissionResponseDTO>>(
                    data.Select(x => _mapper.Map<Permission, PermissionResponseDTO>(x)), pagination);
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

        public async Task<Response<PermissionResponseDTO>> InsertAsync(PermissionInsertDTO dto)
        {
            try
            {
                var permission = _mapper.Map<PermissionInsertDTO, Permission>(dto);

                permission.IncrementalPermission = permission.IncrementalPermission.ToUpper();

                var result = _validator.ValidateBeforeInsert(permission);

                if(!result.IsValid)
                    throw new ValidationException(result);

                var entity = await _repository.InsertAsync(permission);

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

        public async Task<Response<PermissionResponseDTO>> UpdateAsync(long id, PermissionUpdateDTO dto)
        {
            try
            {
                var permission = _mapper.Map<PermissionUpdateDTO, Permission>(dto);

                permission.IncrementalPermission = permission.IncrementalPermission.ToUpper();
                permission.Id = id;

                var result = _validator.ValidateBeforeUpdate(permission);

                if (!result.IsValid)
                    throw new ValidationException(result);

                var entity = await _repository.UpdateAsync(id, permission);

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
