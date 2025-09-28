namespace Application.Integration.Queues;

public static class FlightBookingQueues
{
    public const string BookingCreated = "flight-booking-created";
    public const string BookingConfirmed = "flight-booking-confirmed";
    public const string BookingCancelled = "flight-booking-cancelled";
    public const string BookingPaid = "flight-booking-paid";
    public const string BookingRefunded = "flight-booking-refunded";
    public const string BookingCheckedIn = "flight-booking-checked-in";
    public const string DefaultDomainQueue = "domain-events";
}
