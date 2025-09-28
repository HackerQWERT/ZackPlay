using Application.FlightBooking;
using Domain.FlightBooking.Entities;
using Domain.FlightBooking.Repositories;
using Domain.FlightBooking.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace XUnitTest.Application.FlightBooking;

public class FlightDataMockServiceTests
{
    private readonly InMemoryAirportRepository _airportRepository = new();

    [Fact]
    public async Task GenerateDailyMockFlightsAsync_ShouldMaintainRollingWindow()
    {
        var flightRepository = CreateFlightRepositoryWithSeed();
        var service = CreateService(flightRepository);

        await service.GenerateDailyMockFlightsAsync(new Random(0));

        var flights = flightRepository.GetSnapshot();
        var expectedStart = DateTime.Today.AddDays(1).Date;
        var expectedEnd = DateTime.Today.AddDays(15).Date;

        flights.Should().NotBeEmpty();
        flights.Should().OnlyContain(f => f.DepartureTime.Date >= expectedStart && f.DepartureTime.Date <= expectedEnd);
    }

    [Fact]
    public async Task GenerateDailyMockFlightsAsync_ShouldAddFlightsForNewestDay()
    {
        var flightRepository = CreateFlightRepositoryWithSeed();
        var service = CreateService(flightRepository);

        await service.GenerateDailyMockFlightsAsync(new Random(1));

        var newestDay = DateTime.Today.AddDays(15).Date;
        var flights = flightRepository.GetSnapshot();

        flights.Count(f => f.DepartureTime.Date == newestDay).Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GenerateDailyMockFlightsAsync_ShouldApplyRandomUpdates()
    {
        var flightRepository = CreateFlightRepositoryWithSeed();
        var service = CreateService(flightRepository);

        await service.GenerateDailyMockFlightsAsync(new Random(2));
        var before = flightRepository.GetSnapshot()
            .ToDictionary(f => f.Id, f => new FlightSnapshot(f.BasePrice, f.AvailableSeats, f.Status));

        await service.GenerateDailyMockFlightsAsync(new Random(3));
        var after = flightRepository.GetSnapshot();

        var changed = after.Any(f =>
        {
            if (!before.TryGetValue(f.Id, out var snapshot))
            {
                return false;
            }

            return snapshot.BasePrice != f.BasePrice
                || snapshot.AvailableSeats != f.AvailableSeats
                || snapshot.Status != f.Status;
        });

        changed.Should().BeTrue();
    }

    private FlightDataMockService CreateService(InMemoryFlightRepository flightRepository)
    {
        return new FlightDataMockService(_airportRepository, flightRepository, NullLogger<FlightDataMockService>.Instance);
    }

    private static InMemoryFlightRepository CreateFlightRepositoryWithSeed()
    {
        var repository = new InMemoryFlightRepository();

        var pastFlight = new Flight(
            "CZ1001",
            "CZ",
            "中国南方航空",
            "PEK",
            DateTime.Today.AddDays(-1).AddHours(9),
            "T1",
            "SHA",
            DateTime.Today.AddDays(-1).AddHours(11),
            "T2",
            "Airbus A320",
            180,
            800m);

        repository.Seed(pastFlight);

        return repository;
    }

    private sealed record FlightSnapshot(decimal BasePrice, int AvailableSeats, FlightStatus Status);
}

internal sealed class InMemoryAirportRepository : IAirportRepository
{
    private readonly List<Airport> _airports =
    [
        new("PEK", "北京首都国际机场", "北京", "中国", "Asia/Shanghai"),
        new("SHA", "上海虹桥国际机场", "上海", "中国", "Asia/Shanghai"),
        new("CAN", "广州白云国际机场", "广州", "中国", "Asia/Shanghai")
    ];

    public Task AddAsync(Airport airport)
    {
        var index = _airports.FindIndex(a => a.Code == airport.Code);
        if (index >= 0)
        {
            _airports[index] = airport;
        }
        else
        {
            _airports.Add(airport);
        }
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string code) => Task.FromResult(_airports.Any(a => a.Code.Equals(code, StringComparison.OrdinalIgnoreCase)));

    public Task<IEnumerable<Airport>> GetActivateAirportsAsync()
    {
        var result = _airports.Where(a => a.IsActive).ToList().AsEnumerable();
        return Task.FromResult(result);
    }

    public Task<IEnumerable<Airport>> GetByCountryAsync(string country)
    {
        var result = _airports.Where(a => a.Country.Equals(country, StringComparison.OrdinalIgnoreCase)).ToList().AsEnumerable();
        return Task.FromResult(result);
    }

