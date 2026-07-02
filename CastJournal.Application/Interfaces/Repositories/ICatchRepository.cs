using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CastJournal.Domain.Entities;

namespace CastJournal.Application.Interfaces.Repositories;

public interface ICatchRepository
{
    Task<Catch?> GetByIdAsync(Guid id);
    Task<IEnumerable<Catch>> GetAllByUserIdAsync(string userId);
    Task AddAsync(Catch entity);
    void Update(Catch entity);
    void Delete(Catch entity);
    Task SaveChangesAsync();
}