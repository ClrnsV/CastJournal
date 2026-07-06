using CastJournal.Domain.Entities.Base;
using CastJournal.Domain.Enums;

namespace CastJournal.Domain.Entities;

public class ContentCategory : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public CategoryType Type { get; set; }
    public bool IsActive { get; set; } = true;
}