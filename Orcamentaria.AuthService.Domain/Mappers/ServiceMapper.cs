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
                .ForMember(s => s.CreatedAt, opt => opt.MapFrom(d => d.CreatedAt))
                .ForMember(s => s.CreatedBy, opt => opt.MapFrom(d => d.CreatedBy))
                .ForMember(s => s.UpdatedAt, opt => opt.MapFrom(d => d.UpdatedAt))
                .ForMember(s => s.UpdatedBy, opt => opt.MapFrom(d => d.UpdatedBy))
                .ReverseMap();
        }
    }
}
