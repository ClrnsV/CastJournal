using CastJournal.Application.DTOs.Catches;
using CastJournal.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CastJournal.Application.Interfaces.Repositories;

public interface ICatchRepository
{
    Task<Catch?> GetByIdAsync(Guid id);
    Task<IEnumerable<Catch>> GetAllByUserIdAsync(string userId);
    Task AddAsync(Catch entity);
    void Update(Catch entity);
    void Delete(Catch entity);
    Task SaveChangesAsync();
    Task AddMediaAsync(CatchMedia media);
    Task<CatchMedia?> GetMediaByIdAsync(Guid mediaId);
    void DeleteMedia(CatchMedia media);
    Task<(IEnumerable<Catch> Items, int TotalCount)> GetFilteredByUserIdAsync(string userId, CatchFilterDto filter);
    Task<List<Catch>> GetForAnalyticsAsync(string userId, DateTime? startDate, DateTime? endDate);
}