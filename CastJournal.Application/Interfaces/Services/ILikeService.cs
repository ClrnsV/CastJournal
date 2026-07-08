using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CastJournal.Application.Interfaces.Services;
public interface ILikeService
{
    Task<(bool Succeeded, string? Error)> LikeAsync(Guid catchId, string userId);
    Task<(bool Succeeded, string? Error)> UnlikeAsync(Guid catchId, string userId);
}