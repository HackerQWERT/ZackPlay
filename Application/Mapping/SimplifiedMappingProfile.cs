using Mapster;
using Domain.FlightBooking.Entities;
using Application.Contracts;

namespace Application.Mapping;

/// <summary>
/// 简化的应用层映射配置
/// 负责领域对象 ↔ API Contract 之间的映射
/// 
/// 架构简化：
/// - 移除了中间的 DTO 层
/// - 直接使用 API Contract 作为数据传输对象
/// - 减少了重复定义和维护成本
/// </summary>
public static class SimplifiedMappingProfile
{
    public static void Configure()
    {
        ConfigureAirportMappings();
        ConfigureFlightMappings();
        ConfigurePassengerMappings();
        ConfigureBookingMappings();
    }

    /// <summary>
    /// 配置机场相关映射
    /// </summary>
    private static void ConfigureAirportMappings()
    {
        TypeAdapterConfig<Airport, AirportResponse>
            .NewConfig()
            .Map(dest => dest.DisplayName, src => $"{src.Name} ({src.Code})")
            .Map(dest => dest.Location, src => $"{src.City}, {src.Country}");
    }

    /// <summary>
    /// 配置航班相关映射
    /// </summary>
    private static void ConfigureFlightMappings()
    {
        TypeAdapterConfig<Flight, FlightSearchResponse>
            .NewConfig()
            .Map(dest => dest.Airline, src => src.AirlineName)
            .Map(dest => dest.Duration, src => CalculateFlightDuration(src.DepartureTime, src.ArrivalTime))
            .Map(dest => dest.Price, src => src.BasePrice)
            .Map(dest => dest.PriceDisplay, src => $"¥{src.BasePrice:N0}")
            .Map(dest => dest.Aircraft, src => src.AircraftType)
            .Map(dest => dest.IsDirectFlight, src => true) // 简化处理
            .Map(dest => dest.Amenities, src => new[] { "WiFi", "餐食", "娱乐系统" })
            .Map(dest => dest.Departure, src => MapToDepartureInfo(src))
            .Map(dest => dest.Arrival, src => MapToArrivalInfo(src));

        TypeAdapterConfig<Flight, FlightInfo>
            .NewConfig()
            .Map(dest => dest.Airline, src => src.AirlineName)
            .Map(dest => dest.Departure, src => MapToDepartureInfo(src))
            .Map(dest => dest.Arrival, src => MapToArrivalInfo(src));
    }

    /// <summary>
    /// 配置乘客相关映射
    /// </summary>
    private static void ConfigurePassengerMappings()
    {
        TypeAdapterConfig<Passenger, PassengerInfo>
            .NewConfig()
            .Map(dest => dest.FullName, src => $"{src.FirstName} {src.LastName}");
    }

    /// <summary>
    /// 配置预订相关映射
    /// </summary>
    private static void ConfigureBookingMappings()
    {
        TypeAdapterConfig<Domain.FlightBooking.Entities.FlightBooking, FlightBookingResponse>
            .NewConfig()
            .Map(dest => dest.Status, src => src.Status.ToString());
    }

    /// <summary>
    /// 映射到出发信息
    /// </summary>
    private static DepartureInfo MapToDepartureInfo(Flight flight)
    {
        return new DepartureInfo
        {
            AirportCode = flight.DepartureAirportCode,
            AirportName = $"{flight.DepartureAirportCode} 机场", // 简化处理，实际应该查询机场信息
            City = "出发城市", // 简化处理
            Terminal = flight.DepartureTerminal,
            Time = flight.DepartureTime,
            TimeDisplay = flight.DepartureTime.ToString("HH:mm"),
            DateDisplay = flight.DepartureTime.ToString("M月d日 dddd")
        };
    }

    /// <summary>
    /// 映射到到达信息
    /// </summary>
    private static ArrivalInfo MapToArrivalInfo(Flight flight)
    {
        return new ArrivalInfo
        {
            AirportCode = flight.ArrivalAirportCode,
            AirportName = $"{flight.ArrivalAirportCode} 机场", // 简化处理，实际应该查询机场信息
            City = "到达城市", // 简化处理
            Terminal = flight.ArrivalTerminal,
            Time = flight.ArrivalTime,
            TimeDisplay = flight.ArrivalTime.ToString("HH:mm"),
            DateDisplay = flight.ArrivalTime.ToString("M月d日 dddd")
        };
    }

    /// <summary>
    /// 计算航班飞行时长
    /// </summary>
    private static string CalculateFlightDuration(DateTime departure, DateTime arrival)
    {
        var duration = arrival - departure;
        var hours = (int)duration.TotalHours;
        var minutes = duration.Minutes;

        if (hours > 0 && minutes > 0)
            return $"{hours}小时{minutes}分钟";
        else if (hours > 0)
            return $"{hours}小时";
        else
            return $"{minutes}分钟";
    }
}
