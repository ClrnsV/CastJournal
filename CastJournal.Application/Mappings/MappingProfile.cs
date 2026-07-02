using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CastJournal.Application.DTOs;
using CastJournal.Application.DTOs.Catches;
using CastJournal.Domain.Entities;

namespace CastJournal.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Catch Mappings
        CreateMap<Catch, CatchDto>()
            .ForMember(dest => dest.SpeciesName, opt => opt.MapFrom(src => src.Species != null ? src.Species.CommonName : null))
            .ForMember(dest => dest.LocationName, opt => opt.MapFrom(src => src.Location != null ? src.Location.Name : null));

        CreateMap<CreateCatchDto, Catch>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.Species, opt => opt.Ignore())
            .ForMember(dest => dest.Location, opt => opt.Ignore())
            .ForMember(dest => dest.Media, opt => opt.Ignore());

        CreateMap<CatchMedia, CatchMediaDto>();

        // Add more mappings when creating more DTOs laterrr
    }
}