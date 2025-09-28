using System;

namespace Domain.FlightBooking.ValueObjects;

/// <summary>
/// 乘客个人资料（值对象）
/// </summary>
public readonly record struct PassengerProfile
{
    public string FirstName { get; }
    public string LastName { get; }
    public string Email { get; }
    public string PassportNumber { get; }
    public DateTime DateOfBirth { get; }
    public string Nationality { get; }

    public PassengerProfile(
        string firstName,
        string lastName,
        string email,
        string passportNumber,
        DateTime dateOfBirth,
        string nationality)
    {
        if (string.IsNullOrWhiteSpace(firstName)) throw new ArgumentException("名字不能为空", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName)) throw new ArgumentException("姓氏不能为空", nameof(lastName));
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("邮箱不能为空", nameof(email));
        if (string.IsNullOrWhiteSpace(passportNumber)) throw new ArgumentException("护照号不能为空", nameof(passportNumber));
        if (string.IsNullOrWhiteSpace(nationality)) throw new ArgumentException("国籍不能为空", nameof(nationality));
        if (dateOfBirth >= DateTime.Today) throw new ArgumentException("出生日期不能是未来日期", nameof(dateOfBirth));

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Email = email.Trim();
        PassportNumber = passportNumber.Trim().ToUpperInvariant();
        DateOfBirth = dateOfBirth;
        Nationality = nationality.Trim();
    }
}
