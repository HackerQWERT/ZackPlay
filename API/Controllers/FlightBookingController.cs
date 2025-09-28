using Microsoft.AspNetCore.Mvc;
using Application.Contracts;
using Application.FlightBooking;
using Mapster;
using Domain.FlightBooking.ValueObjects;

namespace ZackPlay.API.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class FlightBookingController : ControllerBase
{
    private readonly FlightBookingService _service;

    public FlightBookingController(FlightBookingService service)
    {
        _service = service;
    }

    /// <summary>
    /// 获取可用机场列表
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AirportResponse>>> Airports()
    {
        var airports = await _service.GetActiveAirportsAsync();
        var result = airports.Adapt<IEnumerable<AirportResponse>>();
        return Ok(result);
    }


    /// <summary>
    /// 搜索航班（复杂请求体）
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<IEnumerable<FlightSearchResponse>>> Search([FromBody] FlightSearchRequest request)
    {
        if (!DateTime.TryParse(request.DepartureDate, out var departureDate))
            return BadRequest("DepartureDate 格式不正确");

        var flights = await _service.SearchFlightsAsync(request.From, request.To, departureDate);
        var result = flights.Adapt<IEnumerable<FlightSearchResponse>>();
        return Ok(result);
    }

    /// <summary>
    /// 创建预订（包含乘客信息）
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<FlightBookingResponse>> Create([FromBody] CreateFlightBookingRequest request)
    {
        if (request.Passenger is null)
            return BadRequest("缺少乘客信息");

        var booking = await _service.CreateBookingAsync(request);
        var response = booking.Adapt<FlightBookingResponse>();

        return CreatedAtAction(nameof(GetByReference), new { reference = booking.BookingReference }, response);
    }

    /// <summary>
    /// 通过预订参考号查询预订
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<FlightBookingResponse>> GetByReference([FromQuery] string reference)
    {
        if (string.IsNullOrWhiteSpace(reference))
            return BadRequest("reference 不能为空");

        var booking = await _service.GetBookingAsync(reference);
        if (booking is null) return NotFound();

        var response = booking.Adapt<FlightBookingResponse>();
        return Ok(response);
    }

    /// <summary>
    /// 确认预订
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<FlightBookingResponse>> Confirm([FromQuery] string reference)
    {
        if (string.IsNullOrWhiteSpace(reference))
            return BadRequest("reference 不能为空");

        var booking = await _service.ConfirmBookingAsync(reference);
        var response = booking.Adapt<FlightBookingResponse>();
        return Ok(response);
    }

    /// <summary>
    /// 取消预订
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<FlightBookingResponse>> Cancel([FromQuery] string reference, [FromQuery] string? reason = null)
    {
        if (string.IsNullOrWhiteSpace(reference))
            return BadRequest("reference 不能为空");

        var booking = await _service.CancelBookingAsync(reference);
        var response = booking.Adapt<FlightBookingResponse>();
        return Ok(response);
    }

    /// <summary>
    /// 查询某乘客的所有预订
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<FlightBookingResponse>>> PassengerBookings([FromQuery] Guid passengerId)
    {
        if (passengerId == Guid.Empty) return BadRequest("passengerId 不能为空");

        var bookings = await _service.GetPassengerBookingsAsync(passengerId);
        var result = bookings.Adapt<IEnumerable<FlightBookingResponse>>();
        return Ok(result);
    }

}

