using Domain.Abstractions;

namespace Domain.FlightBooking.Events;

// 机票预订相关事件
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
