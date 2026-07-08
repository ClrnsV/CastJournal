using CastJournal.Application.DTOs;
using CastJournal.Application.DTOs.Users;
using CastJournal.Application.Interfaces.Services;
using CastJournal.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CastJournal.Domain.Enums;


namespace CastJournal.Infrastructure.Services;

public class UserManagementService : IUserManagementService
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly INotificationService _notificationService;

    public UserManagementService(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, INotificationService notificationService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _notificationService = notificationService;
    }

    public async Task<PagedResult<UserSummaryDto>> GetUsersAsync(UserFilterDto filter)
    {
        var query = _userManager.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim().ToLower();
            query = query.Where(u =>
                (u.UserName != null && u.UserName.ToLower().Contains(term)) ||
                (u.Email != null && u.Email.ToLower().Contains(term)) ||
                (u.FullName != null && u.FullName.ToLower().Contains(term)));
        }

        if (filter.IsActive.HasValue)
            query = query.Where(u => u.IsActive == filter.IsActive.Value);

        var totalCount = await query.CountAsync();

        var page = filter.Page ?? 1;
        var pageSize = filter.PageSize ?? 20;

        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var summaries = new List<UserSummaryDto>();
        foreach (var user in users)
        {
            var dto = await MapToSummaryAsync(user);

            // Role filter applied post-fetch since roles live in separate Identity tables.
            if (!string.IsNullOrWhiteSpace(filter.Role) && !string.Equals(dto.Role, filter.Role, StringComparison.OrdinalIgnoreCase))
                continue;

            summaries.Add(dto);
        }

        return new PagedResult<UserSummaryDto>
        {
            Items = summaries,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<UserSummaryDto?> GetUserByIdAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        return user == null ? null : await MapToSummaryAsync(user);
    }

    public async Task<(ServiceResult Result, UserSummaryDto? User)> CreateUserAsync(CreateUserDto dto)
    {
        if (!await _roleManager.RoleExistsAsync(dto.Role))
            return (ServiceResult.Failure(new[] { $"Role '{dto.Role}' does not exist." }), null);

        var user = new User
        {
            UserName = dto.UserName,
            Email = dto.Email,
            FullName = dto.FullName,
            IsActive = true,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return (ServiceResult.Failure(result.Errors.Select(e => e.Description)), null);

        await _userManager.AddToRoleAsync(user, dto.Role);

        return (ServiceResult.Success(), await MapToSummaryAsync(user));
    }

    public async Task<ServiceResult> UpdateUserAsync(string id, UpdateUserDto dto)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return ServiceResult.Failure(new[] { "User not found." });

        if (!string.IsNullOrWhiteSpace(dto.FullName))
            user.FullName = dto.FullName;

        if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != user.Email)
        {
            var setEmailResult = await _userManager.SetEmailAsync(user, dto.Email);
            if (!setEmailResult.Succeeded)
                return ServiceResult.Failure(setEmailResult.Errors.Select(e => e.Description));

            // Keep username in sync since login currently looks users up by email.
            await _userManager.SetUserNameAsync(user, dto.Email);
        }

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
            return ServiceResult.Failure(updateResult.Errors.Select(e => e.Description));

        if (!string.IsNullOrWhiteSpace(dto.Role))
        {
            if (!await _roleManager.RoleExistsAsync(dto.Role))
                return ServiceResult.Failure(new[] { $"Role '{dto.Role}' does not exist." });

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (!currentRoles.Contains(dto.Role))
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRoleAsync(user, dto.Role);
            }
        }

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> SetActiveStatusAsync(string id, bool isActive, string currentAdminId)
    {
        if (!isActive && id == currentAdminId)
            return ServiceResult.Failure(new[] { "You cannot deactivate your own account." });

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return ServiceResult.Failure(new[] { "User not found." });

        if (!isActive && await IsLastActiveAdminAsync(id))
            return ServiceResult.Failure(new[] { "Cannot deactivate the last remaining active Admin." });

        user.IsActive = isActive;
        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
            return ServiceResult.Failure(result.Errors.Select(e => e.Description));

        await _notificationService.CreateNotificationAsync(
            user.Id,
            isActive ? NotificationType.AccountReactivated : NotificationType.AccountDeactivated,
            isActive ? "Account Reactivated" : "Account Deactivated",
            isActive
                ? "Your account has been reactivated. You can now log in normally."
                : "Your account has been deactivated by an administrator. Contact support if you believe this is a mistake.");

        return ServiceResult.Success();
    }

    private async Task<bool> IsLastActiveAdminAsync(string excludingUserId)
    {
        var admins = await _userManager.GetUsersInRoleAsync("Admin");
        return admins.Count(a => a.IsActive && a.Id != excludingUserId) == 0;
    }

    private async Task<UserSummaryDto> MapToSummaryAsync(User user)
    {
        var roles = await _userManager.GetRolesAsync(user);

        return new UserSummaryDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            FullName = user.FullName,
            Role = roles.FirstOrDefault() ?? "User",
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        };
    }
}