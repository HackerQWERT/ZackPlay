using Domain.FlightBooking.Entities;
using Domain.FlightBooking.Repositories;
using Domain.FlightBooking.ValueObjects;

namespace Domain.FlightBooking.Services;

public sealed class BookingDomainService
{
    private readonly IFlightRepository _flightRepository;
    private readonly IPassengerRepository _passengerRepository;
    private readonly IFlightBookingRepository _bookingRepository;

    public BookingDomainService(
        IFlightRepository flightRepository,
        IPassengerRepository passengerRepository,
        IFlightBookingRepository bookingRepository)
    {
        _flightRepository = flightRepository;
        _passengerRepository = passengerRepository;
        _bookingRepository = bookingRepository;
    }

    public async Task<Domain.FlightBooking.Entities.FlightBooking> CreateBookingAsync(BookingCreationOptions options)
    {
        // 1) 校验航班存在
        var flight = await _flightRepository.GetByIdAsync(options.FlightId)
            ?? throw new InvalidOperationException("Flight not found");

        // 2) 校验座位是否足够（简化）
        var currentBookings = await _bookingRepository.GetBookingCountByFlightAsync(options.FlightId);
        if (currentBookings + options.SeatsCount > flight.TotalSeats)
            throw new InvalidOperationException("Not enough available seats");

        // 3) 查找或创建乘客
        var passengerInfo = options.Passenger;
        var passenger = await _passengerRepository.GetByPassportAsync(passengerInfo.PassportNumber);
        if (passenger is null)
        {
            passenger = new Domain.FlightBooking.Entities.Passenger(
                passengerInfo.FirstName,
                passengerInfo.LastName,
                passengerInfo.DateOfBirth,
                passengerInfo.Gender,
                passengerInfo.PassportNumber,
                passengerInfo.PassportCountry,
                passengerInfo.PassportExpiryDate,
                passengerInfo.Nationality,
                passengerInfo.Email,
                passengerInfo.PhoneNumber
            );
            await _passengerRepository.AddAsync(passenger);
        }

        // 4) 创建预订（聚合根内封装状态变更/事件发布 – 此处简化）
        var booking = new Domain.FlightBooking.Entities.FlightBooking(
            GenerateBookingReference(),
            options.FlightId,
            passenger.Id,
            options.SeatsCount,
            options.CabinClass,
            flight.BasePrice // 简化价格
        );

        await _bookingRepository.AddAsync(booking);
        return booking;
    }

    private static string GenerateBookingReference()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 6)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
