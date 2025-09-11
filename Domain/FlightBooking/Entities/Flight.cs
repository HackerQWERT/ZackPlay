using Domain.Abstractions;
using Domain.FlightBooking.ValueObjects;

namespace Domain.FlightBooking.Entities;

/// <summary>
/// 航班实体
/// </summary>
public class Flight
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string FlightNumber { get; private set; } = default!; // 航班号，如 CZ3456
    public string AirlineCode { get; private set; } = default!; // 航空公司代码，如 CZ
    public string AirlineName { get; private set; } = default!; // 航空公司名称

    // 出发信息
    public string DepartureAirportCode { get; private set; } = default!;
    public DateTime DepartureTime { get; private set; }
    public string DepartureTerminal { get; private set; } = default!;

    // 到达信息
    public string ArrivalAirportCode { get; private set; } = default!;
    public DateTime ArrivalTime { get; private set; }
    public string ArrivalTerminal { get; private set; } = default!;

    // 航班信息
    public string AircraftType { get; private set; } = default!; // 机型，如 A320
    public int TotalSeats { get; private set; } // 总座位数
    public int AvailableSeats { get; private set; } // 可用座位数
    public decimal BasePrice { get; private set; } // 基础价格

    // 状态
    public FlightStatus Status { get; private set; } = FlightStatus.Scheduled;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; private set; }

    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    private Flight() { } // EF Core

    public Flight(
        string flightNumber,
        string airlineCode,
        string airlineName,
        string departureAirportCode,
        DateTime departureTime,
        string departureTerminal,
        string arrivalAirportCode,
        DateTime arrivalTime,
        string arrivalTerminal,
        string aircraftType,
        int totalSeats,
        decimal basePrice)
    {
        ValidateInput(flightNumber, airlineCode, airlineName, departureAirportCode,
                     arrivalAirportCode, aircraftType, totalSeats, basePrice);

        if (arrivalTime <= departureTime)
            throw new ArgumentException("到达时间必须晚于出发时间");

        FlightNumber = flightNumber.ToUpper();
        AirlineCode = airlineCode.ToUpper();
        AirlineName = airlineName;
        DepartureAirportCode = departureAirportCode.ToUpper();
        DepartureTime = departureTime;
        DepartureTerminal = departureTerminal;
        ArrivalAirportCode = arrivalAirportCode.ToUpper();
        ArrivalTime = arrivalTime;
        ArrivalTerminal = arrivalTerminal;
        AircraftType = aircraftType;
        TotalSeats = totalSeats;
        AvailableSeats = totalSeats;
        BasePrice = basePrice;
    }

    public bool CanBook(int requestedSeats)
    {
        return Status == FlightStatus.Scheduled && AvailableSeats >= requestedSeats;
    }

    public void BookSeats(int seatsCount)
    {
        if (!CanBook(seatsCount))
            throw new InvalidOperationException($"无法预订 {seatsCount} 个座位");

        AvailableSeats -= seatsCount;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ReleaseSeats(int seatsCount)
    {
        if (AvailableSeats + seatsCount > TotalSeats)
            throw new InvalidOperationException("释放座位数超过总座位数");

        AvailableSeats += seatsCount;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateStatus(FlightStatus newStatus)
    {
        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice < 0) throw new ArgumentException("价格不能为负数");
        BasePrice = newPrice;
        UpdatedAt = DateTime.UtcNow;
    }

    public TimeSpan FlightDuration => ArrivalTime - DepartureTime;

    private void ValidateInput(string flightNumber, string airlineCode, string airlineName,
                              string departureAirportCode, string arrivalAirportCode,
                              string aircraftType, int totalSeats, decimal basePrice)
    {
        if (string.IsNullOrWhiteSpace(flightNumber)) throw new ArgumentException("航班号不能为空");
        if (string.IsNullOrWhiteSpace(airlineCode)) throw new ArgumentException("航空公司代码不能为空");
        if (string.IsNullOrWhiteSpace(airlineName)) throw new ArgumentException("航空公司名称不能为空");
        if (string.IsNullOrWhiteSpace(departureAirportCode)) throw new ArgumentException("出发机场代码不能为空");
        if (string.IsNullOrWhiteSpace(arrivalAirportCode)) throw new ArgumentException("到达机场代码不能为空");
        if (string.IsNullOrWhiteSpace(aircraftType)) throw new ArgumentException("机型不能为空");
        if (totalSeats <= 0) throw new ArgumentException("座位数必须大于0");
        if (basePrice < 0) throw new ArgumentException("价格不能为负数");
        if (departureAirportCode.Equals(arrivalAirportCode, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("出发和到达机场不能相同");
    }

    public void ClearDomainEvents() => _domainEvents.Clear();
    private void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
}
