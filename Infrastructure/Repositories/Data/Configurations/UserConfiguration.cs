using Infrastructure.Repositories.User.Po;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Repositories.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<UserPo>
{
    public void Configure(EntityTypeBuilder<UserPo> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Username)
            .HasMaxLength(50)
            .IsRequired()
            .HasComment("登录用户名");

        builder.Property(u => u.NormalizedUsername)
            .HasMaxLength(50)
            .IsRequired()
            .HasComment("规范化用户名（大写）");

        builder.Property(u => u.Email)
            .HasMaxLength(256)
            .IsRequired()
            .HasComment("邮箱");

        builder.Property(u => u.NormalizedEmail)
            .HasMaxLength(256)
            .IsRequired()
            .HasComment("规范化邮箱（大写）");

        builder.Property(u => u.DisplayName)
            .HasMaxLength(100)
            .IsRequired()
            .HasComment("显示名称");

        builder.Property(u => u.PasswordHash)
            .HasMaxLength(512)
            .IsRequired()
            .HasComment("密码哈希");

        builder.Property(u => u.Role)
            .HasMaxLength(32)
            .HasComment("角色");

        builder.Property(u => u.IsActive)
            .IsRequired()
            .HasDefaultValue(true)
            .HasComment("是否启用");

        builder.Property(u => u.CreatedAt)
            .IsRequired()
            .HasComment("创建时间（UTC）");

        builder.Property(u => u.UpdatedAt)
            .HasComment("更新时间（UTC）");

        builder.Property(u => u.LastLoginAt)
            .HasComment("最后登录时间（UTC）");

        builder.HasIndex(u => u.NormalizedUsername)
            .IsUnique()
            .HasDatabaseName("UX_users_normalized_username");

        builder.HasIndex(u => u.NormalizedEmail)
            .IsUnique()
            .HasDatabaseName("UX_users_normalized_email");
    }
}
