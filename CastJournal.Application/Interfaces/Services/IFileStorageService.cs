using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CastJournal.Application.Interfaces.Services;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(IFormFile file, Guid catchId);
    Task<string> SaveAvatarAsync(IFormFile file, string userId);
    Task<bool> DeleteFileAsync(string fileUrl);
}