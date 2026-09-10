using CastJournal.Domain.Entities;

namespace CastJournal.Application.Interfaces.Repositories;
public interface ICommentRepository
{
    Task<Comment?> GetByIdAsync(Guid id);
    Task AddAsync(Comment comment);
    Task UpdateAsync(Comment comment);
    Task DeleteAsync(Comment comment);
    Task<(List<Comment> Comments, int TotalCount)> GetByCatchIdAsync(Guid catchId, int page, int pageSize);
    Task<Dictionary<Guid, int>> GetCommentCountsAsync(IEnumerable<Guid> catchIds); // for feed batch mapping, mirrors ILikeRepository
}