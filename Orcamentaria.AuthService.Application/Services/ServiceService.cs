using AutoMapper;
using Orcamentaria.AuthService.Domain.DTOs.Service;
using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.AuthService.Domain.Repositories;
using Orcamentaria.AuthService.Domain.Services;
using Orcamentaria.Lib.Domain.Enums;
using Orcamentaria.Lib.Domain.Exceptions;
using Orcamentaria.Lib.Domain.Models;
using Orcamentaria.Lib.Domain.Models.Exceptions;
using Orcamentaria.Lib.Domain.Validators;

namespace Orcamentaria.AuthService.Application.Services
{
    public class ServiceService : IServiceService
    {
        private readonly IServiceRepository _repository;
        private readonly ITokenService _tokenService;
        private readonly IValidatorEntity<Service> _validator;
        private readonly IMapper _mapper;

        public ServiceService(
            IServiceRepository repository, 
            ITokenService tokenService,
            IValidatorEntity<Service> validator,
            IMapper mapper)
        {
            _repository = repository;
            _tokenService = tokenService;
            _validator = validator;
            _mapper = mapper;
        }

        public Service GetByCredentials(string clientId, string clientSecret)
        {
            try
            {
                var data = _repository.GetByCredentials(clientId, clientSecret);

                if (data is null)
                    throw new InfoException($"O {clientId} e {clientSecret} não foi encontrado", ErrorCodeEnum.NotFound);

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

        public Response<ServiceResponseDTO> GetById(long id)
        {
            try
            {
                var data = _repository.GetById(id);

                if (data is null)
                    throw new InfoException($"O {id} não foi encontrado", ErrorCodeEnum.NotFound);
                
                return new Response<ServiceResponseDTO>(
                    _mapper.Map<Service, ServiceResponseDTO>(data));
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

        public async Task<Response<ServiceResponseDTO>> Insert(ServiceInsertDTO dto)
        {
            try
            {
                var service = _mapper.Map<ServiceInsertDTO, Service>(dto);

                var keys = _tokenService.GenerateSecrets(service);

                service.ClientId = keys["clientId"];
                service.ClientSecret = keys["clientSecret"];

                var result = _validator.ValidateBeforeInsert(service);

                if (!result.IsValid)
                    throw new ValidationException(result);

                var entity = await _repository.Insert(service);

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

        public async Task<Response<ServiceResponseDTO>> Update(long id, ServiceUpdateDTO dto)
        {
            try
            {
                if (_repository.GetById(id) is null)
                    throw new InfoException($"O {id} não foi encontrado", ErrorCodeEnum.NotFound);

                var service = _mapper.Map<ServiceUpdateDTO, Service>(dto);

                service.Id = id;

                var result = _validator.ValidateBeforeInsert(service);

                if (!result.IsValid)
                    throw new ValidationException(result);

                var entity = await _repository.Update(id, service);

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

        public async Task<Response<ServiceResponseDTO>> UpdateCredentials(long id)
        {
            try
            {
                var service = _repository.GetById(id);

                if (service is null)
                    throw new InfoException($"O {id} não foi encontrado", ErrorCodeEnum.NotFound);

                var keys = _tokenService.GenerateSecrets(service);

                await _repository.UpdateCredentials(id, keys["clientId"], keys["clientSecret"]);

                return new Response<ServiceResponseDTO>(
                    new ServiceResponseDTO
                    {
                        Id = id,
                        Name = service.Name,
                        ClientId = keys["clientId"],
                        ClientSecret = keys["clientSecret"] 
                    });
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
