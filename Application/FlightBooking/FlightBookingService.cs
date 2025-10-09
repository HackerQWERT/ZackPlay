using Application.Contracts;
using Application.Integration.Queues;
using Domain.Abstractions;
using Domain.FlightBooking.Entities;
using Domain.FlightBooking.Events;
using Domain.FlightBooking.Repositories;
using Domain.FlightBooking.Services;
using Domain.FlightBooking.ValueObjects;

namespace Application.FlightBooking;

public class FlightBookingService
{
    private readonly IAirportRepository _airportRepository;
    private readonly IFlightRepository _flightRepository;
    private readonly IPassengerRepository _passengerRepository; // 领域服务内部可能用到
    private readonly IFlightBookingRepository _bookingRepository;
    private readonly BookingDomainService _bookingDomainService;
    private readonly IMessageQueueService _messageQueueService;

    public FlightBookingService(
        IAirportRepository airportRepository,
        IFlightRepository flightRepository,
        IPassengerRepository passengerRepository,
        IFlightBookingRepository bookingRepository,
        BookingDomainService bookingDomainService,
        IMessageQueueService messageQueueService)
    {
        _airportRepository = airportRepository;
        _flightRepository = flightRepository;
        _passengerRepository = passengerRepository;
        _bookingRepository = bookingRepository;
        _bookingDomainService = bookingDomainService;
        _messageQueueService = messageQueueService;
    }

    public async Task<IEnumerable<Airport>> GetActiveAirportsAsync() => await _airportRepository.GetActivateAirportsAsync();

    public async Task AddAirportAsync(CreateAirportRequest request)
    {
        var airport = new Airport(request.Code, request.Name, request.City, request.Country, request.TimeZone);
        await _airportRepository.AddAsync(airport);
    }

    public async Task AddFlightAsync(CreateFlightRequest request)
    {
        var flight = new Flight(
            request.FlightNumber,
            request.AirlineCode,
            request.AirlineName,
            request.DepartureAirportCode,
            request.DepartureTime,
            request.DepartureTerminal,
            request.ArrivalAirportCode,
            request.ArrivalTime,
            request.ArrivalTerminal,
            request.AircraftType,
            request.TotalSeats,
            request.BasePrice);
        await _flightRepository.AddAsync(flight);
    }

    public async Task<IEnumerable<Flight>> SearchFlightsAsync(string departureAirport, string arrivalAirport, DateTime departureDate)
        => await _flightRepository.SearchAsync(departureAirport, arrivalAirport, departureDate);

    public async Task<Domain.FlightBooking.Entities.FlightBooking> CreateBookingAsync(CreateFlightBookingRequest request)
    {
        if (request.Passenger is null)
            throw new ArgumentException("Passenger 信息不能为空", nameof(request));

        var cabin = Enum.TryParse<CabinClass>(request.CabinClass, true, out var parsed) ? parsed : CabinClass.Economy;

        var gender = Enum.TryParse<Gender>(request.Passenger.Gender, true, out var genderParsed) ? genderParsed : Gender.Other;

        var passengerProfile = new PassengerProfile(
            request.Passenger.FirstName,
            request.Passenger.LastName,
            request.Passenger.Email,
            request.Passenger.PassportNumber,
            request.Passenger.DateOfBirth,
            request.Passenger.Nationality,
            gender,
            request.Passenger.PassportCountry,
            request.Passenger.PassportExpiryDate,
            request.Passenger.PhoneNumber);

        var options = new BookingCreationOptions(request.FlightId, passengerProfile, request.SeatsCount, cabin);

        var booking = await _bookingDomainService.CreateBookingAsync(options);
        await PublishDomainEventsAsync(booking);
        return booking;
    }

    public async Task<Domain.FlightBooking.Entities.FlightBooking> ConfirmBookingAsync(string bookingReference)
    {
        var booking = await _bookingRepository.GetByReferenceAsync(bookingReference) ?? throw new InvalidOperationException("Booking not found");
        booking.Confirm();
        await _bookingRepository.UpdateAsync(booking);
        await PublishDomainEventsAsync(booking);
        return booking;
    }

    public async Task<Domain.FlightBooking.Entities.FlightBooking> CancelBookingAsync(string bookingReference)
    {
        var booking = await _bookingRepository.GetByReferenceAsync(bookingReference) ?? throw new InvalidOperationException("Booking not found");
        booking.Cancel("User requested cancellation");
        await _bookingRepository.UpdateAsync(booking);
        await PublishDomainEventsAsync(booking);
        return booking;
    }

    public async Task<Domain.FlightBooking.Entities.FlightBooking?> GetBookingAsync(string bookingReference) => await _bookingRepository.GetByReferenceAsync(bookingReference);

    public async Task<IEnumerable<Domain.FlightBooking.Entities.FlightBooking>> GetPassengerBookingsAsync(Guid passengerId)
        => await _bookingRepository.GetByPassengerAsync(passengerId);

    private async Task PublishDomainEventsAsync(IAggregateRoot aggregate)
    {
        if (aggregate.DomainEvents?.Any() == true)
        {
            foreach (var domainEvent in aggregate.DomainEvents)
            {
                var queueName = ResolveQueueName(domainEvent);
                await _messageQueueService.PublishAsync(queueName, domainEvent);
            }
            aggregate.ClearDomainEvents();
        }
    }

    private static string ResolveQueueName(IDomainEvent domainEvent) => domainEvent switch
    {
        FlightBookingCreatedEvent => FlightBookingQueues.BookingCreated,
        FlightBookingConfirmedEvent => FlightBookingQueues.BookingConfirmed,
        FlightBookingCancelledEvent => FlightBookingQueues.BookingCancelled,
        FlightBookingPaidEvent => FlightBookingQueues.BookingPaid,
        FlightBookingRefundedEvent => FlightBookingQueues.BookingRefunded,
        FlightBookingCheckedInEvent => FlightBookingQueues.BookingCheckedIn,
        _ => FlightBookingQueues.DefaultDomainQueue
    };
}
