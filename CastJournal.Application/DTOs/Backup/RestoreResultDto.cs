using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CastJournal.Application.DTOs.Backup;

public class RestoreResultDto
{
    public int SpeciesRestored { get; set; }
    public int LocationsRestored { get; set; }
    public int CategoriesRestored { get; set; }
    public int CatchesRestored { get; set; }
    public int UsersUpdated { get; set; }
    public int UsersSkipped { get; set; }   // referenced in backup but no longer exist — not recreated
    public List<string> Warnings { get; set; } = new();
}