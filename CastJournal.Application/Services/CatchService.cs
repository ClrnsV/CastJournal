using AutoMapper;
using CastJournal.Application.DTOs;
using CastJournal.Application.DTOs.Catches;
using CastJournal.Application.Interfaces.Repositories;
using CastJournal.Application.Interfaces.Services;
using CastJournal.Common.Exceptions;
using CastJournal.Domain.Entities;
using CastJournal.Domain.Enums;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CastJournal.Application.Services;

public class CatchService : ICatchService
{
    private readonly ICatchRepository _catchRepository;
    private readonly IMapper _mapper;
    private readonly IFileStorageService _fileStorageService;
    private readonly IFollowRepository _followRepository;

    public CatchService(ICatchRepository catchRepository, IMapper mapper, IFileStorageService fileStorageService, IFollowRepository followRepository)
    {
        _catchRepository = catchRepository;
        _mapper = mapper;
        _fileStorageService = fileStorageService;
        _followRepository = followRepository;
    }

    public async Task<CatchDto> CreateCatchAsync(CreateCatchDto dto, string userId)
    {
        var speciesExists = await _catchRepository.SpeciesExistsAsync(dto.SpeciesId);
        if (!speciesExists)
            throw new NotFoundException($"Species with ID '{dto.SpeciesId}' was not found.");

        if (dto.LocationId.HasValue)
        {
            var locationExists = await _catchRepository.LocationExistsAsync(dto.LocationId.Value);
            if (!locationExists)
                throw new NotFoundException($"Location with ID '{dto.LocationId}' was not found.");
        }

        var catchEntity = _mapper.Map<Catch>(dto);
        catchEntity.UserId = userId;
        catchEntity.CatchDate = dto.CatchDate == default ? DateTime.UtcNow : dto.CatchDate;

        await _catchRepository.AddAsync(catchEntity);
        await _catchRepository.SaveChangesAsync();

        // Reload with includes for proper DTO mapping
        var createdCatch = await _catchRepository.GetByIdAsync(catchEntity.Id);
        return _mapper.Map<CatchDto>(createdCatch!);
    }

    public async Task<CatchDto?> GetCatchByIdAsync(Guid id, string userId)
    {
        var catchEntity = await _catchRepository.GetByIdAsync(id);

        if (catchEntity == null || catchEntity.UserId != userId)
            return null;

        return _mapper.Map<CatchDto>(catchEntity);
    }

    public async Task<IEnumerable<CatchDto>> GetUserCatchesAsync(string userId)
    {
        var catches = await _catchRepository.GetAllByUserIdAsync(userId);
        return _mapper.Map<IEnumerable<CatchDto>>(catches);
    }

    public async Task<bool> UpdateCatchAsync(Guid id, CreateCatchDto dto, string userId)
    {
        var catchEntity = await _catchRepository.GetByIdAsync(id);
        if (catchEntity == null || catchEntity.UserId != userId)
            return false;

        var speciesExists = await _catchRepository.SpeciesExistsAsync(dto.SpeciesId);
        if (!speciesExists)
            throw new NotFoundException($"Species with ID '{dto.SpeciesId}' was not found.");

        if (dto.LocationId.HasValue)
        {
            var locationExists = await _catchRepository.LocationExistsAsync(dto.LocationId.Value);
            if (!locationExists)
                throw new NotFoundException($"Location with ID '{dto.LocationId}' was not found.");
        }

        // Update properties
        _mapper.Map(dto, catchEntity);
        catchEntity.UpdatedAt = DateTime.UtcNow;

        await _catchRepository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteCatchAsync(Guid id, string userId)
    {
        var catchEntity = await _catchRepository.GetByIdAsync(id);
        if (catchEntity == null || catchEntity.UserId != userId)
            return false;

        catchEntity.IsDeleted = true;
        catchEntity.DeletedAt = DateTime.UtcNow;
        catchEntity.DeletedBy = userId;

        await _catchRepository.SaveChangesAsync();
        return true;
    }
    public async Task<bool> UploadMediaAsync(Guid catchId, IFormFile file, string userId)
    {
        var catchEntity = await _catchRepository.GetByIdAsync(catchId);
        if (catchEntity == null || catchEntity.UserId != userId)
            return false;

        var mediaUrl = await _fileStorageService.SaveFileAsync(file, catchId);

        var media = new CatchMedia
        {
            CatchId = catchId,
            MediaUrl = mediaUrl,
            FileName = file.FileName,
            MediaType = file.ContentType.StartsWith("video") ? MediaType.Video : MediaType.Image,
            OrderIndex = catchEntity.Media.Count
        };

        await _catchRepository.AddMediaAsync(media);   // explicit Add — forces EF state = Added
        await _catchRepository.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<CatchMediaDto>> GetCatchMediaAsync(Guid catchId, string userId)
    {
        var catchEntity = await _catchRepository.GetByIdAsync(catchId);
        if (catchEntity == null || catchEntity.UserId != userId)
            return Enumerable.Empty<CatchMediaDto>();

        return _mapper.Map<IEnumerable<CatchMediaDto>>(catchEntity.Media);
    }

    public async Task<bool> DeleteMediaAsync(Guid mediaId, string userId)
    {
        var media = await _catchRepository.GetMediaByIdAsync(mediaId);
        if (media == null || media.Catch == null || media.Catch.UserId != userId)
            return false;

        await _fileStorageService.DeleteFileAsync(media.MediaUrl);

        _catchRepository.DeleteMedia(media);
        await _catchRepository.SaveChangesAsync();

        return true;
    }
    public async Task<PagedResult<CatchDto>> SearchCatchesAsync(string userId, CatchFilterDto filter)
    {
        var (items, totalCount) = await _catchRepository.GetFilteredByUserIdAsync(userId, filter);

        return new PagedResult<CatchDto>
        {
            Items = _mapper.Map<IEnumerable<CatchDto>>(items),
            TotalCount = totalCount,
            Page = filter.Page ?? 1,
            PageSize = filter.PageSize ?? 20
        };
    }
    public async Task<PagedResult<CatchDto>> GetPublicFeedAsync(CatchFeedFilterDto filter, string? currentUserId)
    {
        var page = filter.Page ?? 1;
        var pageSize = filter.PageSize ?? 20;

        List<string>? followingIds = null;
        if (filter.FollowingOnly && !string.IsNullOrEmpty(currentUserId))
            followingIds = await _followRepository.GetFollowingIdsAsync(currentUserId);

        var (items, totalCount) = await _catchRepository.GetPublicFeedAsync(filter.UserId, followingIds, page, pageSize);

        return new PagedResult<CatchDto>
        {
            Items = _mapper.Map<IEnumerable<CatchDto>>(items),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}