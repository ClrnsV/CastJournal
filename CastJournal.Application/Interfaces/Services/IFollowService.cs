using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CastJournal.Application.Interfaces.Services;
public interface IFollowService
{
    Task<(bool Succeeded, string? Error)> FollowAsync(string followerId, string followingId);
    Task<(bool Succeeded, string? Error)> UnfollowAsync(string followerId, string followingId);
}