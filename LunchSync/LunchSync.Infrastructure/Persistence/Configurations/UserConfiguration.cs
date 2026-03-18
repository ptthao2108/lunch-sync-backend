using LunchSync.Core.Common.Enums;
using LunchSync.Core.Modules.Auth.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LunchSync.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // 1. Map tên bảng
        builder.ToTable("users");

        // 2. Khóa chính
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasDefaultValueSql("gen_random_uuid()");


        // 3. Cấu hình các cột theo Schema
        builder.Property(u => u.CognitoSub).IsRequired().HasMaxLength(255);

        builder.Property(u => u.Email).IsRequired().HasMaxLength(255);
        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.FullName).HasMaxLength(255);

        builder.Property(u => u.Role).HasConversion<string>().HasDefaultValue(UserRole.Host); // Mặc định là host theo schema

        builder.Property(u => u.IsActive).IsRequired().HasDefaultValue(true);
        builder.Property(u => u.CreatedAt).HasDefaultValueSql("NOW()").HasColumnType("timestamp with time zone");

        // 4. Index (Dựa trên các khối highlight xanh trong hình schema)
        builder.HasIndex(u => u.CognitoSub)
               .IsUnique()
               .HasDatabaseName("idx_users_cognito_sub");

        builder.HasIndex(u => u.Role)
               .HasDatabaseName("idx_users_role");
    }
}