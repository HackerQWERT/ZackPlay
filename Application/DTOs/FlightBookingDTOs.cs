namespace Application.DTOs;

/// <summary>
/// 机场数据传输对象
/// 用于应用层内部数据传输
/// </summary>
public class AirportDto
{
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string City { get; set; } = default!;
    public string Country { get; set; } = default!;
    public string TimeZone { get; set; } = default!;
    public bool IsActive { get; set; }
}

/// <summary>
/// 航班搜索数据传输对象
/// </summary>
public class FlightSearchDto
{
    public string DepartureAirport { get; set; } = default!;
    public string ArrivalAirport { get; set; } = default!;
    public DateTime DepartureDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public int AdultCount { get; set; } = 1;
    public int ChildCount { get; set; } = 0;
    public int InfantCount { get; set; } = 0;
    public string CabinClass { get; set; } = "Economy";
}

/// <summary>
/// 航班信息数据传输对象
/// </summary>
public class FlightInfoDto
{
    public Guid Id { get; set; }
    public string FlightNumber { get; set; } = default!;
    public string AirlineCode { get; set; } = default!;
    public string AirlineName { get; set; } = default!;
    public string DepartureAirportCode { get; set; } = default!;
    public string ArrivalAirportCode { get; set; } = default!;
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public string DepartureTerminal { get; set; } = default!;
    public string ArrivalTerminal { get; set; } = default!;
    public decimal BasePrice { get; set; }
    public int AvailableSeats { get; set; }
    public string AircraftType { get; set; } = default!;
    public string Duration { get; set; } = default!; // 计算得出的飞行时长
}
