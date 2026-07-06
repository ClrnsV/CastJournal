using CastJournal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace CastJournal.Infrastructure.Persistence.Configurations;

public class CatchMediaConfiguration : IEntityTypeConfiguration<CatchMedia>
{
    public void Configure(EntityTypeBuilder<CatchMedia> builder)
    {
        builder.HasQueryFilter(m => !m.Catch!.IsDeleted);
    }
}