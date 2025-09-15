using Domain.FlightBooking.Entities;
using Domain.FlightBooking.Repositories;
using Domain.FlightBooking.ValueObjects;
using Domain.FlightBooking.Services;

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
    private readonly IBookingDomainService _bookingDomainService;

    public FlightBookingService(
        IAirportRepository airportRepository,
        IFlightRepository flightRepository,
        IPassengerRepository passengerRepository,
        IFlightBookingRepository bookingRepository,
        IBookingDomainService bookingDomainService)
    {
        _airportRepository = airportRepository;
        _flightRepository = flightRepository;
        _passengerRepository = passengerRepository;
        _bookingRepository = bookingRepository;
        _bookingDomainService = bookingDomainService;
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
        var command = new CreateBookingCommand(
            request.FlightId,
            request.PassengerFirstName,
            request.PassengerLastName,
            request.PassengerEmail,
            request.PassportNumber,
            request.DateOfBirth,
            request.Nationality,
            request.SeatsCount,
            request.CabinClass
        );

        var booking = await _bookingDomainService.CreateBookingAsync(command);
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
        // 保留方法以兼容已有签名（不再使用）
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 6)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
