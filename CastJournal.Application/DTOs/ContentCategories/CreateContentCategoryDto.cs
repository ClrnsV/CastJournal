using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CastJournal.Domain.Enums;

namespace CastJournal.Application.DTOs.ContentCategories;

public class CreateContentCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public CategoryType Type { get; set; }
}
