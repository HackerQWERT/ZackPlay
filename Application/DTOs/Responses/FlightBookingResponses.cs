namespace Application.DTOs.Responses;

/// <summary>
/// 机场响应模型
/// 用于 API 响应
/// </summary>
public class AirportResponse
{
    public string Code { get; set; } = default!;
    public string DisplayName { get; set; } = default!; // "北京首都国际机场 (PEK)"
    public string Location { get; set; } = default!; // "北京, 中国"
    public string TimeZone { get; set; } = default!;
}

/// <summary>
/// 航班搜索结果响应模型
/// </summary>
public class FlightSearchResultResponse
{
    public Guid Id { get; set; }
    public string FlightNumber { get; set; } = default!;
    public string Airline { get; set; } = default!; // "中国国际航空"
    public string AirlineCode { get; set; } = default!; // "CA"

    public DepartureInfoResponse Departure { get; set; } = default!;
    public ArrivalInfoResponse Arrival { get; set; } = default!;

    public string Duration { get; set; } = default!; // "2小时30分钟"
    public decimal Price { get; set; }
    public string PriceDisplay { get; set; } = default!; // "¥1,299"
    public int AvailableSeats { get; set; }
    public string Aircraft { get; set; } = default!; // "波音737-800"

    public bool IsDirectFlight { get; set; } = true;
    public string[] Amenities { get; set; } = Array.Empty<string>(); // ["WiFi", "餐食", "娱乐系统"]
}

/// <summary>
/// 出发信息响应模型
/// </summary>
public class DepartureInfoResponse
{
    public string AirportCode { get; set; } = default!; // "PEK"
    public string AirportName { get; set; } = default!; // "北京首都国际机场"
    public string City { get; set; } = default!; // "北京"
    public string Terminal { get; set; } = default!; // "T3"
    public DateTime Time { get; set; }
    public string TimeDisplay { get; set; } = default!; // "08:30"
    public string DateDisplay { get; set; } = default!; // "3月15日 周五"
}

/// <summary>
/// 到达信息响应模型
/// </summary>
public class ArrivalInfoResponse
{
    public string AirportCode { get; set; } = default!; // "SHA"
    public string AirportName { get; set; } = default!; // "上海虹桥国际机场"
    public string City { get; set; } = default!; // "上海"
    public string Terminal { get; set; } = default!; // "T2"
    public DateTime Time { get; set; }
    public string TimeDisplay { get; set; } = default!; // "11:00"
    public string DateDisplay { get; set; } = default!; // "3月15日 周五"
}
