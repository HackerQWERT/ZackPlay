using Microsoft.AspNetCore.Mvc;
using Application.Contracts;
using Application.FlightBooking;

namespace ZackPlay.API.Controllers;

[ApiController]
[Route("api/admin/[action]")]
public class FlightBookingAdminController : ControllerBase
{
    private readonly FlightBookingService _service;

    public FlightBookingAdminController(FlightBookingService service)
    {
        _service = service;
    }

    /// <summary>
    /// 添加新机场（管理员接口）
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> AddAirport([FromBody] CreateAirportRequest request)
    {
        if (request == null)
            return BadRequest("请求体不能为空");

        await _service.AddAirportAsync(request);
        return Ok(new { Message = "机场添加成功" });
    }

    /// <summary>
    /// 上传新航班（管理员接口）
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> UploadFlight([FromBody] CreateFlightRequest request)
    {
        if (request == null)
            return BadRequest("请求体不能为空");

        await _service.AddFlightAsync(request);
        return Ok(new { Message = "航班上传成功" });
    }
}