using AutoMapper;
using Orcamentaria.AuthService.Domain.DTOs.Service;
using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.AuthService.Domain.Repositories;
using Orcamentaria.AuthService.Domain.Services;
using Orcamentaria.Lib.Domain.Enums;
using Orcamentaria.Lib.Domain.Models;
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
            => _repository.GetByCredentials(clientId, clientSecret);

        public Response<ServiceResponseDTO> GetById(long id)
            => new Response<ServiceResponseDTO>(
                _mapper.Map<Service, ServiceResponseDTO>(_repository.GetById(id)));

        public async Task<Response<ServiceResponseDTO>> Insert(ServiceInsertDTO dto)
        {
            var service = _mapper.Map<ServiceInsertDTO, Service>(dto);

            var keys = _tokenService.GenerateSecrets(service);

            service.ClientId = keys["clientId"];
            service.ClientSecret = keys["clientSecret"];

            var result = _validator.ValidateBeforeInsert(service);

            if (!result.IsValid)
                return new Response<ServiceResponseDTO>(result);

            try
            {
                var entity = await _repository.Insert(service);

                return new Response<ServiceResponseDTO>(_mapper.Map<Service, ServiceResponseDTO>(entity));
            }
            catch (Exception ex)
            {
                return new Response<ServiceResponseDTO>(ResponseErrorEnum.DatabaseError, ex.Message); ;
            }
        }

        public async Task<Response<ServiceResponseDTO>> Update(long id, ServiceUpdateDTO dto)
        {
            var service = _mapper.Map<ServiceUpdateDTO, Service>(dto);

            service.Id = id;

            var result = _validator.ValidateBeforeInsert(service);

            if (!result.IsValid)
                return new Response<ServiceResponseDTO>(result);

            try
            {
                var entity = await _repository.Update(id, service);

                return new Response<ServiceResponseDTO>(_mapper.Map<Service, ServiceResponseDTO>(entity));
            }
            catch (Exception ex)
            {
                return new Response<ServiceResponseDTO>(ResponseErrorEnum.DatabaseError, ex.Message); ;
            }
        }

        public async Task<Response<ServiceResponseDTO>> UpdateCredentials(long id)
        {
            var service = _repository.GetById(id);

            if (service is null)
                return new Response<ServiceResponseDTO>(ResponseErrorEnum.NotFound,  "Serviço não encontrado.");

            var keys = _tokenService.GenerateSecrets(service);

            try
            {
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
            catch (Exception ex)
            {
                return new Response<ServiceResponseDTO>(ResponseErrorEnum.DatabaseError, ex.Message);
            }
        }
    }
}
