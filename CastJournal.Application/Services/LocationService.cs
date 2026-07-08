using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CastJournal.Application.DTOs.Locations;
using CastJournal.Application.Interfaces.Repositories;
using CastJournal.Application.Interfaces.Services;
using CastJournal.Domain.Entities;

namespace CastJournal.Application.Services;

public class LocationService : ILocationService
{
    private readonly ILocationRepository _locationRepository;
    private readonly IMapper _mapper;

    public LocationService(ILocationRepository locationRepository, IMapper mapper)
    {
        _locationRepository = locationRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<LocationDto>> GetUserLocationsAsync(string userId)
    {
        var locations = await _locationRepository.GetAllByUserIdAsync(userId);
        return _mapper.Map<IEnumerable<LocationDto>>(locations);
    }
    public async Task<IEnumerable<LocationDto>> GetPublicLocationsAsync()
    {
        var locations = await _locationRepository.GetPublicLocationsAsync();
        return _mapper.Map<IEnumerable<LocationDto>>(locations);
    }

    public async Task<LocationDto?> GetLocationByIdAsync(Guid id, string userId)
    {
        var location = await _locationRepository.GetByIdAsync(id);
        if (location == null || location.UserId != userId)
            return null;

        return _mapper.Map<LocationDto>(location);
    }

    public async Task<LocationDto> CreateLocationAsync(CreateLocationDto dto, string userId)
    {
        var location = _mapper.Map<FishingLocation>(dto);
        location.UserId = userId;

        await _locationRepository.AddAsync(location);
        await _locationRepository.SaveChangesAsync();

        return _mapper.Map<LocationDto>(location);
    }
    public async Task<LocationDto> UpdateLocationAsync(Guid id, CreateLocationDto dto, string userId)
    {
        var location = await _locationRepository.GetByIdAsync(id);
        if (location == null || location.UserId != userId)
            throw new UnauthorizedAccessException("Location not found or unauthorized");

        _mapper.Map(dto, location);
        _locationRepository.Update(location);
        await _locationRepository.SaveChangesAsync();

        return _mapper.Map<LocationDto>(location);
    }

    public async Task<bool> DeleteLocationAsync(Guid id, string userId)
    {
        var location = await _locationRepository.GetByIdAsync(id);
        if (location == null || location.UserId != userId)
            return false;

        _locationRepository.Delete(location, userId);
        await _locationRepository.SaveChangesAsync();
        return true;
    }
}