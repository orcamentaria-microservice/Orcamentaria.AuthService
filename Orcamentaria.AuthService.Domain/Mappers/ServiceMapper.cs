using Orcamentaria.AuthService.Domain.Models;
using AutoMapper;
using Orcamentaria.AuthService.Domain.DTOs.Service;

namespace Orcamentaria.AuthService.Domain.Mappers
{
    public class ServiceMapper : Profile
    {
        public ServiceMapper() 
        {
            CreateMap<Service, ServiceInsertDTO>()
                .ForMember(s => s.Name, opt => opt.MapFrom(d => d.Name))
                .ReverseMap();

            CreateMap<Service, ServiceUpdateDTO>()
                .ForMember(s => s.Name, opt => opt.MapFrom(d => d.Name))
                .ReverseMap();

            CreateMap<ServiceResponseDTO, Service>()
                .ForMember(s => s.Id, opt => opt.MapFrom(d => d.Id))
                .ForMember(s => s.Name, opt => opt.MapFrom(d => d.Name))
                .ForMember(s => s.ClientId, opt => opt.MapFrom(d => d.ClientId))
                .ForMember(s => s.ClientSecret, opt => opt.MapFrom(d => d.ClientSecret))
                .ReverseMap();
        }
    }
}
