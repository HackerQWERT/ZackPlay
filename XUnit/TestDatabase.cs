using System;
using FluentAssertions;
using Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using XUnitTest.Base;

namespace XUnitTest;

public class TestDatabase : BaseTest
{
    [Fact]
    public void TestConnection()
    {
        var db = Provider.GetRequiredService<FlightBookingDbContext>();

        // Act
        var result = db.Database.CanConnect();

        // Assert
        result.Should().BeTrue();
    }
}
