using Domain.FlightBooking.Entities;
using Domain.FlightBooking.Repositories;
using Domain.FlightBooking.ValueObjects;

namespace Application.FlightBooking;

public interface IFlightBookingService
{
    Task<IEnumerable<Airport>> GetActiveAirportsAsync();
    Task<IEnumerable<Flight>> SearchFlightsAsync(string departureAirport, string arrivalAirport, DateTime departureDate);
    Task<Domain.FlightBooking.Entities.FlightBooking> CreateBookingAsync(CreateBookingRequest request);
    Task<Domain.FlightBooking.Entities.FlightBooking?> GetBookingAsync(string bookingReference);
    Task<Domain.FlightBooking.Entities.FlightBooking> ConfirmBookingAsync(string bookingReference);
    Task<Domain.FlightBooking.Entities.FlightBooking> CancelBookingAsync(string bookingReference);
    Task<IEnumerable<Domain.FlightBooking.Entities.FlightBooking>> GetPassengerBookingsAsync(Guid passengerId);
}

public record CreateBookingRequest(
    Guid FlightId,
    string PassengerFirstName,
    string PassengerLastName,
    string PassengerEmail,
    string PassportNumber,
    DateTime DateOfBirth,
    string Nationality,
    int SeatsCount,
    CabinClass CabinClass
);

public class FlightBookingService : IFlightBookingService
{
    private readonly IAirportRepository _airportRepository;
    private readonly IFlightRepository _flightRepository;
    private readonly IPassengerRepository _passengerRepository;
    private readonly IFlightBookingRepository _bookingRepository;

    public FlightBookingService(
        IAirportRepository airportRepository,
        IFlightRepository flightRepository,
        IPassengerRepository passengerRepository,
        IFlightBookingRepository bookingRepository)
    {
        _airportRepository = airportRepository;
        _flightRepository = flightRepository;
        _passengerRepository = passengerRepository;
        _bookingRepository = bookingRepository;
    }

    public async Task<IEnumerable<Airport>> GetActiveAirportsAsync()
    {
        return await _airportRepository.GetActivateAirportsAsync();
    }

    public async Task<IEnumerable<Flight>> SearchFlightsAsync(string departureAirport, string arrivalAirport, DateTime departureDate)
    {
        return await _flightRepository.SearchAsync(departureAirport, arrivalAirport, departureDate);
    }

    public async Task<Domain.FlightBooking.Entities.FlightBooking> CreateBookingAsync(CreateBookingRequest request)
    {
        // 检查航班是否存在
        var flight = await _flightRepository.GetByIdAsync(request.FlightId);
        if (flight == null)
            throw new InvalidOperationException("Flight not found");

        // 检查是否有足够的座位 (简化检查)
        var currentBookings = await _bookingRepository.GetBookingCountByFlightAsync(request.FlightId);
        if (currentBookings + request.SeatsCount > flight.TotalSeats)
            throw new InvalidOperationException("Not enough available seats");

        // 查找或创建乘客
        var passenger = await _passengerRepository.GetByPassportAsync(request.PassportNumber);
        if (passenger == null)
        {
            passenger = new Passenger(
                request.PassengerFirstName,
                request.PassengerLastName,
                request.DateOfBirth,
                Gender.Other, // 默认值，应该从请求中获取
                request.PassportNumber,
                request.Nationality, // 护照国家，简化处理
                DateTime.UtcNow.AddYears(10), // 默认护照有效期，应该从请求中获取
                request.Nationality,
                request.PassengerEmail,
                "" // 电话号码，应该从请求中获取
            );
            await _passengerRepository.AddAsync(passenger);
        }

        // 创建预订
        var booking = new Domain.FlightBooking.Entities.FlightBooking(
            GenerateBookingReference(),
            request.FlightId,
            passenger.Id,
            request.SeatsCount,
            request.CabinClass,
            flight.BasePrice // 简化价格计算
        );

        await _bookingRepository.AddAsync(booking);
        return booking;
    }

    public async Task<Domain.FlightBooking.Entities.FlightBooking?> GetBookingAsync(string bookingReference)
    {
        return await _bookingRepository.GetByReferenceAsync(bookingReference);
    }

    public async Task<Domain.FlightBooking.Entities.FlightBooking> ConfirmBookingAsync(string bookingReference)
    {
        var booking = await _bookingRepository.GetByReferenceAsync(bookingReference);
        if (booking == null)
            throw new InvalidOperationException("Booking not found");

        booking.Confirm();
        await _bookingRepository.UpdateAsync(booking);
        return booking;
    }

    public async Task<Domain.FlightBooking.Entities.FlightBooking> CancelBookingAsync(string bookingReference)
    {
        var booking = await _bookingRepository.GetByReferenceAsync(bookingReference);
        if (booking == null)
            throw new InvalidOperationException("Booking not found");

        booking.Cancel("User requested cancellation");
        await _bookingRepository.UpdateAsync(booking);
        return booking;
    }

    public async Task<IEnumerable<Domain.FlightBooking.Entities.FlightBooking>> GetPassengerBookingsAsync(Guid passengerId)
    {
        return await _bookingRepository.GetByPassengerAsync(passengerId);
    }

    private string GenerateBookingReference()
    {
        // 生成6位随机字母数字组合
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 6)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
