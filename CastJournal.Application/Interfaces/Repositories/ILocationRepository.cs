using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CastJournal.Domain.Entities;

namespace CastJournal.Application.Interfaces.Repositories;

public interface ILocationRepository
{
    Task<FishingLocation?> GetByIdAsync(Guid id);
    Task<IEnumerable<FishingLocation>> GetAllByUserIdAsync(string userId);
    Task AddAsync(FishingLocation location);
    void Update(FishingLocation location);
    void Delete(FishingLocation location, string deletedBy);
    Task SaveChangesAsync();
}