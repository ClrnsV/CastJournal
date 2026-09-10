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

    // Common Philippine sport/food fish, spanning freshwater, brackish, and saltwater species.
    // ImageUrl points to freely-licensed Wikimedia Commons photos (CC BY / CC BY-SA) for species
    // that are also used as sample catches in SeedTestUserAndCatchesAsync.
    private static List<Species> GetPhilippineSpeciesCatalog() => new()
    {
        // --- Freshwater ---
        new Species
        {
            Id = Guid.NewGuid(), CommonName = "Tilapia", ScientificName = "Oreochromis niloticus",
            Category = "Freshwater", AverageSize = "20-35 cm",
            Description = "Introduced cichlid, now the most widely farmed and caught freshwater fish in the Philippines. Common in lakes, rivers, and fishponds nationwide.",
            ImageUrl = "https://commons.wikimedia.org/wiki/Special:FilePath/Oreochromis_niloticus_(44653802502).jpg",
            IsApproved = true
        },
        new Species
        {
            Id = Guid.NewGuid(), CommonName = "Dalag (Mudfish / Snakehead)", ScientificName = "Channa striata",
            Category = "Freshwater", AverageSize = "30-50 cm",
            Description = "Air-breathing snakehead found in rice paddies, swamps, and slow-moving rivers. A popular bottom-fishing and net catch.",
            ImageUrl = "https://commons.wikimedia.org/wiki/Special:FilePath/Channa_striata_(striped_snakehead).jpg",
            IsApproved = true
        },
        new Species
        {
            Id = Guid.NewGuid(), CommonName = "Hito (Catfish)", ScientificName = "Clarias batrachus",
            Category = "Freshwater", AverageSize = "20-35 cm",
            Description = "Walking catfish common in ponds, canals, and marshes; tolerant of low-oxygen water.",
            IsApproved = true
        },
        new Species
        {
            Id = Guid.NewGuid(), CommonName = "Ayungin (Silver Perch)", ScientificName = "Leiopotherapon plumbeus",
            Category = "Freshwater", AverageSize = "10-15 cm",
            Description = "Small native freshwater fish once abundant in Laguna de Bay; a traditional Filipino table fish.",
            IsApproved = true
        },
        new Species
        {
            Id = Guid.NewGuid(), CommonName = "Martiniko (Climbing Perch)", ScientificName = "Anabas testudineus",
            Category = "Freshwater", AverageSize = "15-20 cm",
            Description = "Hardy labyrinth fish capable of surviving out of water for short periods; found in ponds and flooded fields.",
            IsApproved = true
        },
        new Species
        {
            Id = Guid.NewGuid(), CommonName = "Karpa (Common Carp)", ScientificName = "Cyprinus carpio",
            Category = "Freshwater", AverageSize = "30-50 cm",
            Description = "Introduced carp stocked in lakes and reservoirs; a favorite among freshwater anglers.",
            IsApproved = true
        },
        new Species
        {
            Id = Guid.NewGuid(), CommonName = "Igat (Freshwater Eel)", ScientificName = "Anguilla marmorata",
            Category = "Freshwater", AverageSize = "50-100 cm",
            Description = "Catadromous eel found in rivers and estuaries; migrates to the sea to spawn.",
            IsApproved = true
        },

        // --- Brackish water ---
        new Species
        {
            Id = Guid.NewGuid(), CommonName = "Bangus (Milkfish)", ScientificName = "Chanos chanos",
            Category = "Brackish", AverageSize = "30-60 cm",
            Description = "The Philippines' national fish, farmed extensively in brackish-water fishponds and caught along coastal flats.",
            ImageUrl = "https://commons.wikimedia.org/wiki/Special:FilePath/Milkfish.jpg",
            IsApproved = true
        },
        new Species
        {
            Id = Guid.NewGuid(), CommonName = "Apahap (Barramundi / Sea Bass)", ScientificName = "Lates calcarifer",
            Category = "Brackish", AverageSize = "40-80 cm",
            Description = "Prized gamefish found in estuaries, mangroves, and river mouths; also widely aquacultured.",
            IsApproved = true
        },
        new Species
        {
            Id = Guid.NewGuid(), CommonName = "Kanduli (Silver Catfish)", ScientificName = "Arius manillensis",
            Category = "Brackish", AverageSize = "25-40 cm",
            Description = "Marine/brackish catfish common in estuaries and river mouths around Manila Bay.",
            IsApproved = true
        },
        new Species
        {
            Id = Guid.NewGuid(), CommonName = "Kitang (Spotted Scat)", ScientificName = "Scatophagus argus",
            Category = "Brackish", AverageSize = "15-25 cm",
            Description = "Deep-bodied, spotted fish found around mangroves and river mouths.",
            IsApproved = true
        },

        // --- Saltwater ---
        new Species
        {
            Id = Guid.NewGuid(), CommonName = "Lapu-Lapu (Grouper)", ScientificName = "Epinephelus fuscoguttatus",
            Category = "Saltwater", AverageSize = "40-70 cm",
            Description = "Reef-associated grouper, one of the most sought-after food fish and jigging targets in the Philippines.",
            ImageUrl = "https://commons.wikimedia.org/wiki/Special:FilePath/Brown-marbled_grouper_(Epinephelus_fuscoguttatus).jpg",
            IsApproved = true
        },
        new Species
        {
            Id = Guid.NewGuid(), CommonName = "Tulingan (Skipjack Tuna)", ScientificName = "Katsuwonus pelamis",
            Category = "Saltwater", AverageSize = "40-60 cm",
            Description = "Fast-swimming pelagic tuna caught by trolling and handline across Philippine waters.",
            ImageUrl = "https://commons.wikimedia.org/wiki/Special:FilePath/Skipjack_tuna_(Katsuwonus_pelamis)_in_a_Philippine_fish_market.jpg",
            IsApproved = true
        },
        new Species
        {
            Id = Guid.NewGuid(), CommonName = "Tanigue (Spanish Mackerel)", ScientificName = "Scomberomorus commerson",
            Category = "Saltwater", AverageSize = "60-100 cm",
            Description = "Large, fast pelagic gamefish highly valued for trolling and as sashimi-grade food fish.",
            IsApproved = true
        },
        new Species
        {
            Id = Guid.NewGuid(), CommonName = "Alumahan (Indian Mackerel)", ScientificName = "Rastrelliger kanagurta",
            Category = "Saltwater", AverageSize = "20-30 cm",
            Description = "Schooling pelagic fish common in coastal waters; a staple catch for handline and net fishers.",
            IsApproved = true
        },
        new Species
        {
            Id = Guid.NewGuid(), CommonName = "Galunggong (Round Scad)", ScientificName = "Decapterus macrosoma",
            Category = "Saltwater", AverageSize = "15-25 cm",
            Description = "One of the most commonly caught and consumed fish in the Philippines, forming large coastal schools.",
            IsApproved = true
        },
        new Species
        {
            Id = Guid.NewGuid(), CommonName = "Talakitok (Giant Trevally)", ScientificName = "Caranx ignobilis",
            Category = "Saltwater", AverageSize = "50-90 cm",
            Description = "Powerful reef and inshore predator, a favorite jigging and popping target for sport anglers.",
            IsApproved = true
        },
        new Species
        {
            Id = Guid.NewGuid(), CommonName = "Maya-Maya (Mangrove Red Snapper)", ScientificName = "Lutjanus argentimaculatus",
            Category = "Saltwater", AverageSize = "30-60 cm",
            Description = "Snapper found around mangroves, reefs, and estuaries; moves between fresh, brackish, and marine water.",
            IsApproved = true
        },
        new Species
        {
            Id = Guid.NewGuid(), CommonName = "Bisugo (Threadfin Bream)", ScientificName = "Nemipterus japonicus",
            Category = "Saltwater", AverageSize = "15-25 cm",
            Description = "Small bottom-dwelling reef fish, a common catch on hook-and-line over sandy/muddy bottoms.",
            IsApproved = true
        },
        new Species
        {
            Id = Guid.NewGuid(), CommonName = "Dilis (Anchovy)", ScientificName = "Stolephorus commersonnii",
            Category = "Saltwater", AverageSize = "5-10 cm",
            Description = "Small schooling baitfish, usually caught in bulk by net and dried or fermented into bagoong.",
            IsApproved = true
        },
        new Species
        {
            Id = Guid.NewGuid(), CommonName = "Sapsap (Ponyfish)", ScientificName = "Leiognathus equulus",
            Category = "Saltwater", AverageSize = "10-18 cm",
            Description = "Small, silvery fish common in shallow coastal waters and sandy bays.",
            IsApproved = true
        },
        new Species
        {
            Id = Guid.NewGuid(), CommonName = "Matangbaka (Bigeye Scad)", ScientificName = "Selar crumenophthalmus",
            Category = "Saltwater", AverageSize = "15-25 cm",
            Description = "Schooling scad with large eyes, caught around reefs and open coastal waters.",
            IsApproved = true
        },
        new Species
        {
            Id = Guid.NewGuid(), CommonName = "Labahita (Surgeonfish)", ScientificName = "Acanthurus lineatus",
            Category = "Saltwater", AverageSize = "20-30 cm",
            Description = "Reef surgeonfish named for the sharp spine near its tail; common around coral reefs.",
            IsApproved = true
        },
        new Species
        {
            Id = Guid.NewGuid(), CommonName = "Malasugi (Blue Marlin)", ScientificName = "Makaira mazara",
            Category = "Saltwater", AverageSize = "200-350 cm",
            Description = "Large offshore billfish and prized big-game trolling target, notably off General Santos and Bohol Sea.",
            IsApproved = true
        },
    };

    private static async Task SeedSpeciesAsync(ApplicationDbContext context)
    {
        if (await context.Species.AnyAsync()) return;

        var species = GetPhilippineSpeciesCatalog();

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
            var speciesList = GetPhilippineSpeciesCatalog();
            await context.Species.AddRangeAsync(speciesList);
            await context.SaveChangesAsync();
        }

        // Sample fishing location for the test user, used by the seeded catches below.
        var testLocation = await context.FishingLocations.FirstOrDefaultAsync(l => l.UserId == testUserId);
        if (testLocation == null)
        {
            testLocation = new FishingLocation
            {
                Id = Guid.NewGuid(),
                Name = "Cagayan de Oro River Mouth",
                Description = "Brackish-water spot popular for bangus and apahap.",
                Latitude = 8.4822,
                Longitude = 124.6472,
                WaterType = WaterType.Brackish,
                IsPublic = true,
                UserId = testUserId
            };
            await context.FishingLocations.AddAsync(testLocation);
            await context.SaveChangesAsync();
        }

        if (!await context.Catches.AnyAsync(c => c.UserId == testUserId))
        {
            // Look up seeded species by common name so catches line up with real, photographed fish.
            var bangus = await context.Species.FirstAsync(s => s.CommonName.StartsWith("Bangus"));
            var tilapia = await context.Species.FirstAsync(s => s.CommonName == "Tilapia");
            var lapuLapu = await context.Species.FirstAsync(s => s.CommonName.StartsWith("Lapu-Lapu"));
            var tulingan = await context.Species.FirstAsync(s => s.CommonName.StartsWith("Tulingan"));
            var dalag = await context.Species.FirstAsync(s => s.CommonName.StartsWith("Dalag"));

            var catches = new List<Catch>
            {
                new Catch
                {
                    UserId = testUserId, SpeciesId = bangus.Id, LocationId = testLocation.Id,
                    CatchDate = DateTime.UtcNow.AddDays(-6),
                    Weight = 1.4m, Length = 42.0m, GearUsed = "Bamboo Pole", BaitUsed = "Lumot (Algae Paste)",
                    FishingMethod = "Still Fishing", Notes = "Caught right at sunrise near the fish pen.",
                    WeatherConditions = "Clear", IsPublic = true,
                    Media = new List<CatchMedia>
                    {
                        new CatchMedia
                        {
                            MediaUrl = "https://commons.wikimedia.org/wiki/Special:FilePath/Milkfish.jpg",
                            FileName = "bangus-catch.jpg", MediaType = MediaType.Image, OrderIndex = 0
                        }
                    }
                },
                new Catch
                {
                    UserId = testUserId, SpeciesId = tilapia.Id, LocationId = testLocation.Id,
                    CatchDate = DateTime.UtcNow.AddDays(-5),
                    Weight = 0.65m, Length = 27.5m, GearUsed = "Spinning Rod", BaitUsed = "Earthworm",
                    FishingMethod = "Casting", Notes = "Small but plenty of them biting today.",
                    WeatherConditions = "Partly Cloudy", IsPublic = true,
                    Media = new List<CatchMedia>
                    {
                        new CatchMedia
                        {
                            MediaUrl = "https://commons.wikimedia.org/wiki/Special:FilePath/Oreochromis_niloticus_(44653802502).jpg",
                            FileName = "tilapia-catch.jpg", MediaType = MediaType.Image, OrderIndex = 0
                        }
                    }
                },
                new Catch
                {
                    UserId = testUserId, SpeciesId = lapuLapu.Id, LocationId = testLocation.Id,
                    CatchDate = DateTime.UtcNow.AddDays(-3),
                    Weight = 2.8m, Length = 48.0m, GearUsed = "Jigging Rod", BaitUsed = "Metal Jig",
                    FishingMethod = "Jigging", Notes = "Solid fight from the reef edge — kept for dinner.",
                    WeatherConditions = "Sunny", IsPublic = true,
                    Media = new List<CatchMedia>
                    {
                        new CatchMedia
                        {
                            MediaUrl = "https://commons.wikimedia.org/wiki/Special:FilePath/Brown-marbled_grouper_(Epinephelus_fuscoguttatus).jpg",
                            FileName = "lapu-lapu-catch.jpg", MediaType = MediaType.Image, OrderIndex = 0
                        }
                    }
                },
                new Catch
                {
                    UserId = testUserId, SpeciesId = tulingan.Id, LocationId = testLocation.Id,
                    CatchDate = DateTime.UtcNow.AddDays(-2),
                    Weight = 1.9m, Length = 45.0m, GearUsed = "Trolling Rod", BaitUsed = "Cedar Plug",
                    FishingMethod = "Trolling", Notes = "Hit hard about 200m off the point while trolling.",
                    WeatherConditions = "Windy", IsPublic = true,
                    Media = new List<CatchMedia>
                    {
                        new CatchMedia
                        {
                            MediaUrl = "https://commons.wikimedia.org/wiki/Special:FilePath/Skipjack_tuna_(Katsuwonus_pelamis)_in_a_Philippine_fish_market.jpg",
                            FileName = "tulingan-catch.jpg", MediaType = MediaType.Image, OrderIndex = 0
                        }
                    }
                },
                new Catch
                {
                    UserId = testUserId, SpeciesId = dalag.Id, LocationId = testLocation.Id,
                    CatchDate = DateTime.UtcNow.AddDays(-1),
                    Weight = 0.9m, Length = 33.0m, GearUsed = "Baitcasting", BaitUsed = "Live Frog",
                    FishingMethod = "Bottom Fishing", Notes = "Pulled this one out of the flooded rice paddy.",
                    WeatherConditions = "Overcast", IsPublic = true,
                    Media = new List<CatchMedia>
                    {
                        new CatchMedia
                        {
                            MediaUrl = "https://commons.wikimedia.org/wiki/Special:FilePath/Channa_striata_(striped_snakehead).jpg",
                            FileName = "dalag-catch.jpg", MediaType = MediaType.Image, OrderIndex = 0
                        }
                    }
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
        new() { Name = "Jigging", Type = CategoryType.FishingMethod },
        new() { Name = "Trolling", Type = CategoryType.FishingMethod },
        new() { Name = "Still Fishing", Type = CategoryType.FishingMethod },
        new() { Name = "Spinning Rod", Type = CategoryType.GearType },
        new() { Name = "Baitcasting", Type = CategoryType.GearType },
        new() { Name = "Bamboo Pole", Type = CategoryType.GearType },
        new() { Name = "Jigging Rod", Type = CategoryType.GearType },
        new() { Name = "Trolling Rod", Type = CategoryType.GearType },
        new() { Name = "Live Worm", Type = CategoryType.Bait },
        new() { Name = "Spinnerbait", Type = CategoryType.Bait },
        new() { Name = "Earthworm", Type = CategoryType.Bait },
        new() { Name = "Lumot (Algae Paste)", Type = CategoryType.Bait },
        new() { Name = "Metal Jig", Type = CategoryType.Bait },
        new() { Name = "Cedar Plug", Type = CategoryType.Bait },
        new() { Name = "Live Frog", Type = CategoryType.Bait }
    };

        await context.ContentCategories.AddRangeAsync(categories);
    }

}