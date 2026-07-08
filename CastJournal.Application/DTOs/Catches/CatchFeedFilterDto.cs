using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CastJournal.Application.DTOs.Catches;

public class CatchFeedFilterDto
{
    public string? UserId { get; set; }
    public bool FollowingOnly { get; set; } = false;
    public int? Page { get; set; } = 1;
    public int? PageSize { get; set; } = 20;
}