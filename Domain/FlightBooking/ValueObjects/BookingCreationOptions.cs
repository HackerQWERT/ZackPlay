using System;

namespace Domain.FlightBooking.ValueObjects;

/// <summary>
/// 机票预订创建参数（值对象）
/// </summary>
public readonly record struct BookingCreationOptions
{
    public Guid FlightId { get; }
    public PassengerProfile Passenger { get; }
    public int SeatsCount { get; }
    public CabinClass CabinClass { get; }

    public BookingCreationOptions(Guid flightId, PassengerProfile passenger, int seatsCount, CabinClass cabinClass)
    {
        if (flightId == Guid.Empty) throw new ArgumentException("航班ID不能为空", nameof(flightId));
        if (passenger == default) throw new ArgumentException("乘客信息不能为空", nameof(passenger));
        if (seatsCount <= 0) throw new ArgumentException("座位数必须大于0", nameof(seatsCount));

        FlightId = flightId;
        Passenger = passenger;
        SeatsCount = seatsCount;
        CabinClass = cabinClass;
    }
}
