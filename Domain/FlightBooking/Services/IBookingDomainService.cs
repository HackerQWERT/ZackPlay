using Domain.FlightBooking.Entities;
using Domain.FlightBooking.Repositories;
using Domain.FlightBooking.ValueObjects;

namespace Domain.FlightBooking.Services;

/// <summary>
/// 机票预订领域服务（跨聚合：航班/乘客/预订）
/// </summary>
public interface IBookingDomainService
{
    Task<Domain.FlightBooking.Entities.FlightBooking> CreateBookingAsync(CreateBookingCommand command);
}

/// <summary>
/// 领域层命令对象，避免对 Application DTO 的依赖
/// </summary>
/// <param name="FlightId">航班 ID</param>
/// <param name="PassengerFirstName">乘客名</param>
/// <param name="PassengerLastName">乘客姓</param>
/// <param name="PassengerEmail">乘客邮箱</param>
/// <param name="PassportNumber">护照号</param>
/// <param name="DateOfBirth">出生日期</param>
/// <param name="Nationality">国籍</param>
/// <param name="SeatsCount">座位数</param>
/// <param name="CabinClass">舱位</param>
public record CreateBookingCommand(
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

public sealed class BookingDomainService : IBookingDomainService
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

    public async Task<Domain.FlightBooking.Entities.FlightBooking> CreateBookingAsync(CreateBookingCommand command)
    {
        // 1) 校验航班存在
        var flight = await _flightRepository.GetByIdAsync(command.FlightId)
            ?? throw new InvalidOperationException("Flight not found");

        // 2) 校验座位是否足够（简化）
        var currentBookings = await _bookingRepository.GetBookingCountByFlightAsync(command.FlightId);
        if (currentBookings + command.SeatsCount > flight.TotalSeats)
            throw new InvalidOperationException("Not enough available seats");

        // 3) 查找或创建乘客
        var passenger = await _passengerRepository.GetByPassportAsync(command.PassportNumber);
        if (passenger is null)
        {
            passenger = new Domain.FlightBooking.Entities.Passenger(
                command.PassengerFirstName,
                command.PassengerLastName,
                command.DateOfBirth,
                Gender.Other, // TODO: 从外部传入或根据业务推导
                command.PassportNumber,
                command.Nationality,
                DateTime.UtcNow.AddYears(10), // TODO: 从外部传入
                command.Nationality,
                command.PassengerEmail,
                string.Empty // TODO: 电话从外部传入
            );
            await _passengerRepository.AddAsync(passenger);
        }

        // 4) 创建预订（聚合根内封装状态变更/事件发布 – 此处简化）
        var booking = new Domain.FlightBooking.Entities.FlightBooking(
            GenerateBookingReference(),
            command.FlightId,
            passenger.Id,
            command.SeatsCount,
            command.CabinClass,
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
