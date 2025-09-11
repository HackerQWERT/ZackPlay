namespace Application.DTOs.Requests;

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
