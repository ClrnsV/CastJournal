using CastJournal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CastJournal.Infrastructure.Persistence.Configurations; //this code error and cant migrate

public class LocationConfiguration : IEntityTypeConfiguration<FishingLocation>
{
    public void Configure(EntityTypeBuilder<FishingLocation> builder)
    {
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Name).IsRequired().HasMaxLength(200);
        builder.Property(l => l.Description).HasMaxLength(500);

        
        builder.Property(l => l.IsDeleted).HasDefaultValue(false);
        builder.HasQueryFilter(l => !l.IsDeleted);

      
        //builder.Property<string>("UserId")
        //       .IsRequired();

        builder.HasOne(l => l.User)
               .WithMany(u => u.FishingLocations)
               .HasForeignKey(l => l.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(l => l.Catches)
               .WithOne(c => c.Location)
               .HasForeignKey(c => c.LocationId)
               .IsRequired(false)
               .OnDelete(DeleteBehavior.Restrict);
    }
}