using System;
using System.Diagnostics;
using FluentAssertions;
using Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;
using XUnitTest.Base;

namespace XUnitTest;

public class TestDatabase : BaseTest
{
    private ITestOutputHelper _output { get; }

    public TestDatabase(ITestOutputHelper output) : base()
    {
        _output = output;
    }

    [Fact]
    public async Task TestConnection()
    {
        await using var db = Provider.GetRequiredService<FlightBookingDbContext>();

        // Act
        var result = await db.Database.CanConnectAsync();
        // Assert
        result.Should().BeTrue();

        _output.WriteLine(result.ToString());
    }
    [Fact]
    public async Task TestPlay()
    {
        bool result = "Y" == "Y";
        _output.WriteLine(result.ToString());
    }
}
