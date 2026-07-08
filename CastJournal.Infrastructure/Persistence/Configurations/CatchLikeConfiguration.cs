using CastJournal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CastJournal.Infrastructure.Persistence.Configurations;
public class CatchLikeConfiguration : IEntityTypeConfiguration<CatchLike>
{
    public void Configure(EntityTypeBuilder<CatchLike> builder)
    {
        builder.HasKey(l => l.Id);

        builder.HasIndex(l => new { l.CatchId, l.UserId }).IsUnique();

        builder.HasOne(l => l.Catch)
               .WithMany()
               .HasForeignKey(l => l.CatchId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(l => l.User)
               .WithMany()
               .HasForeignKey(l => l.UserId)
               .OnDelete(DeleteBehavior.Restrict);

        // Matches Catch's own soft-delete filter — keeps likes on deleted catches
        // from appearing in query shapes that don't expect them (same pattern as CatchMediaConfiguration).
        builder.HasQueryFilter(l => !l.Catch!.IsDeleted);
    }
}