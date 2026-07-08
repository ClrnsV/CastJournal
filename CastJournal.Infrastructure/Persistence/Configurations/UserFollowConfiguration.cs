using CastJournal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;



namespace CastJournal.Infrastructure.Persistence.Configurations;
public class UserFollowConfiguration : IEntityTypeConfiguration<UserFollow>
{
    public void Configure(EntityTypeBuilder<UserFollow> builder)
    {
        builder.HasKey(f => f.Id);

        // Prevents the same follow relationship being inserted twice.
        builder.HasIndex(f => new { f.FollowerId, f.FollowingId }).IsUnique();

        // Both FKs point at the same table (AspNetUsers) — SQL Server rejects cascade delete
        // on both sides at once ("multiple cascade paths"), so both must be Restrict here.
        builder.HasOne(f => f.Follower)
               .WithMany()
               .HasForeignKey(f => f.FollowerId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(f => f.Following)
               .WithMany()
               .HasForeignKey(f => f.FollowingId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}