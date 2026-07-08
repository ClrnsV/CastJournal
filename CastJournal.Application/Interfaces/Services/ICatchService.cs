using CastJournal.Application.DTOs;
using CastJournal.Application.DTOs.Catches;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CastJournal.Application.Interfaces.Services;

public interface ICatchService
{
    Task<CatchDto> CreateCatchAsync(CreateCatchDto dto, string userId);
    Task<CatchDto?> GetCatchByIdAsync(Guid id, string userId);
    Task<IEnumerable<CatchDto>> GetUserCatchesAsync(string userId);
    Task<bool> UpdateCatchAsync(Guid id, CreateCatchDto dto, string userId);
    Task<bool> DeleteCatchAsync(Guid id, string userId);
    Task<bool> UploadMediaAsync(Guid catchId, IFormFile file, string userId);
    Task<IEnumerable<CatchMediaDto>> GetCatchMediaAsync(Guid catchId, string userId);
    Task<bool> DeleteMediaAsync(Guid mediaId, string userId);
    Task<PagedResult<CatchDto>> SearchCatchesAsync(string userId, CatchFilterDto filter);
    Task<PagedResult<CatchDto>> GetPublicFeedAsync(CatchFeedFilterDto filter, string? currentUserId);
}