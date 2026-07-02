using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CastJournal.Application.DTOs;

public class CatchMediaDto
{
    public Guid Id { get; set; }
    public string MediaUrl { get; set; } = string.Empty;
    public string? FileName { get; set; }
    public string MediaType { get; set; } = "Image";
    public int OrderIndex { get; set; }
}