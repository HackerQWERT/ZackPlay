using Application.FlightBooking;
using Domain.FlightBooking.ValueObjects;
using Microsoft.AspNetCore.Mvc;

namespace ZackPlay.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FlightBookingController : ControllerBase
{
    private readonly IFlightBookingService _flightBookingService;

    public FlightBookingController(IFlightBookingService flightBookingService)
    {
        _flightBookingService = flightBookingService;
    }

    [HttpGet("airports")]
    public async Task<IActionResult> GetAirports()
    {
        var airports = await _flightBookingService.GetActiveAirportsAsync();
        return Ok(airports.Select(a => new
        {
            a.Code,
            a.Name,
            a.City,
            a.Country
        }));
    }

    [HttpGet("flights/search")]
    public async Task<IActionResult> SearchFlights(
        [FromQuery] string departureAirport,
        [FromQuery] string arrivalAirport,
        [FromQuery] DateTime departureDate)
    {
        if (string.IsNullOrWhiteSpace(departureAirport) || string.IsNullOrWhiteSpace(arrivalAirport))
        {
            return BadRequest("Departure and arrival airports are required");
        }

        var flights = await _flightBookingService.SearchFlightsAsync(departureAirport, arrivalAirport, departureDate);
        return Ok(flights.Select(f => new
        {
            f.Id,
            f.FlightNumber,
            f.AirlineCode,
            f.DepartureAirportCode,
            f.ArrivalAirportCode,
            f.DepartureTime,
            f.ArrivalTime,
            f.TotalSeats,
            f.BasePrice,
            Status = f.Status.ToString()
        }));
    }

    [HttpPost("bookings")]
    public async Task<IActionResult> CreateBooking([FromBody] CreateBookingDto request)
    {
        try
        {
            var bookingRequest = new CreateBookingRequest(
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

            var booking = await _flightBookingService.CreateBookingAsync(bookingRequest);

            return Ok(new
            {
                booking.Id,
                booking.BookingReference,
                booking.FlightId,
                booking.PassengerId,
                booking.SeatsCount,
                CabinClass = booking.CabinClass.ToString(),
                booking.TotalAmount,
                booking.BookingTime,
                Status = booking.Status.ToString()
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("bookings/{bookingReference}")]
    public async Task<IActionResult> GetBooking(string bookingReference)
    {
        var booking = await _flightBookingService.GetBookingAsync(bookingReference);
        if (booking == null)
        {
            return NotFound();
        }

        return Ok(new
        {
            booking.Id,
            booking.BookingReference,
            booking.FlightId,
            booking.PassengerId,
            booking.SeatsCount,
            CabinClass = booking.CabinClass.ToString(),
            booking.TotalAmount,
            booking.BookingTime,
            Status = booking.Status.ToString()
        });
    }

    [HttpPost("bookings/{bookingReference}/confirm")]
    public async Task<IActionResult> ConfirmBooking(string bookingReference)
    {
        try
        {
            var booking = await _flightBookingService.ConfirmBookingAsync(bookingReference);
            return Ok(new
            {
                booking.Id,
                booking.BookingReference,
                Status = booking.Status.ToString(),
                Message = "Booking confirmed successfully"
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("bookings/{bookingReference}/cancel")]
    public async Task<IActionResult> CancelBooking(string bookingReference)
    {
        try
        {
            var booking = await _flightBookingService.CancelBookingAsync(bookingReference);
            return Ok(new
            {
                booking.Id,
                booking.BookingReference,
                Status = booking.Status.ToString(),
                Message = "Booking cancelled successfully"
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("passengers/{passengerId}/bookings")]
    public async Task<IActionResult> GetPassengerBookings(Guid passengerId)
    {
        var bookings = await _flightBookingService.GetPassengerBookingsAsync(passengerId);
        return Ok(bookings.Select(b => new
        {
            b.Id,
            b.BookingReference,
            b.FlightId,
            b.SeatsCount,
            CabinClass = b.CabinClass.ToString(),
            b.TotalAmount,
            b.BookingTime,
            Status = b.Status.ToString()
        }));
    }
}

public record CreateBookingDto(
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
