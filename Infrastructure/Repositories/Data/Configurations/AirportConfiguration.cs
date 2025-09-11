using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Infrastructure.Repositories.FlightBooking.Po;

namespace Infrastructure.Repositories.Data.Configurations;

public class AirportConfiguration : IEntityTypeConfiguration<AirportPo>
{
    public void Configure(EntityTypeBuilder<AirportPo> builder)
    {
        builder.ToTable("airports");
        builder.HasKey(e => e.Code);

        builder.Property(e => e.Code)
            .HasMaxLength(3)
            .IsRequired()
            .HasComment("机场三字码，如PEK、SHA");

        builder.Property(e => e.Name)
            .HasMaxLength(200)
            .IsRequired()
            .HasComment("机场名称");

        builder.Property(e => e.City)
            .HasMaxLength(100)
            .IsRequired()
            .HasComment("所在城市");

        builder.Property(e => e.Country)
            .HasMaxLength(100)
            .IsRequired()
            .HasComment("所在国家");

        builder.Property(e => e.TimeZone)
            .HasMaxLength(50)
            .IsRequired()
            .HasComment("时区");

        builder.Property(e => e.IsActive)
            .HasComment("是否启用");

        builder.HasIndex(e => e.Country).HasDatabaseName("IX_airports_country");
        builder.HasIndex(e => e.City).HasDatabaseName("IX_airports_city");
    }
}
