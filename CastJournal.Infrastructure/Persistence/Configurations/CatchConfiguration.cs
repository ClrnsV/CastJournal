using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        // Fix for decimal warnings
        builder.Property(c => c.Weight)
               .HasPrecision(10, 4);   //  9999.9999 kg

        builder.Property(c => c.Length)
               .HasPrecision(10, 2);   //  9999.99 cm/inches

        builder.HasOne(c => c.User)
               .WithMany(u => u.Catches)
               .HasForeignKey(c => c.UserId);

        builder.HasOne(c => c.Species)
               .WithMany(s => s.Catches)
               .HasForeignKey(c => c.SpeciesId);

        builder.HasOne(c => c.Location)
               .WithMany(l => l.Catches)
               .HasForeignKey(c => c.LocationId)
               .IsRequired(false);
    }
}