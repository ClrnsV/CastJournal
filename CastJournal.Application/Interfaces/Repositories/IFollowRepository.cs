using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CastJournal.Application.Interfaces.Repositories;

public interface IFollowRepository
{
    Task<bool> ExistsAsync(string followerId, string followingId);
    Task FollowAsync(string followerId, string followingId);
    Task UnfollowAsync(string followerId, string followingId);
    Task<int> GetFollowerCountAsync(string userId);
    Task<int> GetFollowingCountAsync(string userId);
    Task<List<string>> GetFollowingIdsAsync(string followerId);
}
