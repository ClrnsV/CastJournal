using CastJournal.Application.DTOs.ContentCategories;
using CastJournal.Application.Interfaces.Services;
using CastJournal.Domain.Entities;
using CastJournal.Domain.Enums;
using CastJournal.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace CastJournal.Infrastructure.Services;

public class ContentCategoryService : IContentCategoryService
{
    private readonly ApplicationDbContext _context;

    public ContentCategoryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ContentCategoryDto>> GetAllAsync(CategoryType? type, bool includeInactive)
    {
        var query = _context.ContentCategories.AsQueryable();

        if (type.HasValue)
            query = query.Where(c => c.Type == type.Value);

        if (!includeInactive)
            query = query.Where(c => c.IsActive);

        return await query
            .OrderBy(c => c.Type).ThenBy(c => c.Name)
            .Select(c => MapToDto(c))
            .ToListAsync();
    }

    public async Task<ContentCategoryDto?> GetByIdAsync(Guid id)
    {
        var category = await _context.ContentCategories.FindAsync(id);
        return category == null ? null : MapToDto(category);
    }

    public async Task<ContentCategoryDto> CreateAsync(CreateContentCategoryDto dto)
    {
        var category = new ContentCategory
        {
            Name = dto.Name.Trim(),
            Description = dto.Description,
            Type = dto.Type,
            IsActive = true
        };

        _context.ContentCategories.Add(category);
        await _context.SaveChangesAsync();

        return MapToDto(category);
    }

    public async Task<ContentCategoryDto?> UpdateAsync(Guid id, UpdateContentCategoryDto dto)
    {
        var category = await _context.ContentCategories.FindAsync(id);
        if (category == null) return null;

        if (!string.IsNullOrWhiteSpace(dto.Name))
            category.Name = dto.Name.Trim();

        if (dto.Description != null)
            category.Description = dto.Description;

        if (dto.IsActive.HasValue)
            category.IsActive = dto.IsActive.Value;

        category.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return MapToDto(category);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var category = await _context.ContentCategories.FindAsync(id);
        if (category == null) return false;

        _context.ContentCategories.Remove(category);
        await _context.SaveChangesAsync();
        return true;
    }

    private static ContentCategoryDto MapToDto(ContentCategory c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        Description = c.Description,
        Type = c.Type,
        IsActive = c.IsActive
    };
}