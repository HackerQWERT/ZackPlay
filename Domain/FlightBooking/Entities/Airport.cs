using Domain.Abstractions;

namespace Domain.FlightBooking.Entities;

/// <summary>
/// 机场实体
/// </summary>
public class Airport
{
    public string Code { get; private set; } = default!; // 机场三字码，如 PEK, SHA
    public string Name { get; private set; } = default!; // 机场名称
    public string City { get; private set; } = default!; // 所在城市
    public string Country { get; private set; } = default!; // 所在国家
    public string TimeZone { get; private set; } = default!; // 时区
    public bool IsActive { get; private set; } = true; // 是否启用
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; private set; }

    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    private Airport() { } // EF Core

    public Airport(string code, string name, string city, string country, string timeZone)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("机场代码不能为空", nameof(code));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("机场名称不能为空", nameof(name));
        if (string.IsNullOrWhiteSpace(city)) throw new ArgumentException("城市不能为空", nameof(city));
        if (string.IsNullOrWhiteSpace(country)) throw new ArgumentException("国家不能为空", nameof(country));
        if (string.IsNullOrWhiteSpace(timeZone)) throw new ArgumentException("时区不能为空", nameof(timeZone));

        Code = code.ToUpper();
        Name = name;
        City = city;
        Country = country;
        TimeZone = timeZone;
    }

    public void UpdateInfo(string name, string city, string country, string timeZone)
    {
        Name = name;
        City = city;
        Country = country;
        TimeZone = timeZone;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ClearDomainEvents() => _domainEvents.Clear();
    private void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
}
