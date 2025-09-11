using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Infrastructure.Repositories.FlightBooking.Po;

namespace Infrastructure.Repositories.Data.Configurations;

public class PassengerConfiguration : IEntityTypeConfiguration<PassengerPo>
{
    public void Configure(EntityTypeBuilder<PassengerPo> builder)
    {
        builder.ToTable("passengers");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.FirstName)
            .HasMaxLength(50)
            .IsRequired()
            .HasComment("名字");

        builder.Property(e => e.LastName)
            .HasMaxLength(50)
            .IsRequired()
            .HasComment("姓氏");

        builder.Property(e => e.Gender)
            .HasComment("性别：0=男,1=女,2=其他");

        builder.Property(e => e.PassportNumber)
            .HasMaxLength(20)
            .IsRequired()
            .HasComment("护照号码");

        builder.Property(e => e.PassportCountry)
            .HasMaxLength(3)
            .IsRequired()
            .HasComment("护照签发国代码");

        builder.Property(e => e.Nationality)
            .HasMaxLength(50)
            .IsRequired()
            .HasComment("国籍");

        builder.Property(e => e.Email)
            .HasMaxLength(200)
            .IsRequired()
            .HasComment("邮箱地址");

        builder.Property(e => e.PhoneNumber)
            .HasMaxLength(20)
            .IsRequired()
            .HasComment("电话号码");

        builder.Property(e => e.Type)
            .HasComment("乘客类型：0=婴儿,1=儿童,2=成人,3=老人");

        // 唯一约束
        builder.HasIndex(e => e.PassportNumber)
            .IsUnique()
            .HasDatabaseName("IX_passengers_passport_unique");

        builder.HasIndex(e => e.Email)
            .IsUnique()
            .HasDatabaseName("IX_passengers_email_unique");
    }
}
