using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CastJournal.Application.DTOs.Species;

namespace CastJournal.Application.Interfaces.Services;

public interface ISpeciesService
{
    Task<IEnumerable<SpeciesDto>> GetAllSpeciesAsync();
    Task<SpeciesDto?> GetSpeciesByIdAsync(Guid id);
    Task<SpeciesDto> CreateSpeciesAsync(CreateSpeciesDto dto);
    Task<SpeciesDto> UpdateSpeciesAsync(Guid id, CreateSpeciesDto dto);
    Task<bool> DeleteSpeciesAsync(Guid id);
}