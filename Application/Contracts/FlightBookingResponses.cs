namespace Application.Contracts;

/// <summary>
/// 机场响应模型
/// </summary>
public class AirportResponse
{
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string DisplayName { get; set; } = default!; // "北京首都国际机场 (PEK)"
    public string City { get; set; } = default!;
    public string Country { get; set; } = default!;
    public string Location { get; set; } = default!; // "北京, 中国"
    public string TimeZone { get; set; } = default!;
}

/// <summary>
/// 航班搜索结果响应
/// </summary>
public class FlightSearchResponse
{
    public Guid Id { get; set; }
    public string FlightNumber { get; set; } = default!;
    public string Airline { get; set; } = default!; // "中国国际航空"
    public string AirlineCode { get; set; } = default!; // "CA"

    public DepartureInfo Departure { get; set; } = default!;
    public ArrivalInfo Arrival { get; set; } = default!;

    public string Duration { get; set; } = default!; // "2小时30分钟"
    public decimal Price { get; set; }
    public string PriceDisplay { get; set; } = default!; // "¥1,299"
    public int AvailableSeats { get; set; }
    public string Aircraft { get; set; } = default!; // "波音737-800"

    public bool IsDirectFlight { get; set; } = true;
    public string[] Amenities { get; set; } = Array.Empty<string>(); // ["WiFi", "餐食", "娱乐系统"]
}

/// <summary>
/// 出发信息
/// </summary>
public class DepartureInfo
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
/// 到达信息
/// </summary>
public class ArrivalInfo
{
    public string AirportCode { get; set; } = default!; // "SHA"
    public string AirportName { get; set; } = default!; // "上海虹桥国际机场"
    public string City { get; set; } = default!; // "上海"
    public string Terminal { get; set; } = default!; // "T2"
    public DateTime Time { get; set; }
    public string TimeDisplay { get; set; } = default!; // "11:00"
    public string DateDisplay { get; set; } = default!; // "3月15日 周五"
}

/// <summary>
/// 航班预订响应
/// </summary>
public class FlightBookingResponse
{
    public Guid Id { get; set; }
    public string BookingReference { get; set; } = default!;
    public string Status { get; set; } = default!;
    public DateTime BookingTime { get; set; }
    public decimal TotalAmount { get; set; }
    public FlightInfo Flight { get; set; } = default!;
    public PassengerInfo Passenger { get; set; } = default!;
}

/// <summary>
/// 航班信息（用于预订响应）
/// </summary>
public class FlightInfo
{
    public Guid Id { get; set; }
    public string FlightNumber { get; set; } = default!;
    public string Airline { get; set; } = default!;
    public DepartureInfo Departure { get; set; } = default!;
    public ArrivalInfo Arrival { get; set; } = default!;
}

/// <summary>
/// 乘客信息（用于预订响应）
/// </summary>
public class PassengerInfo
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string FullName { get; set; } = default!; // "张 三"
    public string Email { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;
}
