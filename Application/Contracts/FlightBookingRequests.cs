namespace Application.Contracts;

/// <summary>
/// 航班搜索请求
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
/// 航班预订请求
/// </summary>
public class FlightBookingRequest
{
    public Guid FlightId { get; set; }
    public Guid PassengerId { get; set; }
    public int SeatsCount { get; set; } = 1;
    public string CabinClass { get; set; } = "Economy";
    public string SpecialRequests { get; set; } = string.Empty;
}

/// <summary>
/// 创建航班预订请求（包含乘客信息）
/// </summary>
public class CreateFlightBookingRequest
{
    public Guid FlightId { get; set; }
    public CreatePassengerRequest Passenger { get; set; } = default!;
    public int SeatsCount { get; set; } = 1;
    public string CabinClass { get; set; } = "Economy";
    public string SpecialRequests { get; set; } = string.Empty;
}

/// <summary>
/// 乘客创建请求
/// </summary>
public class CreatePassengerRequest
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = default!; // "Male", "Female"
    public string PassportNumber { get; set; } = default!;
    public string PassportCountry { get; set; } = default!;
    public DateTime PassportExpiryDate { get; set; }
    public string Nationality { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;
}

/// <summary>
/// 创建机场请求
/// </summary>
public class CreateAirportRequest
{
    public string Code { get; set; } = default!; // 机场三字码，如 PEK
    public string Name { get; set; } = default!; // 机场名称
    public string City { get; set; } = default!; // 所在城市
    public string Country { get; set; } = default!; // 所在国家
    public string TimeZone { get; set; } = default!; // 时区，如 "Asia/Shanghai"
}

/// <summary>
/// 创建航班请求
/// </summary>
public class CreateFlightRequest
{
    public string FlightNumber { get; set; } = default!; // 航班号，如 CZ3456
    public string AirlineCode { get; set; } = default!; // 航空公司代码，如 CZ
    public string AirlineName { get; set; } = default!; // 航空公司名称
    public string DepartureAirportCode { get; set; } = default!; // 出发机场代码
    public DateTime DepartureTime { get; set; } // 出发时间
    public string DepartureTerminal { get; set; } = default!; // 出发航站楼
    public string ArrivalAirportCode { get; set; } = default!; // 到达机场代码
    public DateTime ArrivalTime { get; set; } // 到达时间
    public string ArrivalTerminal { get; set; } = default!; // 到达航站楼
    public string AircraftType { get; set; } = default!; // 机型，如 A320
    public int TotalSeats { get; set; } // 总座位数
    public decimal BasePrice { get; set; } // 基础价格
}

/// <summary>
/// cancel 航班预订请求
/// </summary>
/// <remarks>
/// 包含取消原因等信息
/// </remarks>
public class CancelFlightBookingRequest
{
    public string Reason { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;

}