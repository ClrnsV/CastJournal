using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CastJournal.Domain.Entities;

namespace CastJournal.Application.Interfaces.Repositories;

public interface ISpeciesRepository
{
    Task<Species?> GetByIdAsync(Guid id);
    Task<IEnumerable<Species>> GetAllAsync(bool onlyApproved = true);
    Task AddAsync(Species species);
    void Update(Species species);
    void Delete(Species species);
    Task SaveChangesAsync();
}