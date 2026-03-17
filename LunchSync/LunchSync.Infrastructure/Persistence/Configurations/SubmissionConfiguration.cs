using LunchSync.Core.Modules.RestaurantsAndDishes.Entities;
using LunchSync.Core.Common.Enums;
namespace LunchSync.Infrastructure.Persistence.Configurations;

public class SubmissionConfiguration : IEntityTypeConfiguration<Submission>
{
    public void Configure(EntityTypeBuilder<Submission> builder)
    {
        builder.ToTable("submissions");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasDefaultValueSql("gen_random_uuid()");
        
        builder.Property(s => s.RestaurantName).HasMaxLength(200).IsRequired();
        builder.Property(s => s.Address).IsRequired();
        builder.Property(s => s.Status).HasConversion<string>().HasMaxLength(20).HasDefaultValue(SubmissionStatus.Pending);
        builder.Property(s => s.PriceTier).HasConversion<string>().HasMaxLength(20);
        
        // Khớp với DEFAULT NOW() trong SQL
        builder.Property(s => s.CreatedAt).HasColumnType("timestamp with time zone").HasDefaultValueSql("NOW()");
        builder.Property(s => s.ReviewedAt).HasColumnType("timestamp with time zone");


        //reference user(id)
        builder.HasOne(s => s.User)
            .WithMany(u => u.Submissions)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(s => s.User)
               .WithMany()
               .HasForeignKey(s => s.ReviewedBy)
               .OnDelete(DeleteBehavior.SetNull);

        // Cấu hình Index từ file SQL
        builder.HasIndex(s => s.Status).HasDatabaseName("idx_submissions_status");
        builder.HasIndex(s => s.UserId).HasDatabaseName("idx_submissions_user");
    }
}



