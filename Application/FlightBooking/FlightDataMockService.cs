using Domain.FlightBooking.Entities;
using Domain.FlightBooking.Repositories;
using Domain.FlightBooking.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Application.FlightBooking;

public class FlightDataMockService
{
    private readonly IAirportRepository _airportRepository;
    private readonly IFlightRepository _flightRepository;
    private readonly ILogger<FlightDataMockService> _logger;

    private static readonly AirlineInfo[] Airlines =
    [
        new("CZ", "中国南方航空"),
        new("MU", "中国东方航空"),
        new("CA", "中国国际航空"),
        new("HU", "海南航空"),
        new("ZH", "深圳航空"),
        new("MF", "厦门航空")
    ];

    private static readonly string[] AircraftTypes =
    [
        "Airbus A320",
        "Airbus A321",
        "Airbus A350-900",
        "Boeing 737-800",
        "Boeing 787-9",
        "COMAC C919"
    ];

    public FlightDataMockService(
        IAirportRepository airportRepository,
        IFlightRepository flightRepository,
        ILogger<FlightDataMockService> logger)
    {
        _airportRepository = airportRepository;
        _flightRepository = flightRepository;
        _logger = logger;
    }

    public Task GenerateDailyMockFlightsAsync()
        => GenerateDailyMockFlightsAsync(Random.Shared);

