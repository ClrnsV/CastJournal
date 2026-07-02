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
    Task<IEnumerable<Species>> GetAllAsync();
    Task AddAsync(Species entity);
    void Update(Species entity);
    void Delete(Species entity);
}