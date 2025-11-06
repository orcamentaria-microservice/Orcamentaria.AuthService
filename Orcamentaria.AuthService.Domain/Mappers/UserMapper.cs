using Orcamentaria.AuthService.Domain.DTOs.User;
using Orcamentaria.AuthService.Domain.Models;
using AutoMapper;

namespace Orcamentaria.AuthService.Domain.Mappers
{
    public class UserMapper : Profile
    {
        public UserMapper() 
        {
            CreateMap<User, UserInsertDTO>()
                .ForMember(s => s.Name, opt => opt.MapFrom(d => d.Name))
                .ForMember(s => s.Email, opt => opt.MapFrom(d => d.Email))
                .ForMember(s => s.Password, opt => opt.MapFrom(d => d.Password))
                .ReverseMap();

            CreateMap<User, UserUpdateDTO>()
                .ForMember(s => s.Name, opt => opt.MapFrom(d => d.Name))
                .ForMember(s => s.Active, opt => opt.MapFrom(d => d.Active))
                .ReverseMap();

            CreateMap<UserResponseDTO, User>()
                .ForMember(s => s.Id, opt => opt.MapFrom(d => d.Id))
                .ForMember(s => s.Name, opt => opt.MapFrom(d => d.Name))
                .ForMember(s => s.Email, opt => opt.MapFrom(d => d.Email))
                .ForMember(s => s.CompanyId, opt => opt.MapFrom(d => d.CompanyId))
                .ForMember(s => s.CreatedAt, opt => opt.MapFrom(d => d.CreatedAt))
                .ForMember(s => s.CreatedBy, opt => opt.MapFrom(d => d.CreatedBy))
                .ForMember(s => s.UpdatedAt, opt => opt.MapFrom(d => d.UpdatedAt))
                .ForMember(s => s.UpdatedBy, opt => opt.MapFrom(d => d.UpdatedBy))
                .ReverseMap();
        }
    }
}
