using CastJournal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CastJournal.Infrastructure.Persistence.Configurations;
public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Content).HasMaxLength(1000).IsRequired();

        builder.HasIndex(c => c.CatchId);

        builder.HasOne(c => c.Catch)
               .WithMany()
               .HasForeignKey(c => c.CatchId)
               .OnDelete(DeleteBehavior.Cascade); // deleting a catch removes its comments

        // Restrict, not Cascade — same reasoning as UserFollowConfiguration:
        // avoids "multiple cascade paths" once combined with Catch -> User.
        builder.HasOne(c => c.User)
               .WithMany()
               .HasForeignKey(c => c.UserId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(c => !c.Catch!.IsDeleted);

    }
}