using Microsoft.Extensions.DependencyInjection;
using Orcamentaria.AuthService.Domain.DTOs.Bootstrap;
using Orcamentaria.AuthService.Domain.Models;
using Orcamentaria.AuthService.Domain.Repositories;
using Orcamentaria.AuthService.Domain.Services;
using Orcamentaria.Lib.Domain.Enums;
using Orcamentaria.Lib.Domain.Exceptions;
using Orcamentaria.Lib.Domain.Models;
using Orcamentaria.Lib.Domain.Models.Exceptions;
using Orcamentaria.Lib.Domain.Models.Responses;

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

        public async Task<Response<BootstrapResponseDTO>> GenerateBootstrapSecretAsync(long serviceId)
        {
            try
            {
                var bootstrapSecretTokenService = _provider.GetRequiredKeyedService<ITokenService<Bootstrap>>("bootstrapSecretToken");

                var service = await _serviceService.GetByIdAsync(serviceId);

                if (service is null)
                    throw new InfoException($"O id do servico {serviceId} nao foi encontrado.", ErrorCodeEnum.NotFound);

                var gridParams = new GridParams
                {
                    Filters = new List<FilterParam>
                    {
                        new FilterParam
                        {
                            Field = "ServiceId",
                            Operator = "eq",
                            Value = serviceId.ToString()
                        },
                        new FilterParam
                        {
                            Field = "Active",
                            Operator = "eq",
                            Value = (1).ToString()
                        },
                    }
                };

                var (bootstrap, paginataion) = await _repository.GetAsync(gridParams);

                if (bootstrap.Any())
                    await _repository.Inactive(bootstrap.First().Id);

                var newBootstrap = new Bootstrap
                {
                    ServiceId = serviceId,
                    Active = true,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddHours(6),
                    Hash = "HashPlaceholder"
                };

                var insertedBootstrap =  await _repository.InsertAsync(newBootstrap);

                var secretAndHash = bootstrapSecretTokenService.Generate(insertedBootstrap);

                var parts = secretAndHash.Split(" || ");

                await _repository.UpdateHash(insertedBootstrap.Id, parts[1]);

                var response = new BootstrapResponseDTO
                {
                    BootstrapId = insertedBootstrap.Id,
                    ServiceId = serviceId,
                    ServiceName = service.Name,
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

        public async Task<Bootstrap?> GetByIdAsync(long id)
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

        public async Task<Response<long>> RevokeBootstrapSecretAsync(long serviceId)
        {
            try
            {
                var gridParams = new GridParams
                {
                    Filters = new List<FilterParam>
                    {
                        new FilterParam
                        {
                            Field = "ServiceId",
                            Operator = "eq",
                            Value = serviceId.ToString()
                        },
                        new FilterParam
                        {
                            Field = "Active",
                            Operator = "eq",
                            Value = (true).ToString()
                        },
                    }
                };

                var (bootstrap, paginataion) = await _repository.GetAsync(gridParams);

                if (!bootstrap.Any())
                    throw new InfoException("Nao ha bootstrap secret para ser revogado.", ErrorCodeEnum.NotFound);

                _repository.Inactive(bootstrap.First().Id);

                return new Response<long>(serviceId, "Bootstrap secret revogado com sucesso.");
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
