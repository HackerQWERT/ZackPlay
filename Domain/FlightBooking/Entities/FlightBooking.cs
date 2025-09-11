using Domain.Abstractions;
using Domain.FlightBooking.ValueObjects;
using Domain.FlightBooking.Events;

namespace Domain.FlightBooking.Entities;

/// <summary>
/// 机票预订实体 - 聚合根
/// </summary>
public class FlightBooking : IAggregateRoot
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string BookingReference { get; private set; } = default!; // 预订参考号
    public Guid FlightId { get; private set; }
    public Guid PassengerId { get; private set; }

    // 预订详情
    public int SeatsCount { get; private set; }
    public CabinClass CabinClass { get; private set; }
    public decimal UnitPrice { get; private set; } // 单价
    public decimal TotalAmount { get; private set; } // 总金额
    public string SpecialRequests { get; private set; } = string.Empty; // 特殊要求

    // 状态和时间
    public BookingStatus Status { get; private set; } = BookingStatus.Pending;
    public DateTime BookingTime { get; private set; } = DateTime.UtcNow;
    public DateTime? ConfirmationTime { get; private set; }
    public DateTime? CancellationTime { get; private set; }
    public DateTime? CheckInTime { get; private set; }

    // 支付信息
    public PaymentStatus PaymentStatus { get; private set; } = PaymentStatus.Pending;
    public DateTime? PaymentTime { get; private set; }
    public string? PaymentReference { get; private set; }

    // 取消信息
    public string? CancellationReason { get; private set; }
    public decimal RefundAmount { get; private set; }

    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    private FlightBooking() { } // EF Core

    public FlightBooking(
        string bookingReference,
        Guid flightId,
        Guid passengerId,
        int seatsCount,
        CabinClass cabinClass,
        decimal unitPrice,
        string specialRequests = "")
    {
        ValidateInput(bookingReference, flightId, passengerId, seatsCount, unitPrice);

        BookingReference = bookingReference.ToUpper();
        FlightId = flightId;
        PassengerId = passengerId;
        SeatsCount = seatsCount;
        CabinClass = cabinClass;
        UnitPrice = unitPrice;
        TotalAmount = CalculateTotalAmount(seatsCount, unitPrice, cabinClass);
        SpecialRequests = specialRequests ?? string.Empty;

        AddDomainEvent(new FlightBookingCreatedEvent(Id, BookingReference, FlightId, PassengerId, TotalAmount));
    }

    public void Confirm()
    {
        if (Status != BookingStatus.Pending)
            throw new InvalidOperationException($"只有待确认的预订才能确认，当前状态：{Status}");

        Status = BookingStatus.Confirmed;
        ConfirmationTime = DateTime.UtcNow;

        AddDomainEvent(new FlightBookingConfirmedEvent(Id, BookingReference));
    }

    public void Cancel(string reason)
    {
        if (Status == BookingStatus.Cancelled)
            return;

        if (Status == BookingStatus.CheckedIn)
            throw new InvalidOperationException("已办理登机手续的预订无法取消");

        var previousStatus = Status;
        Status = BookingStatus.Cancelled;
        CancellationTime = DateTime.UtcNow;
        CancellationReason = reason;

        // 计算退款金额
        RefundAmount = CalculateRefundAmount(previousStatus);

        AddDomainEvent(new FlightBookingCancelledEvent(Id, BookingReference, reason, RefundAmount));
    }

    public void ProcessPayment(string paymentReference)
    {
        if (PaymentStatus == PaymentStatus.Paid)
            throw new InvalidOperationException("该预订已经支付");

        PaymentStatus = PaymentStatus.Paid;
        PaymentTime = DateTime.UtcNow;
        PaymentReference = paymentReference;

        AddDomainEvent(new FlightBookingPaidEvent(Id, BookingReference, TotalAmount, paymentReference));
    }

    public void RefundPayment()
    {
        if (PaymentStatus != PaymentStatus.Paid)
            throw new InvalidOperationException("只有已支付的预订才能退款");

        PaymentStatus = PaymentStatus.Refunded;
        AddDomainEvent(new FlightBookingRefundedEvent(Id, BookingReference, RefundAmount));
    }

    public void CheckIn()
    {
        if (Status != BookingStatus.Confirmed)
            throw new InvalidOperationException("只有已确认的预订才能办理登机手续");

        if (PaymentStatus != PaymentStatus.Paid)
            throw new InvalidOperationException("未支付的预订无法办理登机手续");

        Status = BookingStatus.CheckedIn;
        CheckInTime = DateTime.UtcNow;

        AddDomainEvent(new FlightBookingCheckedInEvent(Id, BookingReference));
    }

    public void UpdateSpecialRequests(string specialRequests)
    {
        if (Status == BookingStatus.Cancelled)
            throw new InvalidOperationException("已取消的预订无法修改");

        SpecialRequests = specialRequests ?? string.Empty;
    }

    public bool CanBeCancelled => Status != BookingStatus.Cancelled && Status != BookingStatus.CheckedIn;
    public bool CanCheckIn => Status == BookingStatus.Confirmed && PaymentStatus == PaymentStatus.Paid;
    public bool IsActive => Status != BookingStatus.Cancelled;

    private decimal CalculateTotalAmount(int seatsCount, decimal unitPrice, CabinClass cabinClass)
    {
        var baseAmount = seatsCount * unitPrice;

        // 根据舱位等级调整价格
        var classMultiplier = cabinClass switch
        {
            CabinClass.Economy => 1.0m,
            CabinClass.PremiumEconomy => 1.3m,
            CabinClass.Business => 2.5m,
            CabinClass.First => 4.0m,
            _ => 1.0m
        };

        return baseAmount * classMultiplier;
    }

    private decimal CalculateRefundAmount(BookingStatus previousStatus)
    {
        // 根据预订状态和时间计算退款金额
        if (PaymentStatus != PaymentStatus.Paid)
            return 0;

        return previousStatus switch
        {
            BookingStatus.Pending => TotalAmount, // 未确认可全额退款
            BookingStatus.Confirmed => TotalAmount * 0.8m, // 已确认扣除20%手续费
            _ => 0
        };
    }

    private void ValidateInput(string bookingReference, Guid flightId, Guid passengerId, int seatsCount, decimal unitPrice)
    {
        if (string.IsNullOrWhiteSpace(bookingReference)) throw new ArgumentException("预订参考号不能为空");
        if (flightId == Guid.Empty) throw new ArgumentException("航班ID不能为空");
        if (passengerId == Guid.Empty) throw new ArgumentException("乘客ID不能为空");
        if (seatsCount <= 0) throw new ArgumentException("座位数必须大于0");
        if (unitPrice < 0) throw new ArgumentException("单价不能为负数");
    }

    public void ClearDomainEvents() => _domainEvents.Clear();
    private void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
}
