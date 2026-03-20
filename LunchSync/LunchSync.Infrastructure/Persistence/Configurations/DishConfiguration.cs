using LunchSync.Core.Modules.RestaurantsAndDishes.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LunchSync.Infrastructure.Persistence.Configurations;

public class DishConfiguration : IEntityTypeConfiguration<Dish>
{
    public void Configure(EntityTypeBuilder<Dish> builder)
    {
        builder.ToTable("dishes");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).HasDefaultValueSql("gen_random_uuid()");


        builder.Property(d => d.Name).IsRequired().HasMaxLength(100);
        builder.HasIndex(d => d.Name).IsUnique();

        builder.Property(d => d.Version).HasDefaultValue(1);
        builder.Property(d => d.Category).IsRequired().HasMaxLength(50);

        builder.OwnsOne(d => d.Profile, profileBuilder =>
        {
            profileBuilder.ToJson();
        });

        // builder.HasIndex("Profile").HasMethod("gin");


        // Check Constraint đảm bảo profile không bị rỗng/sai định dạng
        builder.ToTable(t => t.HasCheckConstraint("chk_profile_valid", "jsonb_typeof(profile) = 'object'"));

        //TIMESTAMPTZ
        builder.Property(e => e.CreatedAt)
               .IsRequired()
               .HasDefaultValueSql("NOW()")
               .HasColumnType("timestamp with time zone");

        builder.Property(e => e.UpdatedAt)
               .HasDefaultValueSql("NOW()")
               .HasColumnType("timestamp with time zone");

    }
}
