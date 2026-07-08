using AutoMapper;
using CastJournal.Application.DTOs;
using CastJournal.Application.DTOs.Catches;
using CastJournal.Application.DTOs.Locations;
using CastJournal.Application.DTOs.Notifications;
using CastJournal.Application.DTOs.Profile;
using CastJournal.Application.DTOs.Species;
using CastJournal.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace CastJournal.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Catch Mappings
        CreateMap<Catch, CatchDto>()
            .ForMember(dest => dest.UserFullName, opt => opt.MapFrom(src => src.User != null ? src.User.FullName : null))
            .ForMember(dest => dest.UserAvatarUrl, opt => opt.MapFrom(src => src.User != null ? src.User.AvatarUrl : null))
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

        //Species Mappings
        CreateMap<Species, SpeciesDto>();
        CreateMap<CreateSpeciesDto, Species>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        // Location Mappings
        CreateMap<FishingLocation, LocationDto>();
        CreateMap<CreateLocationDto, FishingLocation>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.Catches, opt => opt.Ignore());

        //Profile Mappings
        CreateMap<User, ProfileDto>();
        CreateMap<User, PublicProfileDto>()
            .ForMember(dest => dest.MemberSince, opt => opt.MapFrom(src => src.CreatedAt));

        // Notification Mappings
        CreateMap<Notification, NotificationDto>();


        // Add more mappings when creating more DTOs laterrr
    }
}