using System;
using Domain.Abstractions;
using Domain.FlightBooking.Events;
using Application.FlightBooking;

namespace API.Consumers;

/// <summary>
/// 监听领域事件队列，调用 Application 层处理业务逻辑
/// </summary>
public class FlightBookingConsumer : BackgroundService
{
    private readonly ILogger<FlightBookingConsumer> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IMessageQueueService _messageQueueService;

    public FlightBookingConsumer(
        ILogger<FlightBookingConsumer> logger,
        IServiceScopeFactory serviceScopeFactory,
        IMessageQueueService messageQueueService)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _messageQueueService = messageQueueService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("FlightBookingConsumer started - subscribing queues");
        try
        {
            // 队列：提交预订
            await _messageQueueService.SubscribeAsync<FlightBookingSubmitedEvent>(
                "submit-booking",
                async (evt) => await SafeExecute(evt, HandleFlightBookingSubmited));

            // 队列：创建成功（如果有其他系统产生此事件也可直连）
            await _messageQueueService.SubscribeAsync<FlightBookingCreatedEvent>(
                "booking-created",
                async (evt) => await SafeExecute(evt, HandleFlightBookingCreated));

            await _messageQueueService.SubscribeAsync<FlightBookingConfirmedEvent>(
                "booking-confirmed",
                async (evt) => await SafeExecute(evt, HandleFlightBookingConfirmed));

            await _messageQueueService.SubscribeAsync<FlightBookingCancelledEvent>(
                "booking-cancelled",
                async (evt) => await SafeExecute(evt, HandleFlightBookingCancelled));


            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in FlightBookingConsumer main loop");
        }
    }

    private async Task SafeExecute<TEvent>(TEvent evt, Func<TEvent, IServiceProvider, Task> handler)
        where TEvent : IDomainEvent
    {
        using var scope = _serviceScopeFactory.CreateScope();
        try
        {
            await handler(evt, scope.ServiceProvider);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed processing {EventType}", typeof(TEvent).Name);
        }
    }

    /// <summary>
    /// 处理创建订单开始事件 - 调用 Application 层
    /// </summary>
    private async Task HandleFlightBookingSubmited(FlightBookingSubmitedEvent @event, IServiceProvider serviceProvider)
    {
        _logger.LogInformation("[Request:{RequestId}] Processing submission-requested event for Flight {FlightId} Seats {Seats}",
            @event.RequestId, @event.FlightId, @event.SeatsCount);

        // 真正执行业务创建：调用 Domain Service
        var bookingDomainService = serviceProvider.GetRequiredService<Domain.FlightBooking.Services.BookingDomainService>();
        var mq = serviceProvider.GetRequiredService<IMessageQueueService>();

        try
        {
            var command = new Domain.FlightBooking.Services.CreateBookingCommand(
                @event.FlightId,
                @event.PassengerFirstName,
                @event.PassengerLastName,
                @event.PassengerEmail,
                @event.PassportNumber,
                @event.DateOfBirth,
                @event.Nationality,
                @event.SeatsCount,
                Enum.TryParse<Domain.FlightBooking.ValueObjects.CabinClass>(@event.CabinClass, true, out var cabin) ? cabin : Domain.FlightBooking.ValueObjects.CabinClass.Economy
            );

            var booking = await bookingDomainService.CreateBookingAsync(command);

            // 发布聚合根产生的领域事件（含 FlightBookingCreatedEvent）
            if (booking.DomainEvents?.Any() == true)
            {
                foreach (var de in booking.DomainEvents)
                {
                    await mq.PublishAsync("domain-events", de);
                }
                booking.ClearDomainEvents();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Request:{RequestId}] Failed to create booking", @event.RequestId);

        }
    }

    /// <summary>
    /// 处理预订创建完成事件 - 调用 Application 层
    /// </summary>
    private async Task HandleFlightBookingCreated(FlightBookingCreatedEvent @event, IServiceProvider serviceProvider)
    {
        _logger.LogInformation("Handling flight booking created: {BookingReference}", @event.BookingReference);

        // 这里调用 Application 层的具体业务处理逻辑
        // 例如：
        // var emailService = serviceProvider.GetRequiredService<IEmailService>();
        // await emailService.SendBookingConfirmationAsync(@event.BookingReference);

        // var inventoryService = serviceProvider.GetRequiredService<IInventoryService>();
        // await inventoryService.UpdateSeatAvailabilityAsync(@event.FlightId);

        await Task.CompletedTask; // 示例：异步操作
    }

    /// <summary>
    /// 处理预订确认事件 - 调用 Application 层
    /// </summary>
    private async Task HandleFlightBookingConfirmed(FlightBookingConfirmedEvent @event, IServiceProvider serviceProvider)
    {
        _logger.LogInformation("Handling flight booking confirmed: {BookingReference}", @event.BookingReference);

        // 调用 Application 层处理确认后的业务逻辑：
        // - 生成电子票
        // - 通知合作伙伴
        // - 发送确认邮件

        await Task.CompletedTask;
    }

    /// <summary>
    /// 处理预订取消事件 - 调用 Application 层
    /// </summary>
    private async Task HandleFlightBookingCancelled(FlightBookingCancelledEvent @event, IServiceProvider serviceProvider)
    {
        _logger.LogInformation("Handling flight booking cancelled: {BookingReference}, Reason: {Reason}",
            @event.BookingReference, @event.Reason);
        // 调用 Application 层处理取消后的业务逻辑：
        // - 处理退款
        // - 释放座位
        // - 发送取消通知

        await Task.CompletedTask;
    }


}