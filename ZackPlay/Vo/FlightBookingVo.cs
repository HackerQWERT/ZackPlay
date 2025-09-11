namespace ZackPlay.Vo;

/// <summary>
/// 机场视图模型
/// 用于 API 响应和前端展示
/// </summary>
public class AirportVo
{
    public string Code { get; set; } = default!;
    public string DisplayName { get; set; } = default!; // "北京首都国际机场 (PEK)"
    public string Location { get; set; } = default!; // "北京, 中国"
    public string TimeZone { get; set; } = default!;
}

/// <summary>
/// 航班搜索请求模型
/// </summary>
public class FlightSearchRequest
{
    public string From { get; set; } = default!;
    public string To { get; set; } = default!;
    public string DepartureDate { get; set; } = default!; // "2024-03-15"
    public string? ReturnDate { get; set; }
    public int PassengerCount { get; set; } = 1;
    public string Class { get; set; } = "economy";
}

/// <summary>
/// 航班搜索结果视图模型
/// </summary>
public class FlightSearchResultVo
{
    public Guid Id { get; set; }
    public string FlightNumber { get; set; } = default!;
    public string Airline { get; set; } = default!; // "中国国际航空"
    public string AirlineCode { get; set; } = default!; // "CA"

    public DepartureInfoVo Departure { get; set; } = default!;
    public ArrivalInfoVo Arrival { get; set; } = default!;

    public string Duration { get; set; } = default!; // "2小时30分钟"
    public decimal Price { get; set; }
    public string PriceDisplay { get; set; } = default!; // "¥1,299"
    public int AvailableSeats { get; set; }
    public string Aircraft { get; set; } = default!; // "波音737-800"

    public bool IsDirectFlight { get; set; } = true;
    public string[] Amenities { get; set; } = Array.Empty<string>(); // ["WiFi", "餐食", "娱乐系统"]
}

/// <summary>
/// 出发信息视图模型
/// </summary>
public class DepartureInfoVo
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
/// 到达信息视图模型
/// </summary>
public class ArrivalInfoVo
{
    public string AirportCode { get; set; } = default!; // "SHA"
    public string AirportName { get; set; } = default!; // "上海虹桥国际机场"
    public string City { get; set; } = default!; // "上海"
    public string Terminal { get; set; } = default!; // "T2"
    public DateTime Time { get; set; }
    public string TimeDisplay { get; set; } = default!; // "11:00"
    public string DateDisplay { get; set; } = default!; // "3月15日 周五"
}
