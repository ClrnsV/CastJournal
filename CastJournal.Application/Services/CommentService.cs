using AutoMapper;
using CastJournal.Application.DTOs;
using CastJournal.Application.DTOs.Comments;
using CastJournal.Application.Interfaces.Repositories;
using CastJournal.Application.Interfaces.Services;
using CastJournal.Domain.Entities;
using CastJournal.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace CastJournal.Application.Services;
public class CommentService : ICommentService
{
    private readonly ICommentRepository _commentRepository;
    private readonly ICatchRepository _catchRepository;
    private readonly INotificationService _notificationService;
    private readonly UserManager<User> _userManager;
    private readonly IMapper _mapper;

    public CommentService(ICommentRepository commentRepository, ICatchRepository catchRepository,
        INotificationService notificationService, UserManager<User> userManager, IMapper mapper)
    {
        _commentRepository = commentRepository;
        _catchRepository = catchRepository;
        _notificationService = notificationService;
        _userManager = userManager;
        _mapper = mapper;
    }

    public async Task<(CommentDto? Comment, string? Error)> AddCommentAsync(Guid catchId, string userId, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return (null, "Comment cannot be empty.");

        var catchEntity = await _catchRepository.GetByIdAsync(catchId);
        if (catchEntity == null)
            return (null, "Catch not found.");

        var comment = new Comment
        {
            CatchId = catchId,
            UserId = userId,
            Content = content.Trim()
        };

        await _commentRepository.AddAsync(comment);

        if (catchEntity.UserId != userId)
        {
            var commenter = await _userManager.FindByIdAsync(userId);
            var commenterName = commenter?.FullName ?? commenter?.UserName ?? "Someone";

            await _notificationService.CreateNotificationAsync(
                catchEntity.UserId,
                NotificationType.NewComment,
                "New Comment",
                $"{commenterName} commented on your catch!",
                relatedEntityType: "Catch",
                relatedEntityId: catchId.ToString());
        }

        var saved = await _commentRepository.GetByIdAsync(comment.Id);
        var dto = _mapper.Map<CommentDto>(saved);
        dto.IsOwnedByCurrentUser = true;
        return (dto, null);
    }

    public async Task<(bool Succeeded, string? Error)> UpdateCommentAsync(Guid commentId, string userId, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return (false, "Comment cannot be empty.");

        var comment = await _commentRepository.GetByIdAsync(commentId);
        if (comment == null)
            return (false, "Comment not found.");

        if (comment.UserId != userId)
            return (false, "You can only edit your own comments.");

        comment.Content = content.Trim();
        comment.EditedAt = DateTime.UtcNow;
        await _commentRepository.UpdateAsync(comment);

        return (true, null);
    }

    public async Task<(bool Succeeded, string? Error)> DeleteCommentAsync(Guid commentId, string userId)
    {
        var comment = await _commentRepository.GetByIdAsync(commentId);
        if (comment == null)
            return (true, null); // already gone — idempotent, matches UnfollowAsync

        // Either the comment's author or the catch owner can remove it —
        // lets someone moderate comments on their own post.
        if (comment.UserId != userId && comment.Catch?.UserId != userId)
            return (false, "You don't have permission to delete this comment.");

        await _commentRepository.DeleteAsync(comment);
        return (true, null);
    }

    public async Task<PagedResult<CommentDto>> GetCommentsAsync(Guid catchId, string? currentUserId, int page, int pageSize)
    {
        var (comments, totalCount) = await _commentRepository.GetByCatchIdAsync(catchId, page, pageSize);

        var dtos = comments.Select(c =>
        {
            var dto = _mapper.Map<CommentDto>(c);
            dto.IsOwnedByCurrentUser = currentUserId != null && c.UserId == currentUserId;
            return dto;
        }).ToList();

        return new PagedResult<CommentDto> { Items = dtos, TotalCount = totalCount, Page = page, PageSize = pageSize };
    }
}