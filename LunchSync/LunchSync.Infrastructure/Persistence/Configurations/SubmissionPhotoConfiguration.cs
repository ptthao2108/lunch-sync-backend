using LunchSync.Core.Modules.RestaurantsAndDishes.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace  LunchSync.Infrastructure.Persistence.Configurations;

public class SubmissionPhotoConfiguration : IEntityTypeConfiguration<SubmissionPhoto>
{
    public void Configure(EntityTypeBuilder<SubmissionPhoto> builder)
    {
        builder.ToTable("submission_photos");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(p => p.PhotoUrl).IsRequired();
        builder.Property(p => p.DisplayOrder).HasDefaultValue(0);
        builder.Property(p => p.CreatedAt).HasColumnType("timestamp with time zone").HasDefaultValueSql("NOW()");

        // Cấu hình Quan hệ & Cascade Delete
        builder.HasOne(p => p.Submission)
               .WithMany(s => s.Photos)
               .HasForeignKey(p => p.SubmissionId)
               .OnDelete(DeleteBehavior.Cascade); // ON DELETE CASCADE

        // Index cho FK
        builder.HasIndex(p => p.SubmissionId).HasDatabaseName("idx_submission_photos_sub");
    }
}