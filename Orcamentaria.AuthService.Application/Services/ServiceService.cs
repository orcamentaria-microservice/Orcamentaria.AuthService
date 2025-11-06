using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Orcamentaria.AuthService.Domain.DTOs.Service;
using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.AuthService.Domain.Repositories;
using Orcamentaria.AuthService.Domain.Services;
using Orcamentaria.Lib.Domain.Enums;
using Orcamentaria.Lib.Domain.Exceptions;
using Orcamentaria.Lib.Domain.Models;
using Orcamentaria.Lib.Domain.Models.Exceptions;
using Orcamentaria.Lib.Domain.Models.Responses;
using Orcamentaria.Lib.Domain.Validators;

namespace Orcamentaria.AuthService.Application.Services
{
    public class ServiceService : IServiceService
    {
        private readonly IServiceRepository _repository;
        private readonly IServiceProvider _provider;
        private readonly IValidatorEntity<Service> _validator;
        private readonly IMapper _mapper;

        public ServiceService(
            IServiceRepository repository,
            IServiceProvider provider,
            IValidatorEntity<Service> validator,
            IMapper mapper)
        {
            _repository = repository;
            _provider = provider;
            _validator = validator;
            _mapper = mapper;
        }

        public async Task<Response<IEnumerable<ServiceResponseDTO>>?> GetAsync(GridParams gridParams)
        {
            try
            {
                var (data, pagination) = await _repository.GetAsync(gridParams);

                if (!data.Any())
                    throw new InfoException($"Nenhum dado foi encontrado.", ErrorCodeEnum.NotFound);

                return new Response<IEnumerable<ServiceResponseDTO>>(
                    data.Select(x => _mapper.Map<Service, ServiceResponseDTO>(x)), pagination);
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

        public Service GetByCredentials(string clientId, string clientSecret)
        {
            try
            {
                var data = _repository.GetByCredentials(clientId, clientSecret);

                if (data is null)
                    throw new InfoException($"O {clientId} e {clientSecret} nao foi encontrado", ErrorCodeEnum.NotFound);

                return data;
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

        public async Task<Service?> GetByIdAsync(long id)
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

        public async Task<Response<ServiceResponseDTO>> InsertAsync(ServiceInsertDTO dto)
        {
            try
            {
                var clientIdTokenService = _provider.GetRequiredKeyedService<ITokenService<Service>>("clientIdToken");
                var clientSecretTokenService = _provider.GetRequiredKeyedService<ITokenService<Service>>("clientSecretToken");

                var service = _mapper.Map<ServiceInsertDTO, Service>(dto);

                var clientId = clientIdTokenService.Generate(service);
                var clientSecret = clientSecretTokenService.Generate(service);

                service.ClientId = clientId;
                service.ClientSecret = clientSecret;

                var result = _validator.ValidateBeforeInsert(service);

                if (!result.IsValid)
                    throw new ValidationException(result);

                var entity = await _repository.InsertAsync(service);

                return new Response<ServiceResponseDTO>(_mapper.Map<Service, ServiceResponseDTO>(entity));
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

        public async Task<Response<ServiceResponseDTO>> UpdateAsync(long id, ServiceUpdateDTO dto)
        {
            try
            {
                var service = _mapper.Map<ServiceUpdateDTO, Service>(dto);

                service.Id = id;

                var result = _validator.ValidateBeforeInsert(service);

                if (!result.IsValid)
                    throw new ValidationException(result);

                var entity = await _repository.UpdateAsync(id, service);

                return new Response<ServiceResponseDTO>(_mapper.Map<Service, ServiceResponseDTO>(entity));
            }
            catch (DatabaseException)
            {
                throw;
            }
            catch (InfoException)
            {
                throw;
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new UnexpectedException(ex.Message, ex);
            }
        }

        public async Task<Response<ServiceResponseDTO>> UpdateCredentialsAsync(long id)
        {
            try
            {
                var entity = await _repository.GetByIdAsync(id);

                if (entity is null)
                    throw new InfoException($"O {id} nao foi encontrado.", ErrorCodeEnum.NotFound);

                var clientIdTokenService = _provider.GetRequiredKeyedService<ITokenService<Service>>("clientIdToken");
                var clientSecretTokenService = _provider.GetRequiredKeyedService<ITokenService<Service>>("clientSecretToken");

                var service = await _repository.GetByIdAsync(id);

                if (service is null)
                    throw new InfoException($"O {id} nao foi encontrado", ErrorCodeEnum.NotFound);

                entity.ClientId = clientIdTokenService.Generate(service);
                entity.ClientSecret = clientSecretTokenService.Generate(service);

                await _repository.UpdateAsync(id, entity);

                return new Response<ServiceResponseDTO>(_mapper.Map<Service, ServiceResponseDTO>(entity));
            }
            catch (DatabaseException)
            {
                throw;
            }
            catch (InfoException)
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
