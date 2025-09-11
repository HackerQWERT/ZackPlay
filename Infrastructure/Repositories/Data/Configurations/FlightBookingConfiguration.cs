using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Infrastructure.Repositories.FlightBooking.Po;

namespace Infrastructure.Repositories.Data.Configurations;

public class FlightBookingConfiguration : IEntityTypeConfiguration<FlightBookingPo>
{
    public void Configure(EntityTypeBuilder<FlightBookingPo> builder)
    {
        builder.ToTable("flight_bookings");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.BookingReference)
            .HasMaxLength(20)
            .IsRequired()
            .HasComment("预订参考号");

        builder.Property(e => e.CabinClass)
            .HasComment("舱位等级：0=经济舱,1=高端经济舱,2=商务舱,3=头等舱");

        builder.Property(e => e.UnitPrice)
            .HasColumnType("decimal(18,2)")
            .HasComment("单价");

        builder.Property(e => e.TotalAmount)
            .HasColumnType("decimal(18,2)")
            .HasComment("总金额");

        builder.Property(e => e.RefundAmount)
            .HasColumnType("decimal(18,2)")
            .HasComment("退款金额");

        builder.Property(e => e.SpecialRequests)
            .HasMaxLength(500)
            .HasComment("特殊要求");

        builder.Property(e => e.Status)
            .HasComment("预订状态：0=待确认,1=已确认,2=已登机,3=已取消");

        builder.Property(e => e.PaymentStatus)
            .HasComment("支付状态：0=待支付,1=已支付,2=支付失败,3=已退款");

        builder.Property(e => e.PaymentReference)
            .HasMaxLength(100)
            .HasComment("支付参考号");

        builder.Property(e => e.CancellationReason)
            .HasMaxLength(500)
            .HasComment("取消原因");

        // 外键关系
        builder.HasOne<FlightPo>()
            .WithMany()
            .HasForeignKey(e => e.FlightId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<PassengerPo>()
            .WithMany()
            .HasForeignKey(e => e.PassengerId)
            .OnDelete(DeleteBehavior.Restrict);

        // 索引
        builder.HasIndex(e => e.BookingReference)
            .IsUnique()
            .HasDatabaseName("IX_bookings_reference_unique");

        builder.HasIndex(e => e.FlightId).HasDatabaseName("IX_bookings_flight");
        builder.HasIndex(e => e.PassengerId).HasDatabaseName("IX_bookings_passenger");
        builder.HasIndex(e => e.BookingTime).HasDatabaseName("IX_bookings_time");
        builder.HasIndex(e => e.Status).HasDatabaseName("IX_bookings_status");
        builder.HasIndex(e => e.PaymentStatus).HasDatabaseName("IX_bookings_payment_status");
    }
}
