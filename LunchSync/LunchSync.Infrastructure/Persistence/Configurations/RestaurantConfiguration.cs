using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LunchSync.Core.Modules.RestaurantsAndDishes.Entities;
using LunchSync.Core.Common.Enums;

namespace LunchSync.Infrastructure.Persistence.Configurations;

public class RestaurantConfiguration : IEntityTypeConfiguration<Restaurant>
{
    public void Configure(EntityTypeBuilder<Restaurant> builder)
    {
        builder.ToTable("restaurants");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(r => r.Name).IsRequired().HasMaxLength(255);
        builder.Property(r => r.Address).IsRequired().HasMaxLength(500);
        builder.Property(r => r.PriceTier).IsRequired().HasConversion<string>();
        builder.Property(r => r.Status).IsRequired().HasConversion<string>().HasDefaultValue(RestaurantStatus.Active);
        builder.Property(r => r.Source).IsRequired().HasConversion<string>().HasDefaultValue(RestaurantSource.Admin);
        builder.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("NOW()")
               .HasColumnType("timestamp with time zone");
    }
}