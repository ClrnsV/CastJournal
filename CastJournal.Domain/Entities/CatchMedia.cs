using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CastJournal.Domain.Entities.Base;
using CastJournal.Domain.Enums;

namespace CastJournal.Domain.Entities;

public class CatchMedia : BaseEntity
{
    public string MediaUrl { get; set; } = string.Empty;
    public string? FileName { get; set; }
    public MediaType MediaType { get; set; }
    public int OrderIndex { get; set; } = 0;

    public Guid CatchId { get; set; }
    public Catch? Catch { get; set; }
}