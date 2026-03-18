using LunchSync.Core.Common.Enums;
using LunchSync.Core.Modules.RestaurantsAndDishes.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LunchSync.Infrastructure.Persistence.Configurations;

public class CollectionConfiguration : IEntityTypeConfiguration<Collection>
{
    public void Configure(EntityTypeBuilder<Collection> builder)
    {
        builder.ToTable("collections");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(c => c.Name).IsRequired().HasMaxLength(255);
        builder.Property(c => c.CoverageRadiusMeters).HasDefaultValue(800);
        builder.Property(c => c.Status).HasConversion<string>().HasDefaultValue(CollectionStatus.Active);
        builder.Property(c => c.CreatedAt).HasDefaultValueSql("NOW()");

    }
}