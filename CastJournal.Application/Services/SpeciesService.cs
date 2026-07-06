using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CastJournal.Application.DTOs.Species;
using CastJournal.Application.Interfaces.Repositories;
using CastJournal.Application.Interfaces.Services;
using CastJournal.Domain.Entities;

namespace CastJournal.Application.Services;

public class SpeciesService : ISpeciesService
{
    private readonly ISpeciesRepository _speciesRepository;
    private readonly IMapper _mapper;

    public SpeciesService(ISpeciesRepository speciesRepository, IMapper mapper)
    {
        _speciesRepository = speciesRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<SpeciesDto>> GetAllSpeciesAsync()
    {
        var species = await _speciesRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<SpeciesDto>>(species);
    }

    public async Task<SpeciesDto?> GetSpeciesByIdAsync(Guid id)
    {
        var species = await _speciesRepository.GetByIdAsync(id);
        return _mapper.Map<SpeciesDto>(species);
    }

    public async Task<SpeciesDto> CreateSpeciesAsync(CreateSpeciesDto dto)
    {
        var species = _mapper.Map<Species>(dto);
        species.IsApproved = true;

        await _speciesRepository.AddAsync(species);
        await _speciesRepository.SaveChangesAsync();

        return _mapper.Map<SpeciesDto>(species);
    }
    public async Task<SpeciesDto> UpdateSpeciesAsync(Guid id, CreateSpeciesDto dto)
    {
        var species = await _speciesRepository.GetByIdAsync(id);
        if (species == null)
            throw new KeyNotFoundException("Species not found");

        _mapper.Map(dto, species);
        _speciesRepository.Update(species);
        await _speciesRepository.SaveChangesAsync();

        return _mapper.Map<SpeciesDto>(species);
    }

    public async Task<bool> DeleteSpeciesAsync(Guid id)
    {
        var species = await _speciesRepository.GetByIdAsync(id);
        if (species == null)
            return false;

        _speciesRepository.Delete(species);
        await _speciesRepository.SaveChangesAsync();
        return true;
    }
}