using CastJournal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CastJournal.Infrastructure.Persistence.Configurations;

public class RevokedTokenConfiguration : IEntityTypeConfiguration<RevokedToken>
{
    public void Configure(EntityTypeBuilder<RevokedToken> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Jti).IsRequired().HasMaxLength(100);
        builder.HasIndex(r => r.Jti).IsUnique();   // fast lookup on every authenticated request
    }
}