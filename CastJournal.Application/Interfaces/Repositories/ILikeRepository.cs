using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CastJournal.Application.Interfaces.Repositories;
public interface ILikeRepository
{
    Task<bool> ExistsAsync(Guid catchId, string userId);
    Task LikeAsync(Guid catchId, string userId);
    Task UnlikeAsync(Guid catchId, string userId);
    Task<int> GetLikeCountAsync(Guid catchId);
    Task<Dictionary<Guid, int>> GetLikeCountsAsync(IEnumerable<Guid> catchIds); // for feed batch mapping
}