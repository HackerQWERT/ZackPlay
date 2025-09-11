using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Infrastructure.Repositories.FlightBooking.Po;

namespace Infrastructure.Repositories.Data.Configurations;

public class FlightConfiguration : IEntityTypeConfiguration<FlightPo>
{
    public void Configure(EntityTypeBuilder<FlightPo> builder)
    {
        builder.ToTable("flights");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.FlightNumber)
            .HasMaxLength(16)
            .IsRequired()
            .HasComment("航班号，如CZ3456");

        builder.Property(e => e.AirlineCode)
            .HasMaxLength(3)
            .IsRequired()
            .HasComment("航空公司代码，如CZ");

        builder.Property(e => e.AirlineName)
            .HasMaxLength(100)
            .IsRequired()
            .HasComment("航空公司名称");

        builder.Property(e => e.DepartureAirportCode)
            .HasMaxLength(3)
            .IsRequired()
            .HasComment("出发机场代码");

        builder.Property(e => e.DepartureTerminal)
            .HasMaxLength(10)
            .HasComment("出发航站楼");

        builder.Property(e => e.ArrivalAirportCode)
            .HasMaxLength(3)
            .IsRequired()
            .HasComment("到达机场代码");

        builder.Property(e => e.ArrivalTerminal)
            .HasMaxLength(10)
            .HasComment("到达航站楼");

        builder.Property(e => e.AircraftType)
            .HasMaxLength(20)
            .IsRequired()
            .HasComment("机型，如A320");

        builder.Property(e => e.BasePrice)
            .HasColumnType("decimal(18,2)")
            .HasComment("基础价格");

        builder.Property(e => e.Status)
            .HasComment("航班状态：0=计划中,1=登机中,2=已起飞,3=飞行中,4=已到达,5=延误,6=取消");

        // 组合唯一索引：航班号 + 出发日期
        builder.HasIndex(e => new { e.FlightNumber, e.DepartureTime })
            .IsUnique()
            .HasDatabaseName("IX_flights_number_departure");

        // 查询优化索引
        builder.HasIndex(e => new { e.DepartureAirportCode, e.ArrivalAirportCode, e.DepartureTime })
            .HasDatabaseName("IX_flights_route_time");

        builder.HasIndex(e => e.AirlineCode).HasDatabaseName("IX_flights_airline");
        builder.HasIndex(e => e.Status).HasDatabaseName("IX_flights_status");
    }
}