    public async Task GenerateDailyMockFlightsAsync(Random random)
    {
        if (random is null)
        {
            throw new ArgumentNullException(nameof(random));
        }

        var today = DateTime.Today;
        var windowStart = today.AddDays(1);
        var windowEnd = windowStart.AddDays(14);

        var airports = (await _airportRepository.GetActivateAirportsAsync()).Where(a => a.IsActive).ToList();

        if (airports.Count < 2)
        {
            _logger.LogWarning(
                "Mock flight maintenance skipped for {Start} - {End}: at least two active airports are required.",
                windowStart.ToShortDateString(),
                windowEnd.ToShortDateString());
            return;
        }

        await _flightRepository.DeleteByDepartureBeforeAsync(windowStart);

        var flightsInWindow = (await _flightRepository.GetByDepartureDateRangeAsync(windowStart, windowEnd)).ToList();

        var desiredFlightsPerDay = Math.Min(airports.Count * 2, 30);
        var generatedKeys = new HashSet<string>(
            flightsInWindow.Select(f => $"{f.FlightNumber}-{f.DepartureTime:yyyyMMddHHmm}"),
            StringComparer.OrdinalIgnoreCase);

        var totalGenerated = 0;
        var totalAttempts = 0;

        for (var day = 0; day < 15; day++)
        {
            var targetDate = windowStart.AddDays(day);
            var flightsForDay = flightsInWindow.Where(f => f.DepartureTime.Date == targetDate.Date).ToList();
            var missingCount = desiredFlightsPerDay - flightsForDay.Count;

            var attempts = 0;
            while (missingCount > 0 && attempts < missingCount * 8)
            {
                attempts++;
                totalAttempts++;

                var departure = airports[random.Next(airports.Count)];
                var arrival = airports[random.Next(airports.Count)];

                if (departure.Code.Equals(arrival.Code, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var airline = Airlines[random.Next(Airlines.Length)];
                var flightNumber = $"{airline.Code}{random.Next(1000, 9999)}";

                var departureTime = targetDate
                    .AddHours(random.Next(6, 23))
                    .AddMinutes(random.Next(0, 4) * 15);
                var arrivalTime = departureTime
                    .AddHours(random.Next(1, 12))
                    .AddMinutes(random.Next(0, 4) * 15);

                if (arrivalTime <= departureTime)
                {
                    continue;
                }

                var key = $"{flightNumber}-{departureTime:yyyyMMddHHmm}";
                if (!generatedKeys.Add(key))
                {
                    continue;
                }

                if (await _flightRepository.ExistsAsync(flightNumber, departureTime))
                {
                    generatedKeys.Remove(key);
                    continue;
                }

                var aircraftType = AircraftTypes[random.Next(AircraftTypes.Length)];
                var totalSeats = random.Next(120, 321);
                var basePrice = Math.Round((decimal)(random.NextDouble() * 400 + 600), 2);

                try
                {
                    var flight = new Flight(
                        flightNumber,
                        airline.Code,
                        airline.Name,
                        departure.Code,
                        departureTime,
                        $"T{random.Next(1, 4)}",
                        arrival.Code,
                        arrivalTime,
                        $"T{random.Next(1, 4)}",
                        aircraftType,
                        totalSeats,
                        basePrice);

                    await _flightRepository.AddAsync(flight);
                    flightsInWindow.Add(flight);
                    flightsForDay.Add(flight);
                    missingCount--;
                    totalGenerated++;
                }
                catch (Exception ex)
                {
                    generatedKeys.Remove(key);
                    _logger.LogWarning(ex, "Failed to generate mock flight {FlightNumber}.", flightNumber);
                }
            }
        }

        _logger.LogInformation(
            "Generated {Generated} flights between {Start} and {End} after {Attempts} attempts.",
            totalGenerated,
            windowStart.ToShortDateString(),
            windowEnd.ToShortDateString(),
            totalAttempts);

        await ApplyRandomUpdatesAsync(flightsInWindow, random, windowStart, windowEnd);
    }

    private async Task ApplyRandomUpdatesAsync(
        List<Flight> flights,
        Random random,
        DateTime windowStart,
        DateTime windowEnd)
    {
        if (flights.Count == 0)
        {
            return;
        }

        var candidates = flights
            .Where(f => f.DepartureTime.Date >= windowStart.Date && f.DepartureTime.Date <= windowEnd.Date)
            .ToList();

        if (candidates.Count == 0)
        {
            return;
        }

        var statuses = Enum.GetValues<FlightStatus>();
        var updateTarget = Math.Max(1, candidates.Count / 8);
        var updated = 0;
        var selected = new HashSet<Guid>();

        for (var i = 0; i < updateTarget; i++)
        {
            Flight flight;
            var guard = 0;
            do
            {
                flight = candidates[random.Next(candidates.Count)];
            }
            while (!selected.Add(flight.Id) && guard++ < candidates.Count * 2);

            var changed = false;

            var newStatus = statuses[random.Next(statuses.Length)];
            if (newStatus != flight.Status)
            {
                flight.UpdateStatus(newStatus);
                changed = true;
            }

            var multiplier = (decimal)(random.NextDouble() * 0.4 - 0.2);
            var proposedPrice = Math.Round(flight.BasePrice * (1 + multiplier), 2, MidpointRounding.AwayFromZero);
            if (proposedPrice <= 0)
            {
                proposedPrice = 100m;
            }
            if (proposedPrice == flight.BasePrice)
            {
                proposedPrice = Math.Round(flight.BasePrice + Math.Max(50m, flight.BasePrice * 0.05m), 2, MidpointRounding.AwayFromZero);
            }
            if (proposedPrice != flight.BasePrice)
            {
                flight.UpdatePrice(proposedPrice);
                changed = true;
            }

            var newAvailableSeats = random.Next(0, flight.TotalSeats + 1);
            if (newAvailableSeats == flight.AvailableSeats)
            {
                newAvailableSeats = Math.Clamp(flight.AvailableSeats + (random.Next(0, 2) == 0 ? -1 : 1), 0, flight.TotalSeats);
            }
            if (newAvailableSeats != flight.AvailableSeats)
            {
                flight.AdjustAvailableSeats(newAvailableSeats);
                changed = true;
            }

            if (changed)
            {
                await _flightRepository.UpdateAsync(flight);
                updated++;
            }
        }

        _logger.LogInformation(
            "Randomly updated {Updated} flights between {Start} and {End}.",
            updated,
            windowStart.ToShortDateString(),
            windowEnd.ToShortDateString());
    }

    private sealed record AirlineInfo(string Code, string Name);
}
