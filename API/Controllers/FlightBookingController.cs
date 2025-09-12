using Microsoft.AspNetCore.Mvc;

namespace ZackPlay.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Tags("Flight Booking v1")]
public class FlightBookingV1Controller : ControllerBase
{
    /// <summary>
    /// Get all flight bookings - Version 1
    /// </summary>
    [HttpGet]
    public IActionResult GetFlightBookings()
    {
        return Ok(new
        {
            Version = "1.0",
            Message = "Flight bookings from API v1",
            Data = new[] { "Booking1", "Booking2" }
        });
    }

    /// <summary>
    /// Create a new flight booking - Version 1
    /// </summary>
    [HttpPost]
    public IActionResult CreateFlightBooking([FromBody] object booking)
    {
        return Ok(new
        {
            Version = "1.0",
            Message = "Booking created via API v1",
            BookingId = 123
        });
    }
}

[ApiController]
[Route("api/v2/[controller]")]
[Tags("Flight Booking v2")]
public class FlightBookingV2Controller : ControllerBase
{
    /// <summary>
    /// Get all flight bookings - Version 2 (Enhanced)
    /// </summary>
    [HttpGet]
    public IActionResult GetFlightBookings()
    {
        return Ok(new
        {
            Version = "2.0",
            Message = "Enhanced flight bookings from API v2",
            Data = new[]
            {
                new { Id = 1, BookingRef = "FB001", Status = "Confirmed", CreatedAt = DateTime.UtcNow.AddDays(-1) },
                new { Id = 2, BookingRef = "FB002", Status = "Pending", CreatedAt = DateTime.UtcNow.AddHours(-2) }
            },
            Meta = new
            {
                TotalCount = 2,
                ApiVersion = "2.0",
                LastUpdated = DateTime.UtcNow
            }
        });
    }

    /// <summary>
    /// Create a new flight booking - Version 2 (Enhanced)
    /// </summary>
    [HttpPost]
    public IActionResult CreateFlightBooking([FromBody] object booking)
    {
        return Ok(new
        {
            Version = "2.0",
            Message = "Enhanced booking created via API v2",
            BookingId = 456,
            Status = "Confirmed",
            Timestamp = DateTime.UtcNow,
            Tracking = new
            {
                Reference = "TRK-456",
                EstimatedProcessing = "2-3 business days"
            }
        });
    }

    /// <summary>
    /// Cancel a flight booking - New in Version 2
    /// </summary>
    [HttpDelete("{id}")]
    public IActionResult CancelFlightBooking(int id)
    {
        return Ok(new
        {
            Version = "2.0",
            Message = $"Booking {id} cancelled successfully",
            BookingId = id,
            Status = "Cancelled",
            Timestamp = DateTime.UtcNow,
            RefundInfo = new
            {
                RefundAmount = 150.00,
                RefundMethod = "Original payment method",
                ProcessingTime = "5-7 business days"
            }
        });
    }
}