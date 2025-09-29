using Microsoft.EntityFrameworkCore;
using Infrastructure.Repositories.FlightBooking.Po;
using Infrastructure.Repositories.User.Po;

namespace Infrastructure.Data;

/// <summary>
/// 机票预订系统数据库上下文
/// </summary>
public class FlightBookingDbContext : DbContext
{
    public FlightBookingDbContext(DbContextOptions<FlightBookingDbContext> options) : base(options)
    {
    }

    // 机票预订业务核心表
    public DbSet<AirportPo> Airports => Set<AirportPo>();
    public DbSet<FlightPo> Flights => Set<FlightPo>();
    public DbSet<PassengerPo> Passengers => Set<PassengerPo>();
    public DbSet<FlightBookingPo> FlightBookings => Set<FlightBookingPo>();
    public DbSet<UserPo> Users => Set<UserPo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FlightBookingDbContext).Assembly);


    }
}
