using CastJournal.Application.DTOs.Auth;
using CastJournal.Application.Interfaces.Repositories;
using CastJournal.Application.Interfaces.Services;
using CastJournal.Application.Services;
using CastJournal.Domain.Entities;
using CastJournal.Infrastructure.Persistence.Context;
using CastJournal.Infrastructure.Persistence.Repositories;
using CastJournal.Infrastructure.Services;
using CastJournal.Infrastructure.Storage;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CastJournal.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Identity
        services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

        // JWT Settings
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

        // Services
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<ICatchService, CatchService>();
        services.AddScoped<ISpeciesService, SpeciesService>();
        services.AddScoped<ILocationService, LocationService>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<IAnalyticsService, AnalyticsService>();
        services.AddScoped<IExportService, ExportService>();
        services.AddScoped<IUserManagementService, UserManagementService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IContentCategoryService, ContentCategoryService>();
        services.AddScoped<IBackupService, BackupService>();

        // Repositories
        services.AddScoped<ICatchRepository, CatchRepository>();
        services.AddScoped<ISpeciesRepository, SpeciesRepository>();
        services.AddScoped<ILocationRepository, LocationRepository>();
        services.AddScoped<IRevokedTokenRepository, RevokedTokenRepository>();

        return services;
    }
}