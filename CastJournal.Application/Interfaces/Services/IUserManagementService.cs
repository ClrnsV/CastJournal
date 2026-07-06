using CastJournal.Application.DTOs;
using CastJournal.Application.DTOs.Users;

namespace CastJournal.Application.Interfaces.Services;

public interface IUserManagementService
{
    Task<PagedResult<UserSummaryDto>> GetUsersAsync(UserFilterDto filter);
    Task<UserSummaryDto?> GetUserByIdAsync(string id);
    Task<(ServiceResult Result, UserSummaryDto? User)> CreateUserAsync(CreateUserDto dto);
    Task<ServiceResult> UpdateUserAsync(string id, UpdateUserDto dto);
    Task<ServiceResult> SetActiveStatusAsync(string id, bool isActive);
}