    public Task<Airport?> GetByCodeAsync(string code)
    {
        var airport = _airports.FirstOrDefault(a => a.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult<Airport?>(airport);
    }

    public Task UpdateAsync(Airport airport)
    {
        var index = _airports.FindIndex(a => a.Code == airport.Code);
        if (index >= 0)
        {
            _airports[index] = airport;
        }
        return Task.CompletedTask;
    }
}

internal sealed class InMemoryFlightRepository : IFlightRepository
{
    private readonly List<Flight> _flights = new();
    private readonly object _syncRoot = new();

    public Task AddAsync(Flight flight)
    {
        lock (_syncRoot)
        {
            _flights.Add(CloneFlight(flight));
        }
        return Task.CompletedTask;
    }

    public Task DeleteByDepartureBeforeAsync(DateTime cutoffDate)
    {
        lock (_syncRoot)
        {
            _flights.RemoveAll(f => f.DepartureTime.Date < cutoffDate.Date);
        }
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string flightNumber, DateTime departureDate)
    {
        lock (_syncRoot)
        {
            return Task.FromResult(_flights.Any(f => f.FlightNumber.Equals(flightNumber, StringComparison.OrdinalIgnoreCase)
                                                  && f.DepartureTime.Date == departureDate.Date));
        }
    }

    public Task<IEnumerable<Flight>> GetByAirlineAsync(string airlineCode)
    {
        lock (_syncRoot)
        {
            var flights = _flights
                .Where(f => f.AirlineCode.Equals(airlineCode, StringComparison.OrdinalIgnoreCase))
                .Select(CloneFlight)
                .ToList()
                .AsEnumerable();
            return Task.FromResult(flights);
        }
    }

    public Task<Flight?> GetByFlightNumberAsync(string flightNumber, DateTime departureDate)
    {
        lock (_syncRoot)
        {
            var flight = _flights.FirstOrDefault(f => f.FlightNumber.Equals(flightNumber, StringComparison.OrdinalIgnoreCase)
                                                    && f.DepartureTime.Date == departureDate.Date);
            return Task.FromResult(CloneFlightOrDefault(flight));
        }
    }

    public Task<IEnumerable<Flight>> GetByDepartureDateRangeAsync(DateTime startDateInclusive, DateTime endDateInclusive)
    {
        lock (_syncRoot)
        {
            var start = startDateInclusive.Date;
            var end = endDateInclusive.Date;
            var flights = _flights
                .Where(f => f.DepartureTime.Date >= start && f.DepartureTime.Date <= end)
                .Select(CloneFlight)
                .ToList()
                .AsEnumerable();
            return Task.FromResult(flights);
        }
    }

    public Task<Flight?> GetByIdAsync(Guid id)
    {
        lock (_syncRoot)
        {
            var flight = _flights.FirstOrDefault(f => f.Id == id);
            return Task.FromResult(CloneFlightOrDefault(flight));
        }
    }

    public Task<IEnumerable<Flight>> SearchAsync(string departureAirport, string arrivalAirport, DateTime departureDate)
    {
        lock (_syncRoot)
        {
            var flights = _flights
                .Where(f => f.DepartureAirportCode.Equals(departureAirport, StringComparison.OrdinalIgnoreCase)
                         && f.ArrivalAirportCode.Equals(arrivalAirport, StringComparison.OrdinalIgnoreCase)
                         && f.DepartureTime.Date == departureDate.Date)
                .Select(CloneFlight)
                .ToList()
                .AsEnumerable();
            return Task.FromResult(flights);
        }
    }

    public Task UpdateAsync(Flight flight)
    {
        lock (_syncRoot)
        {
            var index = _flights.FindIndex(f => f.Id == flight.Id);
            if (index >= 0)
            {
                _flights[index] = CloneFlight(flight);
            }
        }
        return Task.CompletedTask;
    }

    public List<Flight> GetSnapshot()
    {
        lock (_syncRoot)
        {
            return _flights.Select(CloneFlight).ToList();
        }
    }

    public void Seed(params Flight[] flights)
    {
        lock (_syncRoot)
        {
            foreach (var flight in flights)
            {
                _flights.Add(CloneFlight(flight));
            }
        }
    }

    private static Flight? CloneFlightOrDefault(Flight? flight)
    {
        return flight is null ? null : CloneFlight(flight);
    }

    private static Flight CloneFlight(Flight flight)
    {
        return Flight.Rehydrate(
            flight.Id,
            flight.FlightNumber,
            flight.AirlineCode,
            flight.AirlineName,
            flight.DepartureAirportCode,
            flight.DepartureTime,
            flight.DepartureTerminal,
            flight.ArrivalAirportCode,
            flight.ArrivalTime,
            flight.ArrivalTerminal,
            flight.AircraftType,
            flight.TotalSeats,
            flight.AvailableSeats,
            flight.BasePrice,
            flight.Status,
            flight.CreatedAt,
            flight.UpdatedAt);
    }
}
