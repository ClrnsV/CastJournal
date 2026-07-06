using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CastJournal.Application.DTOs.Reports;

public class SpeciesOverviewReportDto
{
    public int TotalSpecies { get; set; }
    public int ApprovedSpecies { get; set; }
    public int PendingSpecies { get; set; }
    public List<SystemSpeciesBreakdownDto> MostCaughtSpecies { get; set; } = new();
    public List<string> SpeciesNeverCaught { get; set; } = new();
}