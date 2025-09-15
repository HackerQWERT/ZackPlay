using Domain.Abstractions;
using Domain.FlightBooking.ValueObjects;

namespace Domain.FlightBooking.Entities;

/// <summary>
/// 乘客实体
/// </summary>
public class Passenger : IAggregateRoot
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;
    public string FullName => $"{FirstName} {LastName}";
    public DateTime DateOfBirth { get; private set; }
    public Gender Gender { get; private set; }
    public string PassportNumber { get; private set; } = default!;
    public string PassportCountry { get; private set; } = default!;
    public DateTime PassportExpiryDate { get; private set; }
    public string Nationality { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string PhoneNumber { get; private set; } = default!;
    public PassengerType Type { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; private set; }

    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    private Passenger() { } // EF Core

    public Passenger(
        string firstName,
        string lastName,
        DateTime dateOfBirth,
        Gender gender,
        string passportNumber,
        string passportCountry,
        DateTime passportExpiryDate,
        string nationality,
        string email,
        string phoneNumber)
    {
        ValidateInput(firstName, lastName, passportNumber, passportCountry, nationality, email, phoneNumber);
        ValidateDates(dateOfBirth, passportExpiryDate);

        FirstName = firstName;
        LastName = lastName;
        DateOfBirth = dateOfBirth;
        Gender = gender;
        PassportNumber = passportNumber.ToUpper();
        PassportCountry = passportCountry.ToUpper();
        PassportExpiryDate = passportExpiryDate;
        Nationality = nationality;
        Email = email.ToLower();
        PhoneNumber = phoneNumber;
        Type = CalculatePassengerType(dateOfBirth);
    }

    public int Age => DateTime.Today.Year - DateOfBirth.Year -
                     (DateTime.Today.DayOfYear < DateOfBirth.DayOfYear ? 1 : 0);

    public bool IsPassportValid => PassportExpiryDate > DateTime.Today.AddMonths(6);

    public void UpdateContactInfo(string email, string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("邮箱不能为空");
        if (string.IsNullOrWhiteSpace(phoneNumber)) throw new ArgumentException("电话号码不能为空");

        Email = email.ToLower();
        PhoneNumber = phoneNumber;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePassportInfo(string passportNumber, string passportCountry, DateTime passportExpiryDate)
    {
        if (string.IsNullOrWhiteSpace(passportNumber)) throw new ArgumentException("护照号不能为空");
        if (string.IsNullOrWhiteSpace(passportCountry)) throw new ArgumentException("护照签发国不能为空");
        if (passportExpiryDate <= DateTime.Today) throw new ArgumentException("护照已过期或即将过期");

        PassportNumber = passportNumber.ToUpper();
        PassportCountry = passportCountry.ToUpper();
        PassportExpiryDate = passportExpiryDate;
        UpdatedAt = DateTime.UtcNow;
    }

    private PassengerType CalculatePassengerType(DateTime dateOfBirth)
    {
        var age = DateTime.Today.Year - dateOfBirth.Year;
        if (DateTime.Today.DayOfYear < dateOfBirth.DayOfYear) age--;

        return age switch
        {
            < 2 => PassengerType.Infant,
            < 12 => PassengerType.Child,
            >= 65 => PassengerType.Senior,
            _ => PassengerType.Adult
        };
    }

    private void ValidateInput(string firstName, string lastName, string passportNumber,
                              string passportCountry, string nationality, string email, string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(firstName)) throw new ArgumentException("名字不能为空");
        if (string.IsNullOrWhiteSpace(lastName)) throw new ArgumentException("姓氏不能为空");
        if (string.IsNullOrWhiteSpace(passportNumber)) throw new ArgumentException("护照号不能为空");
        if (string.IsNullOrWhiteSpace(passportCountry)) throw new ArgumentException("护照签发国不能为空");
        if (string.IsNullOrWhiteSpace(nationality)) throw new ArgumentException("国籍不能为空");
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("邮箱不能为空");
        if (string.IsNullOrWhiteSpace(phoneNumber)) throw new ArgumentException("电话号码不能为空");
    }

    private void ValidateDates(DateTime dateOfBirth, DateTime passportExpiryDate)
    {
        if (dateOfBirth >= DateTime.Today) throw new ArgumentException("出生日期不能是未来日期");
        if (passportExpiryDate <= DateTime.Today) throw new ArgumentException("护照已过期或即将过期");

        var age = DateTime.Today.Year - dateOfBirth.Year;
        if (age > 120) throw new ArgumentException("年龄不合理");
    }

    public void ClearDomainEvents() => _domainEvents.Clear();
    private void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
}
