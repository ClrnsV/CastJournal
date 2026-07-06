using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CastJournal.Domain.Entities;

namespace CastJournal.Application.Interfaces.Repositories;

public interface IRevokedTokenRepository
{
    Task AddAsync(RevokedToken token);
    Task<bool> IsRevokedAsync(string jti);
}