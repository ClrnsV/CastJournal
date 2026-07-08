using CastJournal.Domain.Entities;
using CastJournal.Domain.Enums;
using CastJournal.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CastJournal.Infrastructure.Persistence.Seed;

public static class ApplicationDbSeeder
{
    // Runs in every environment, including production.Roles, the admin account,
    // species, and content categories are all things a live app needs to function at all.
    public static async Task SeedEssentialDataAsync(ApplicationDbContext context, RoleManager<IdentityRole> roleManager,
          UserManager<User> userManager, IConfiguration configuration)
    {
        await SeedRolesAsync(roleManager);
        await SeedSpeciesAsync(context);
        await SeedAdminUserAsync(userManager, configuration);
        await SeedContentCategoriesAsync(context);
        await SeedLocationsAsync(context, userManager, configuration);
        await context.SaveChangesAsync();
    }
    // Dev-only convenience data — a fake test user with sample catches for local testing.
    // Never call this in Production; it plants a throwaway account with no real password checks intended for prod use.
    public static async Task SeedDevelopmentDataAsync(ApplicationDbContext context, UserManager<User> userManager)
    {
        await SeedTestUserAndCatchesAsync(context, userManager);
        await context.SaveChangesAsync();
    }
    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        foreach (var role in new[] { "Admin", "User" })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    private static async Task SeedAdminUserAsync(UserManager<User> userManager, IConfiguration configuration)
    {
        var adminEmail = configuration["AdminSeed:Email"] ?? "admin@castjournal.com";
        var adminPassword = configuration["AdminSeed:Password"]
            ?? throw new InvalidOperationException("AdminSeed:Password is not configured.");

        // Check both email AND username — Identity enforces uniqueness on both
        if (await userManager.FindByEmailAsync(adminEmail) != null) return;
        if (await userManager.FindByNameAsync("admin") != null) return;

        var admin = new User
        {
            UserName = "admin",
            Email = adminEmail,
            FullName = "CastJournal Admin",
            IsActive = true,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(admin, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, "Admin");
        }
        else
        {
            var errors = string.Join(" | ", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
            throw new InvalidOperationException($"Failed to seed admin user: {errors}");
        }
    }

    private static async Task SeedSpeciesAsync(ApplicationDbContext context)
    {
        if (await context.Species.AnyAsync()) return;

        var species = new List<Species>
        {
            new Species { Id = Guid.NewGuid(), CommonName = "Largemouth Bass", ScientificName = "Micropterus salmoides", Category = "Freshwater", IsApproved = true },
            new Species { Id = Guid.NewGuid(), CommonName = "Rainbow Trout", ScientificName = "Oncorhynchus mykiss", Category = "Freshwater", IsApproved = true },
            new Species { Id = Guid.NewGuid(), CommonName = "Northern Pike", ScientificName = "Esox lucius", Category = "Freshwater", IsApproved = true }
        };

        await context.Species.AddRangeAsync(species);
    }
    private static async Task SeedLocationsAsync(ApplicationDbContext context, UserManager<User> userManager, IConfiguration configuration)
    {
        if (await context.FishingLocations.AnyAsync()) return;

        var adminEmail = configuration["AdminSeed:Email"] ?? "admin@castjournal.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null) return; // admin not created yet; skip for now

        var location = new FishingLocation
        {
            Id = Guid.NewGuid(),
            Name = "Default Lake",
            Description = "Seeded default fishing location.",
            Latitude = 8.4542,
            Longitude = 124.6319,
            WaterType = WaterType.Freshwater,
            IsPublic = true,
            UserId = adminUser.Id
        };

        await context.FishingLocations.AddAsync(location);
    }
    private static async Task SeedTestUserAndCatchesAsync(ApplicationDbContext context, UserManager<User> userManager)
    {
        const string testUserId = "test-user-123";

        if (!await context.Users.AnyAsync(u => u.Id == testUserId))
        {
            var testUser = new User
            {
                Id = testUserId,
                UserName = "testangler",
                Email = "test@castjournal.com",
                FullName = "Test Angler",
                IsActive = true,
                EmailConfirmed = true
            };
            await context.Users.AddAsync(testUser);
            await context.SaveChangesAsync();
            await userManager.AddToRoleAsync(testUser, "User");
        }

        if (!await context.Species.AnyAsync())
        {
            var speciesList = new List<Species>
            {
                new Species { Id = Guid.NewGuid(), CommonName = "Largemouth Bass", ScientificName = "Micropterus salmoides", Category = "Freshwater", IsApproved = true },
                new Species { Id = Guid.NewGuid(), CommonName = "Rainbow Trout", ScientificName = "Oncorhynchus mykiss", Category = "Freshwater", IsApproved = true },
                new Species { Id = Guid.NewGuid(), CommonName = "Northern Pike", ScientificName = "Esox lucius", Category = "Freshwater", IsApproved = true }
            };
            await context.Species.AddRangeAsync(speciesList);
            await context.SaveChangesAsync();
        }

        if (!await context.Catches.AnyAsync(c => c.UserId == testUserId))
        {
            var firstSpecies = await context.Species.FirstAsync();

            var catches = new List<Catch>
            {
                new Catch
                {
                    UserId = testUserId, SpeciesId = firstSpecies.Id, CatchDate = DateTime.UtcNow.AddDays(-2),
                    Weight = 3.25m, Length = 48.5m, GearUsed = "Spinning Rod", BaitUsed = "Spinnerbait",
                    FishingMethod = "Casting", Notes = "Beautiful fight near the weeds!", WeatherConditions = "Sunny"
                },
                new Catch
                {
                    UserId = testUserId, SpeciesId = firstSpecies.Id, CatchDate = DateTime.UtcNow.AddDays(-1),
                    Weight = 1.85m, Length = 35.0m, GearUsed = "Baitcasting", BaitUsed = "Live Worm",
                    FishingMethod = "Bottom Fishing", Notes = "Small but feisty", WeatherConditions = "Cloudy"
                }
            };

            await context.Catches.AddRangeAsync(catches);
        }
    }
    private static async Task SeedContentCategoriesAsync(ApplicationDbContext context)
    {
        if (await context.ContentCategories.AnyAsync()) return;

        var categories = new List<ContentCategory>
    {
        new() { Name = "Casting", Type = CategoryType.FishingMethod },
        new() { Name = "Bottom Fishing", Type = CategoryType.FishingMethod },
        new() { Name = "Fly Fishing", Type = CategoryType.FishingMethod },
        new() { Name = "Spinning Rod", Type = CategoryType.GearType },
        new() { Name = "Baitcasting", Type = CategoryType.GearType },
        new() { Name = "Live Worm", Type = CategoryType.Bait },
        new() { Name = "Spinnerbait", Type = CategoryType.Bait }
    };

        await context.ContentCategories.AddRangeAsync(categories);
    }

}