namespace Domain.FlightBooking.ValueObjects;

/// <summary>
/// 航班状态
/// </summary>
public enum FlightStatus
{
    Scheduled = 0,    // 计划中
    Boarding = 1,     // 登机中
    Departed = 2,     // 已起飞
    InFlight = 3,     // 飞行中
    Arrived = 4,      // 已到达
    Delayed = 5,      // 延误
    Cancelled = 6     // 取消
}

/// <summary>
/// 性别
/// </summary>
public enum Gender
{
    Male = 0,
    Female = 1,
    Other = 2
}

/// <summary>
/// 乘客类型
/// </summary>
public enum PassengerType
{
    Infant = 0,   // 婴儿 (0-2岁)
    Child = 1,    // 儿童 (2-12岁)
    Adult = 2,    // 成人 (12-65岁)
    Senior = 3    // 老人 (65岁以上)
}

/// <summary>
/// 预订状态
/// </summary>
public enum BookingStatus
{
    Pending = 0,      // 待确认
    Confirmed = 1,    // 已确认
    CheckedIn = 2,    // 已登机
    Cancelled = 3     // 已取消
}

/// <summary>
/// 舱位等级
/// </summary>
public enum CabinClass
{
    Economy = 0,         // 经济舱
    PremiumEconomy = 1,  // 高端经济舱
    Business = 2,        // 商务舱
    First = 3           // 头等舱
}

/// <summary>
/// 支付状态
/// </summary>
public enum PaymentStatus
{
    Pending = 0,   // 待支付
    Paid = 1,      // 已支付
    Failed = 2,    // 支付失败
    Refunded = 3   // 已退款
}
