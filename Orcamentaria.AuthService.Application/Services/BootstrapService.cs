using Microsoft.Extensions.DependencyInjection;
using Orcamentaria.AuthService.Domain.DTOs.Bootstrap;
using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.AuthService.Domain.Repositories;
using Orcamentaria.AuthService.Domain.Services;
using Orcamentaria.Lib.Domain.Enums;
using Orcamentaria.Lib.Domain.Exceptions;
using Orcamentaria.Lib.Domain.Models;
using Orcamentaria.Lib.Domain.Models.Exceptions;

namespace Orcamentaria.AuthService.Application.Services
{
    public class BootstrapService : IBootstrapService
    {
        private readonly IBootstrapRepository _repository;
        private readonly IServiceService _serviceService;
        private readonly IServiceProvider _provider;

        public BootstrapService(
            IBootstrapRepository repository,
            IServiceService serviceService,
            IServiceProvider provider)
        {
            _repository = repository;
            _serviceService = serviceService;
            _provider = provider;
        }

        public async Task<Response<BootstrapResponseDTO>> CreateBootstrapSecret(long serviceId)
        {
            try
            {
                var bootstrapSecretTokenService = _provider.GetRequiredKeyedService<ITokenService<Bootstrap>>("bootstrapSecretToken");
                
                var service = _serviceService.GetById(serviceId);

                if(!service.Success)
                    throw new InfoException($"O {serviceId} não foi encontrado", ErrorCodeEnum.NotFound);

                var bootstrap = _repository.GetActiveByServiceId(serviceId);

                if (bootstrap is not null)
                    await _repository.Inactive(bootstrap.Id);

                var newBootstrap = new Bootstrap
                {
                    ServiceId = serviceId,
                    Active = true,
                    CreateAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddHours(6),
                    Hash = "HashPlaceholder"
                };

                var insertedBootstrap =  await _repository.Insert(newBootstrap);

                var secretAndHash = bootstrapSecretTokenService.Generate(insertedBootstrap);

                var parts = secretAndHash.Split(" || ");

                await _repository.UpdateHash(insertedBootstrap.Id, parts[1]);

                var response = new BootstrapResponseDTO
                {
                    BootstrapId = insertedBootstrap.Id,
                    ServiceId = serviceId,
                    ServiceName = service.Data.Name,
                    BootstrapSecret = parts[0],
                    ExpiresAt = insertedBootstrap.ExpiresAt
                };

                return new Response<BootstrapResponseDTO>(response);
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

        public Response<BootstrapResponseDTO> GetById(long id)
        {
            try
            {
                var bootstrap = _repository.GetById(id);

                if (bootstrap is null)
                    throw new InfoException($"O {id} não encontrado", ErrorCodeEnum.NotFound);

                var response = new BootstrapResponseDTO
                {
                    BootstrapId = bootstrap.Id,
                    ServiceId = bootstrap.ServiceId,
                    ExpiresAt = bootstrap.ExpiresAt
                };

                return new Response<BootstrapResponseDTO>(response);
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

        public Response<long> RevokeBootstrapSecret(long serviceId)
        {
            try
            {
                var bootstrap = _repository.GetActiveByServiceId(serviceId);

                if(bootstrap is null)
                    throw new InfoException("Token de bootstrap ativo não encontrado para o serviço informado", ErrorCodeEnum.NotFound);

                _repository.Inactive(bootstrap.Id);

                return new Response<long>(serviceId);
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
