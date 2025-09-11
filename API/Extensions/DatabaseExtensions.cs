using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Infrastructure.Data;
using Infrastructure.Repositories.FlightBooking.Po;
using Domain.FlightBooking.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace ZackPlay.Extensions;

public static class DatabaseExtensions
{
    /// <summary>
    /// 初始化数据库
    /// </summary>
    public static async Task<IHost> InitializeDatabaseAsync(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FlightBookingDbContext>();

        // 应用任何挂起的迁移
        await context.Database.MigrateAsync();

        // 种子数据
        await SeedDataAsync(context);

        return host;
    }

    private static async Task SeedDataAsync(FlightBookingDbContext context)
    {
        // 如果已有数据则跳过
        if (context.Airports.Any())
            return;

        // 添加机场数据
        var airports = new List<AirportPo>
        {
            new() { Code = "PEK", Name = "北京首都国际机场", City = "北京", Country = "中国", TimeZone = "Asia/Shanghai", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Code = "SHA", Name = "上海虹桥国际机场", City = "上海", Country = "中国", TimeZone = "Asia/Shanghai", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Code = "CAN", Name = "广州白云国际机场", City = "广州", Country = "中国", TimeZone = "Asia/Shanghai", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Code = "SZX", Name = "深圳宝安国际机场", City = "深圳", Country = "中国", TimeZone = "Asia/Shanghai", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Code = "LAX", Name = "洛杉矶国际机场", City = "洛杉矶", Country = "美国", TimeZone = "America/Los_Angeles", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Code = "JFK", Name = "肯尼迪国际机场", City = "纽约", Country = "美国", TimeZone = "America/New_York", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Code = "LHR", Name = "伦敦希思罗机场", City = "伦敦", Country = "英国", TimeZone = "Europe/London", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Code = "NRT", Name = "东京成田国际机场", City = "东京", Country = "日本", TimeZone = "Asia/Tokyo", IsActive = true, CreatedAt = DateTime.UtcNow }
        };

        context.Airports.AddRange(airports);

        // 添加航班数据
        var flights = new List<FlightPo>
        {
            new()
            {
                Id = Guid.NewGuid(),
                FlightNumber = "CA101",
                AirlineCode = "CA",
                AirlineName = "中国国际航空",
                DepartureAirportCode = "PEK",
                ArrivalAirportCode = "SHA",
                DepartureTime = DateTime.Today.AddDays(1).AddHours(8),
                ArrivalTime = DateTime.Today.AddDays(1).AddHours(10).AddMinutes(30),
                DepartureTerminal = "T3",
                ArrivalTerminal = "T2",
                AircraftType = "A320",
                TotalSeats = 180,
                AvailableSeats = 180,
                BasePrice = 800,
                Status = (int)FlightStatus.Scheduled,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                FlightNumber = "MU202",
                AirlineCode = "MU",
                AirlineName = "中国东方航空",
                DepartureAirportCode = "SHA",
                ArrivalAirportCode = "CAN",
                DepartureTime = DateTime.Today.AddDays(1).AddHours(14),
                ArrivalTime = DateTime.Today.AddDays(1).AddHours(17),
                DepartureTerminal = "T1",
                ArrivalTerminal = "T2",
                AircraftType = "B737",
                TotalSeats = 150,
                AvailableSeats = 150,
                BasePrice = 900,
                Status = (int)FlightStatus.Scheduled,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                FlightNumber = "CZ303",
                AirlineCode = "CZ",
                AirlineName = "中国南方航空",
                DepartureAirportCode = "CAN",
                ArrivalAirportCode = "SZX",
                DepartureTime = DateTime.Today.AddDays(2).AddHours(9),
                ArrivalTime = DateTime.Today.AddDays(2).AddHours(10).AddMinutes(15),
                DepartureTerminal = "T1",
                ArrivalTerminal = "T3",
                AircraftType = "A319",
                TotalSeats = 120,
                AvailableSeats = 120,
                BasePrice = 400,
                Status = (int)FlightStatus.Scheduled,
                CreatedAt = DateTime.UtcNow
            }
        };

        context.Flights.AddRange(flights);

        await context.SaveChangesAsync();
    }
}
