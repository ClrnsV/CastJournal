using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CastJournal.Application.DTOs.ContentCategories;
using CastJournal.Domain.Enums;

namespace CastJournal.Application.Interfaces.Services;

public interface IContentCategoryService
{
    Task<IEnumerable<ContentCategoryDto>> GetAllAsync(CategoryType? type, bool includeInactive);
    Task<ContentCategoryDto?> GetByIdAsync(Guid id);
    Task<ContentCategoryDto> CreateAsync(CreateContentCategoryDto dto);
    Task<ContentCategoryDto?> UpdateAsync(Guid id, UpdateContentCategoryDto dto);
    Task<bool> DeleteAsync(Guid id);
}