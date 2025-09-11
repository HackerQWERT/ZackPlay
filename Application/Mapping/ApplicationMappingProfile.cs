using Mapster;
using Application.DTOs;
using Application.DTOs.Requests;
using Application.DTOs.Responses;

namespace Application.Mapping;

/// <summary>
/// 应用层映射配置
/// 负责 DTO ↔ Request/Response 之间的映射
/// 使用 Mapster 提高效率，减少样板代码
/// </summary>
public static class ApplicationMappingProfile
{
    public static void Configure()
    {
        ConfigureAirportMappings();
        ConfigureFlightMappings();
    }

    /// <summary>
    /// 配置机场相关映射
    /// </summary>
    private static void ConfigureAirportMappings()
    {
        TypeAdapterConfig<AirportDto, AirportResponse>
            .NewConfig()
            .Map(dest => dest.DisplayName, src => $"{src.Name} ({src.Code})")
            .Map(dest => dest.Location, src => $"{src.City}, {src.Country}");
    }

    /// <summary>
    /// 配置航班相关映射
    /// </summary>
    private static void ConfigureFlightMappings()
    {
        TypeAdapterConfig<FlightSearchDto, FlightSearchRequest>
            .NewConfig()
            .Map(dest => dest.From, src => src.DepartureAirport)
            .Map(dest => dest.To, src => src.ArrivalAirport)
            .Map(dest => dest.DepartureDate, src => src.DepartureDate.ToString("yyyy-MM-dd"))
            .Map(dest => dest.ReturnDate, src => src.ReturnDate != null ? src.ReturnDate.Value.ToString("yyyy-MM-dd") : null)
            .Map(dest => dest.PassengerCount, src => src.AdultCount + src.ChildCount + src.InfantCount)
            .Map(dest => dest.Class, src => src.CabinClass.ToLower());

        TypeAdapterConfig<FlightInfoDto, FlightSearchResultResponse>
            .NewConfig()
            .Map(dest => dest.Airline, src => src.AirlineName)
            .Map(dest => dest.Duration, src => CalculateFlightDuration(src.DepartureTime, src.ArrivalTime))
            .Map(dest => dest.Price, src => src.BasePrice)
            .Map(dest => dest.PriceDisplay, src => $"¥{src.BasePrice:N0}")
            .Map(dest => dest.Aircraft, src => src.AircraftType)
            .Map(dest => dest.IsDirectFlight, src => true) // 简化处理，实际应该根据航班信息判断
            .Map(dest => dest.Amenities, src => new[] { "WiFi", "餐食", "娱乐系统" }) // 示例数据
            .Map(dest => dest.Departure, src => new DepartureInfoResponse
            {
                AirportCode = src.DepartureAirportCode,
                Terminal = src.DepartureTerminal,
                Time = src.DepartureTime,
                TimeDisplay = src.DepartureTime.ToString("HH:mm"),
                DateDisplay = src.DepartureTime.ToString("M月d日 dddd")
            })
            .Map(dest => dest.Arrival, src => new ArrivalInfoResponse
            {
                AirportCode = src.ArrivalAirportCode,
                Terminal = src.ArrivalTerminal,
                Time = src.ArrivalTime,
                TimeDisplay = src.ArrivalTime.ToString("HH:mm"),
                DateDisplay = src.ArrivalTime.ToString("M月d日 dddd")
            });
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
