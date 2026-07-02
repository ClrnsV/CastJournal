using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CastJournal.Application.DTOs.Catches;

namespace CastJournal.Application.Interfaces.Services;

public interface ICatchService
{
    Task<CatchDto> CreateCatchAsync(CreateCatchDto dto, string userId);
    Task<CatchDto?> GetCatchByIdAsync(Guid id, string userId);
    Task<IEnumerable<CatchDto>> GetUserCatchesAsync(string userId);
    Task UpdateCatchAsync(Guid id, CreateCatchDto dto, string userId);
    Task DeleteCatchAsync(Guid id, string userId);
}