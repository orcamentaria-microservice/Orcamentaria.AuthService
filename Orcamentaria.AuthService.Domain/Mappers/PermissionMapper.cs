﻿using Orcamentaria.AuthService.Domain.Models;
using AutoMapper;
using Orcamentaria.AuthService.Domain.DTOs.Permission;

namespace Orcamentaria.AuthService.Domain.Mappers
{
    public class PermissionMapper : Profile
    {
        public PermissionMapper() 
        {
            CreateMap<Permission, PermissionInsertDTO>()
                .ForMember(s => s.Resource, opt => opt.MapFrom(d => d.Resource))
                .ForMember(s => s.Description, opt => opt.MapFrom(d => d.Description))
                .ForMember(s => s.Type, opt => opt.MapFrom(d => d.Type))
                .ForMember(s => s.IncrementalPermission, opt => opt.MapFrom(d => d.IncrementalPermission))
                .ReverseMap();

            CreateMap<Permission, PermissionUpdateDTO>()
                .ForMember(s => s.Resource, opt => opt.MapFrom(d => d.Resource))
                .ForMember(s => s.Description, opt => opt.MapFrom(d => d.Description))
                .ForMember(s => s.Type, opt => opt.MapFrom(d => d.Type))
                .ForMember(s => s.IncrementalPermission, opt => opt.MapFrom(d => d.IncrementalPermission))
                .ReverseMap();

            CreateMap<PermissionResponseDTO, Permission>()
                .ForMember(s => s.Id, opt => opt.MapFrom(d => d.Id))
                .ForMember(s => s.Resource, opt => opt.MapFrom(d => d.Resource))
                .ForMember(s => s.Description, opt => opt.MapFrom(d => d.Description))
                .ForMember(s => s.Type, opt => opt.MapFrom(d => d.Type))
                .ForMember(s => s.IncrementalPermission, opt => opt.MapFrom(d => d.IncrementalPermission))
                .ForMember(s => s.CreateAt, opt => opt.MapFrom(d => d.CreateAt))
                .ReverseMap();
        }
    }
}
