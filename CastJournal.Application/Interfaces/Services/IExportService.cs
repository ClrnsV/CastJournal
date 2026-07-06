using CastJournal.Application.DTOs.Export;


namespace CastJournal.Application.Interfaces.Services;

public interface IExportService
{
    Task<ExportResultDto> ExportCatchesAsync(string userId, ExportRequestDto request);
}