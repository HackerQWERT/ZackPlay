using Domain.FlightBooking.Entities;
using Domain.FlightBooking.ValueObjects;

namespace Infrastructure.Repositories.FlightBooking.Po;

/// <summary>
/// 机场持久化对象
/// </summary>
public class AirportPo
{
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string City { get; set; } = default!;
    public string Country { get; set; } = default!;
    public string TimeZone { get; set; } = default!;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Airport ToDomain()
    {
        var airport = new Airport(Code, Name, City, Country, TimeZone);
        if (!IsActive) airport.Deactivate();
        return airport;
    }

    public static AirportPo FromDomain(Airport airport)
    {
        return new AirportPo
        {
            Code = airport.Code,
            Name = airport.Name,
            City = airport.City,
            Country = airport.Country,
            TimeZone = airport.TimeZone,
            IsActive = airport.IsActive,
            CreatedAt = airport.CreatedAt,
            UpdatedAt = airport.UpdatedAt
        };
    }
}

/// <summary>
/// 航班持久化对象
/// </summary>
public class FlightPo
{
    public Guid Id { get; set; }
    public string FlightNumber { get; set; } = default!;
    public string AirlineCode { get; set; } = default!;
    public string AirlineName { get; set; } = default!;
    public string DepartureAirportCode { get; set; } = default!;
    public DateTime DepartureTime { get; set; }
    public string DepartureTerminal { get; set; } = default!;
    public string ArrivalAirportCode { get; set; } = default!;
    public DateTime ArrivalTime { get; set; }
    public string ArrivalTerminal { get; set; } = default!;
    public string AircraftType { get; set; } = default!;
    public int TotalSeats { get; set; }
    public int AvailableSeats { get; set; }
    public decimal BasePrice { get; set; }
    public int Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Flight ToDomain()
    {
        return Flight.Rehydrate(
            Id,
            FlightNumber,
            AirlineCode,
            AirlineName,
            DepartureAirportCode,
            DepartureTime,
            DepartureTerminal,
            ArrivalAirportCode,
            ArrivalTime,
            ArrivalTerminal,
            AircraftType,
            TotalSeats,
            AvailableSeats,
            BasePrice,
            (FlightStatus)Status,
            CreatedAt,
            UpdatedAt
        );
    }

    public static FlightPo FromDomain(Flight flight)
    {
        return new FlightPo
        {
            Id = flight.Id,
            FlightNumber = flight.FlightNumber,
            AirlineCode = flight.AirlineCode,
            AirlineName = flight.AirlineName,
            DepartureAirportCode = flight.DepartureAirportCode,
            DepartureTime = flight.DepartureTime,
            DepartureTerminal = flight.DepartureTerminal,
            ArrivalAirportCode = flight.ArrivalAirportCode,
            ArrivalTime = flight.ArrivalTime,
            ArrivalTerminal = flight.ArrivalTerminal,
            AircraftType = flight.AircraftType,
            TotalSeats = flight.TotalSeats,
            AvailableSeats = flight.AvailableSeats,
            BasePrice = flight.BasePrice,
            Status = (int)flight.Status,
            CreatedAt = flight.CreatedAt,
            UpdatedAt = flight.UpdatedAt
        };
    }
}

/// <summary>
/// 乘客持久化对象
/// </summary>
public class PassengerPo
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public DateTime DateOfBirth { get; set; }
    public int Gender { get; set; }
    public string PassportNumber { get; set; } = default!;
    public string PassportCountry { get; set; } = default!;
    public DateTime PassportExpiryDate { get; set; }
    public string Nationality { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;
    public int Type { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Passenger ToDomain()
    {
        return new Passenger(
            FirstName,
            LastName,
            DateOfBirth,
            (Gender)Gender,
            PassportNumber,
            PassportCountry,
            PassportExpiryDate,
            Nationality,
            Email,
            PhoneNumber
        );
    }

    public static PassengerPo FromDomain(Passenger passenger)
    {
        return new PassengerPo
        {
            Id = passenger.Id,
            FirstName = passenger.FirstName,
            LastName = passenger.LastName,
            DateOfBirth = passenger.DateOfBirth,
            Gender = (int)passenger.Gender,
            PassportNumber = passenger.PassportNumber,
            PassportCountry = passenger.PassportCountry,
            PassportExpiryDate = passenger.PassportExpiryDate,
            Nationality = passenger.Nationality,
            Email = passenger.Email,
            PhoneNumber = passenger.PhoneNumber,
            Type = (int)passenger.Type,
            CreatedAt = passenger.CreatedAt,
            UpdatedAt = passenger.UpdatedAt
        };
    }
}

/// <summary>
/// 机票预订持久化对象
/// </summary>
public class FlightBookingPo
{
    public Guid Id { get; set; }
    public string BookingReference { get; set; } = default!;
    public Guid FlightId { get; set; }
    public Guid PassengerId { get; set; }
    public int SeatsCount { get; set; }
    public int CabinClass { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalAmount { get; set; }
    public string SpecialRequests { get; set; } = string.Empty;
    public int Status { get; set; }
    public DateTime BookingTime { get; set; }
    public DateTime? ConfirmationTime { get; set; }
    public DateTime? CancellationTime { get; set; }
    public DateTime? CheckInTime { get; set; }
    public int PaymentStatus { get; set; }
    public DateTime? PaymentTime { get; set; }
    public string? PaymentReference { get; set; }
    public string? CancellationReason { get; set; }
    public decimal RefundAmount { get; set; }

    public Domain.FlightBooking.Entities.FlightBooking ToDomain()
    {
        var booking = new Domain.FlightBooking.Entities.FlightBooking(
            BookingReference,
            FlightId,
            PassengerId,
            SeatsCount,
            (CabinClass)CabinClass,
            UnitPrice,
            SpecialRequests
        );

        // 设置状态和时间（需要通过反射或添加内部方法）
        // 这里简化处理，实际项目中可能需要更复杂的状态恢复逻辑

        return booking;
    }

    public static FlightBookingPo FromDomain(Domain.FlightBooking.Entities.FlightBooking booking)
    {
        return new FlightBookingPo
        {
            Id = booking.Id,
            BookingReference = booking.BookingReference,
            FlightId = booking.FlightId,
            PassengerId = booking.PassengerId,
            SeatsCount = booking.SeatsCount,
            CabinClass = (int)booking.CabinClass,
            UnitPrice = booking.UnitPrice,
            TotalAmount = booking.TotalAmount,
            SpecialRequests = booking.SpecialRequests,
            Status = (int)booking.Status,
            BookingTime = booking.BookingTime,
            ConfirmationTime = booking.ConfirmationTime,
            CancellationTime = booking.CancellationTime,
            CheckInTime = booking.CheckInTime,
            PaymentStatus = (int)booking.PaymentStatus,
            PaymentTime = booking.PaymentTime,
            PaymentReference = booking.PaymentReference,
            CancellationReason = booking.CancellationReason,
            RefundAmount = booking.RefundAmount
        };
    }
}
