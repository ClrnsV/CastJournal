using CastJournal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CastJournal.Infrastructure.Persistence.Configurations;

public class CatchConfiguration : IEntityTypeConfiguration<Catch>
{
    public void Configure(EntityTypeBuilder<Catch> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Notes).HasMaxLength(1000);
        builder.Property(c => c.WeatherConditions).HasMaxLength(200);
        builder.Property(c => c.Weight).HasPrecision(10, 4);
        builder.Property(c => c.Length).HasPrecision(10, 2);

        // Soft Delete
        builder.Property(c => c.IsDeleted).HasDefaultValue(false);
        builder.HasQueryFilter(c => !c.IsDeleted);

        // Relationships
        builder.HasOne(c => c.User)
               .WithMany(u => u.Catches)
               .HasForeignKey(c => c.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Species)
               .WithMany(s => s.Catches)
               .HasForeignKey(c => c.SpeciesId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Location)
                .WithMany(l => l.Catches)
                .HasForeignKey(c => c.LocationId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

        // Media Relationship - Make it optional to avoid warning
        builder.HasMany(c => c.Media)
               .WithOne(m => m.Catch)
               .HasForeignKey(m => m.CatchId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}