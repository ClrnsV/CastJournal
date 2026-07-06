using AutoMapper;
using CastJournal.Application.DTOs.Analytics;
using CastJournal.Application.DTOs.Catches;
using CastJournal.Application.DTOs.Export;
using CastJournal.Application.Interfaces.Repositories;
using CastJournal.Application.Interfaces.Services;
using CastJournal.Domain.Enums;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace CastJournal.Application.Services;

public class ExportService : IExportService
{
    private readonly ICatchRepository _catchRepository;
    private readonly IAnalyticsService _analyticsService;
    private readonly IMapper _mapper;

    public ExportService(ICatchRepository catchRepository, IAnalyticsService analyticsService, IMapper mapper)
    {
        _catchRepository = catchRepository;
        _analyticsService = analyticsService;
        _mapper = mapper;
    }

    public async Task<ExportResultDto> ExportCatchesAsync(string userId, ExportRequestDto request)
    {
        // Reuse UC-14's filtering —  turned paging off so to get everything that matches.
        var filter = new CatchFilterDto
        {
            SpeciesId = request.SpeciesId,
            LocationId = request.LocationId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            MinWeight = request.MinWeight,
            MaxWeight = request.MaxWeight,
            GearUsed = request.GearUsed,
            BaitUsed = request.BaitUsed,
            FishingMethod = request.FishingMethod,
            SearchTerm = request.SearchTerm,
            Page = 1,
            PageSize = int.MaxValue
        };

        var (items, _) = await _catchRepository.GetFilteredByUserIdAsync(userId, filter);
        var catches = _mapper.Map<List<CatchDto>>(items);

        AnalyticsDto? analytics = null;
        if (request.IncludeAnalytics)
        {
            analytics = await _analyticsService.GetAnalyticsAsync(userId, new AnalyticsFilterDto
            {
                StartDate = request.StartDate,
                EndDate = request.EndDate
            });
        }

        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");

        return request.Format switch
        {
            ExportFormat.Csv => new ExportResultDto
            {
                FileContent = BuildCsv(catches),
                FileName = $"CastJournal_Catches_{timestamp}.csv",
                ContentType = "text/csv"
            },
            ExportFormat.Json => new ExportResultDto
            {
                FileContent = BuildJson(catches, analytics),
                FileName = $"CastJournal_Catches_{timestamp}.json",
                ContentType = "application/json"
            },
            ExportFormat.Pdf => new ExportResultDto
            {
                FileContent = BuildPdf(catches, analytics),
                FileName = $"CastJournal_Catches_{timestamp}.pdf",
                ContentType = "application/pdf"
            },
            _ => throw new NotSupportedException($"Export format {request.Format} is not supported.")
        };
    }

    private static byte[] BuildCsv(List<CatchDto> catches)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Date,Species,Location,Weight (kg),Length (cm),Gear,Bait,Method,Weather,Notes");

        foreach (var c in catches)
        {
            sb.AppendLine(string.Join(",",
                Escape(c.CatchDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
                Escape(c.SpeciesName),
                Escape(c.LocationName),
                Escape(c.Weight?.ToString(CultureInfo.InvariantCulture)),
                Escape(c.Length?.ToString(CultureInfo.InvariantCulture)),
                Escape(c.GearUsed),
                Escape(c.BaitUsed),
                Escape(c.FishingMethod),
                Escape(c.WeatherConditions),
                Escape(c.Notes)));
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    // Wraps a field in quotes and escapes embedded quotes, per RFC 4180.
    private static string Escape(string? value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        var needsQuoting = value.Contains(',') || value.Contains('"') || value.Contains('\n');
        var escaped = value.Replace("\"", "\"\"");
        return needsQuoting ? $"\"{escaped}\"" : escaped;
    }

    private static byte[] BuildJson(List<CatchDto> catches, AnalyticsDto? analytics)
    {
        var payload = new
        {
            ExportedAt = DateTime.UtcNow,
            TotalRecords = catches.Count,
            Catches = catches,
            Analytics = analytics
        };

        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        return Encoding.UTF8.GetBytes(json);
    }

    private static byte[] BuildPdf(List<CatchDto> catches, AnalyticsDto? analytics)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Text("CastJournal - Catch Export").FontSize(18).Bold();

                page.Content().PaddingTop(15).Column(column =>
                {
                    column.Item().Text($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC").FontSize(8);
                    column.Item().Text($"Total records: {catches.Count}").FontSize(8);

                    if (analytics != null)
                    {
                        column.Item().PaddingTop(10).Text("Summary").FontSize(12).Bold();
                        column.Item().Text(
                            $"Total weight: {analytics.TotalWeight:0.##} kg   |   " +
                            $"Avg weight: {analytics.AverageWeight:0.##} kg   |   " +
                            $"Avg length: {analytics.AverageLength:0.##} cm");

                        if (analytics.TopSpecies.Any())
                        {
                            var top = string.Join(", ", analytics.TopSpecies.Take(3)
                                .Select(s => $"{s.SpeciesName} ({s.Count})"));
                            column.Item().Text($"Top species: {top}");
                        }
                    }

                    column.Item().PaddingTop(15).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1.2f);  // Date
                            columns.RelativeColumn(1.5f);  // Species
                            columns.RelativeColumn(1.5f);  // Location
                            columns.RelativeColumn(1f);    // Weight
                            columns.RelativeColumn(1f);    // Length
                            columns.RelativeColumn(1.3f);  // Method
                            columns.RelativeColumn(2f);    // Notes
                        });

                        table.Header(header =>
                        {
                            foreach (var title in new[] { "Date", "Species", "Location", "Weight (kg)", "Length (cm)", "Method", "Notes" })
                            {
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text(title).Bold();
                            }
                        });

                        foreach (var c in catches)
                        {
                            table.Cell().Padding(4).Text(c.CatchDate.ToString("yyyy-MM-dd"));
                            table.Cell().Padding(4).Text(c.SpeciesName ?? "-");
                            table.Cell().Padding(4).Text(c.LocationName ?? "-");
                            table.Cell().Padding(4).Text(c.Weight?.ToString("0.##") ?? "-");
                            table.Cell().Padding(4).Text(c.Length?.ToString("0.##") ?? "-");
                            table.Cell().Padding(4).Text(c.FishingMethod ?? "-");
                            table.Cell().Padding(4).Text(c.Notes ?? "-");
                        }
                    });
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.CurrentPageNumber();
                    x.Span(" / ");
                    x.TotalPages();
                });
            });
        }).GeneratePdf();
    }
}