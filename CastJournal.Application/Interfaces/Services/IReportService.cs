using CastJournal.Application.DTOs.Reports;

namespace CastJournal.Application.Interfaces.Services;

public interface IReportService
{
    Task<SystemReportDto> GenerateReportAsync(SystemReportFilterDto filter);
}