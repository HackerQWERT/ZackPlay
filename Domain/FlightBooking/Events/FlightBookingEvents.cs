using Domain.Abstractions;

namespace Domain.FlightBooking.Events;

// 机票预订相关事件

/// <summary>
/// 订单提交请求事件 (提交订单) - 仅由提交入口（Producer）发布，Consumer 收到后真正执行业务创建。
/// </summary>
public record FlightBookingSubmitedEvent(
    Guid RequestId,
    Guid FlightId,
    string PassengerFirstName,
    string PassengerLastName,
    string PassengerEmail,
    string PassportNumber,
    DateTime DateOfBirth,
    string Nationality,
    int SeatsCount,
    string CabinClass,
    DateTime RequestTime) : DomainEventBase(DateTime.UtcNow);

/// <summary>
/// 创建订单完成事件 - 当订单创建成功完成时发布
/// </summary>
public record FlightBookingCreatedEvent(
    Guid BookingId,
    string BookingReference,
    Guid FlightId,
    Guid PassengerId,
    decimal TotalAmount) : DomainEventBase(DateTime.UtcNow);


public record FlightBookingConfirmedEvent(
    Guid BookingId,
    string BookingReference) : DomainEventBase(DateTime.UtcNow);

public record FlightBookingCancelledEvent(
    Guid BookingId,
    string BookingReference,
    string Reason,
    decimal RefundAmount) : DomainEventBase(DateTime.UtcNow);

public record FlightBookingPaidEvent(
    Guid BookingId,
    string BookingReference,
    decimal Amount,
    string PaymentReference) : DomainEventBase(DateTime.UtcNow);

public record FlightBookingRefundedEvent(
    Guid BookingId,
    string BookingReference,
    decimal RefundAmount) : DomainEventBase(DateTime.UtcNow);

public record FlightBookingCheckedInEvent(
    Guid BookingId,
    string BookingReference) : DomainEventBase(DateTime.UtcNow);

// 航班相关事件
public record FlightCreatedEvent(
    Guid FlightId,
    string FlightNumber,
    string DepartureAirport,
    string ArrivalAirport,
    DateTime DepartureTime) : DomainEventBase(DateTime.UtcNow);

public record FlightStatusChangedEvent(
    Guid FlightId,
    string FlightNumber,
    string OldStatus,
    string NewStatus) : DomainEventBase(DateTime.UtcNow);

public record FlightDelayedEvent(
    Guid FlightId,
    string FlightNumber,
    DateTime OriginalDepartureTime,
    DateTime NewDepartureTime,
    string Reason) : DomainEventBase(DateTime.UtcNow);

public record FlightCancelledEvent(
    Guid FlightId,
    string FlightNumber,
    string Reason) : DomainEventBase(DateTime.UtcNow);

// 乘客相关事件
public record PassengerRegisteredEvent(
    Guid PassengerId,
    string FullName,
    string Email,
    string PassportNumber) : DomainEventBase(DateTime.UtcNow);

public record PassengerContactUpdatedEvent(
    Guid PassengerId,
    string Email,
    string PhoneNumber) : DomainEventBase(DateTime.UtcNow);
