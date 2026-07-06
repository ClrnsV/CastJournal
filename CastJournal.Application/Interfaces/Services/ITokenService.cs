using CastJournal.Application.DTOs.Auth;
using CastJournal.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CastJournal.Application.Interfaces.Services;

public interface ITokenService
{
    Task<AuthResponseDto> GenerateTokenAsync(User user, IList<string> roles);
    Task RevokeTokenAsync(string jti, string userId, DateTime expiresAt);
}