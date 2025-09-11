using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Text;
using ZackPlay.Models;
using ZackPlay.Data;
using Microsoft.EntityFrameworkCore;

namespace ZackPlay.Tests;

public class ApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetFlights_ShouldReturnOk()
    {
        // Act
        var response = await _client.GetAsync("/api/flights");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.NotEmpty(content);
    }

    [Fact]
    public async Task CreateFlight_ShouldReturnCreated()
    {
        // Arrange
        var flight = new Flight
        {
            FlightNumber = "TEST123",
            Origin = "测试起点",
            Destination = "测试终点",
            DepartureTime = DateTime.Now.AddDays(1),
            ArrivalTime = DateTime.Now.AddDays(1).AddHours(2),
            Price = 500.00m,
            AvailableSeats = 100,
            TotalSeats = 150,
            Aircraft = "Test Aircraft"
        };

        var json = JsonConvert.SerializeObject(flight);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/flights", content);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
    }
}
