using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CastJournal.Application.DTOs.Locations;

namespace CastJournal.Application.Interfaces.Services;

public interface ILocationService
{
    Task<IEnumerable<LocationDto>> GetUserLocationsAsync(string userId);
    Task<LocationDto?> GetLocationByIdAsync(Guid id, string userId);
    Task<LocationDto> CreateLocationAsync(CreateLocationDto dto, string userId);
    Task<LocationDto> UpdateLocationAsync(Guid id, CreateLocationDto dto, string userId);
    Task<bool> DeleteLocationAsync(Guid id, string userId);
}