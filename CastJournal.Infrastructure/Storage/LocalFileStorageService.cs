using CastJournal.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
namespace CastJournal.Infrastructure.Storage;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _uploadPath;
    public LocalFileStorageService()
    {
        _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        if (!Directory.Exists(_uploadPath))
            Directory.CreateDirectory(_uploadPath);
    }

    public async Task<string> SaveFileAsync(IFormFile file, Guid catchId)
    {
        var catchFolder = Path.Combine(_uploadPath, catchId.ToString());
        if (!Directory.Exists(catchFolder))
            Directory.CreateDirectory(catchFolder);

        var fileName = $"{Guid.NewGuid()}_{file.FileName}";
        var fullPath = Path.Combine(catchFolder, fileName);

        await using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/uploads/{catchId}/{fileName}";
    }

    public Task<bool> DeleteFileAsync(string fileUrl)
    {
        try
        {
            // fileUrl looks like "/uploads/{catchId}/{fileName}"
            var relativePath = fileUrl.TrimStart('/');
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath);

            if (File.Exists(fullPath))
                File.Delete(fullPath);

            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public async Task<string> SaveAvatarAsync(IFormFile file, string userId)
    {
        var avatarFolder = Path.Combine(_uploadPath, "avatars", userId);
        if (!Directory.Exists(avatarFolder))
            Directory.CreateDirectory(avatarFolder);

        var fileName = $"{Guid.NewGuid()}_{file.FileName}";
        var fullPath = Path.Combine(avatarFolder, fileName);

        await using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/uploads/avatars/{userId}/{fileName}";
    }
}