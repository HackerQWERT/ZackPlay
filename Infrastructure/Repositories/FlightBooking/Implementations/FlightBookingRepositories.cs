using Domain.FlightBooking.Entities;
using Domain.FlightBooking.Repositories;
using Domain.FlightBooking.ValueObjects;
using Infrastructure.Data;
using Infrastructure.Repositories.FlightBooking.Po;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.FlightBooking.Implementations;

public class AirportRepository : IAirportRepository
{
    private readonly FlightBookingDbContext _context;

    public AirportRepository(FlightBookingDbContext context)
    {
        _context = context;
    }

    public async Task<Airport?> GetByCodeAsync(string code)
    {
        var po = await _context.Airports.FirstOrDefaultAsync(a => a.Code == code.ToUpper());
        return po?.ToDomain();
    }

    public async Task<IEnumerable<Airport>> GetByCountryAsync(string country)
    {
        var pos = await _context.Airports
            .Where(a => a.Country == country && a.IsActive)
            .ToListAsync();

        return pos.Select(po => po.ToDomain());
    }

    public async Task<IEnumerable<Airport>> GetActivateAirportsAsync()
    {
        var pos = await _context.Airports
            .Where(a => a.IsActive)
            .OrderBy(a => a.Country)
            .ThenBy(a => a.City)
            .ToListAsync();

        return pos.Select(po => po.ToDomain());
    }

    public async Task AddAsync(Airport airport)
    {
        var po = AirportPo.FromDomain(airport);
        await _context.Airports.AddAsync(po);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Airport airport)
    {
        var po = AirportPo.FromDomain(airport);
        _context.Airports.Update(po);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(string code)
    {
        return await _context.Airports.AnyAsync(a => a.Code == code.ToUpper());
    }
}

public class FlightRepository : IFlightRepository
{
    private readonly FlightBookingDbContext _context;

    public FlightRepository(FlightBookingDbContext context)
    {
        _context = context;
    }

    public async Task<Flight?> GetByIdAsync(Guid id)
    {
        var po = await _context.Flights.FirstOrDefaultAsync(f => f.Id == id);
        return po?.ToDomain();
    }

    public async Task<Flight?> GetByFlightNumberAsync(string flightNumber, DateTime departureDate)
    {
        var po = await _context.Flights
            .FirstOrDefaultAsync(f => f.FlightNumber == flightNumber.ToUpper()
                                   && f.DepartureTime.Date == departureDate.Date);
        return po?.ToDomain();
    }

    public async Task<IEnumerable<Flight>> SearchAsync(string departureAirport, string arrivalAirport, DateTime departureDate)
    {
        var pos = await _context.Flights
            .Where(f => f.DepartureAirportCode == departureAirport.ToUpper()
                     && f.ArrivalAirportCode == arrivalAirport.ToUpper()
                     && f.DepartureTime.Date == departureDate.Date
                     && f.Status == (int)FlightStatus.Scheduled)
            .OrderBy(f => f.DepartureTime)
            .ToListAsync();

        return pos.Select(po => po.ToDomain());
    }

    public async Task<IEnumerable<Flight>> GetByAirlineAsync(string airlineCode)
    {
        var pos = await _context.Flights
            .Where(f => f.AirlineCode == airlineCode.ToUpper())
            .OrderBy(f => f.DepartureTime)
            .ToListAsync();

        return pos.Select(po => po.ToDomain());
    }

    public async Task<IEnumerable<Flight>> GetByDepartureDateRangeAsync(DateTime startDateInclusive, DateTime endDateInclusive)
    {
        var start = startDateInclusive.Date;
        var endExclusive = endDateInclusive.Date.AddDays(1);

        var pos = await _context.Flights
            .Where(f => f.DepartureTime >= start && f.DepartureTime < endExclusive)
            .OrderBy(f => f.DepartureTime)
            .ToListAsync();

        return pos.Select(po => po.ToDomain());
    }

    public async Task AddAsync(Flight flight)
    {
        var po = FlightPo.FromDomain(flight);
        await _context.Flights.AddAsync(po);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Flight flight)
    {
        var existing = await _context.Flights.FindAsync(flight.Id);
        if (existing != null)
        {
            existing.FlightNumber = flight.FlightNumber;
            existing.AirlineCode = flight.AirlineCode;
            existing.AirlineName = flight.AirlineName;
            existing.DepartureAirportCode = flight.DepartureAirportCode;
            existing.DepartureTime = flight.DepartureTime;
            existing.DepartureTerminal = flight.DepartureTerminal;
            existing.ArrivalAirportCode = flight.ArrivalAirportCode;
            existing.ArrivalTime = flight.ArrivalTime;
            existing.ArrivalTerminal = flight.ArrivalTerminal;
            existing.AircraftType = flight.AircraftType;
            existing.TotalSeats = flight.TotalSeats;
            existing.AvailableSeats = flight.AvailableSeats;
            existing.BasePrice = flight.BasePrice;
            existing.Status = (int)flight.Status;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            var po = FlightPo.FromDomain(flight);
            await _context.Flights.AddAsync(po);
        }
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(string flightNumber, DateTime departureDate)
    {
        return await _context.Flights
            .AnyAsync(f => f.FlightNumber == flightNumber.ToUpper()
                        && f.DepartureTime.Date == departureDate.Date);
    }

    public async Task DeleteByDepartureBeforeAsync(DateTime cutoffDate)
    {
        var cutoff = cutoffDate.Date;
        var expiredFlights = await _context.Flights
            .Where(f => f.DepartureTime.Date < cutoff)
            .ToListAsync();

        if (expiredFlights.Count == 0)
        {
            return;
        }

        _context.Flights.RemoveRange(expiredFlights);
        await _context.SaveChangesAsync();
    }
}

public class PassengerRepository : IPassengerRepository
{
    private readonly FlightBookingDbContext _context;

    public PassengerRepository(FlightBookingDbContext context)
    {
        _context = context;
    }

    public async Task<Passenger?> GetByIdAsync(Guid id)
    {
        var po = await _context.Passengers.FirstOrDefaultAsync(p => p.Id == id);
        return po?.ToDomain();
    }

    public async Task<Passenger?> GetByPassportAsync(string passportNumber)
    {
        var po = await _context.Passengers.FirstOrDefaultAsync(p => p.PassportNumber == passportNumber.ToUpper());
        return po?.ToDomain();
    }

    public async Task<Passenger?> GetByEmailAsync(string email)
    {
        var po = await _context.Passengers.FirstOrDefaultAsync(p => p.Email == email.ToLower());
        return po?.ToDomain();
    }

    public async Task<IEnumerable<Passenger>> SearchAsync(string firstName, string lastName)
    {
        var pos = await _context.Passengers
            .Where(p => p.FirstName.Contains(firstName) || p.LastName.Contains(lastName))
            .ToListAsync();

        return pos.Select(po => po.ToDomain());
    }

    public async Task AddAsync(Passenger passenger)
    {
        var po = PassengerPo.FromDomain(passenger);
        await _context.Passengers.AddAsync(po);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Passenger passenger)
    {
        var po = PassengerPo.FromDomain(passenger);
        _context.Passengers.Update(po);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsByPassportAsync(string passportNumber)
    {
        return await _context.Passengers.AnyAsync(p => p.PassportNumber == passportNumber.ToUpper());
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _context.Passengers.AnyAsync(p => p.Email == email.ToLower());
    }
}

public class FlightBookingRepository : IFlightBookingRepository
{
    private readonly FlightBookingDbContext _context;

    public FlightBookingRepository(FlightBookingDbContext context)
    {
        _context = context;
    }

    public async Task<Domain.FlightBooking.Entities.FlightBooking?> GetByIdAsync(Guid id)
    {
        var po = await _context.FlightBookings.FirstOrDefaultAsync(b => b.Id == id);
        return po?.ToDomain();
    }

    public async Task<Domain.FlightBooking.Entities.FlightBooking?> GetByReferenceAsync(string bookingReference)
    {
        var po = await _context.FlightBookings.FirstOrDefaultAsync(b => b.BookingReference == bookingReference.ToUpper());
        return po?.ToDomain();
    }

    public async Task<IEnumerable<Domain.FlightBooking.Entities.FlightBooking>> GetByPassengerAsync(Guid passengerId)
    {
        var pos = await _context.FlightBookings
            .Where(b => b.PassengerId == passengerId)
            .OrderByDescending(b => b.BookingTime)
            .ToListAsync();

        return pos.Select(po => po.ToDomain());
    }

    public async Task<IEnumerable<Domain.FlightBooking.Entities.FlightBooking>> GetByFlightAsync(Guid flightId)
    {
        var pos = await _context.FlightBookings
            .Where(b => b.FlightId == flightId)
            .OrderBy(b => b.BookingTime)
            .ToListAsync();

        return pos.Select(po => po.ToDomain());
    }

    public async Task<IEnumerable<Domain.FlightBooking.Entities.FlightBooking>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var pos = await _context.FlightBookings
            .Where(b => b.BookingTime.Date >= startDate.Date && b.BookingTime.Date <= endDate.Date)
            .OrderByDescending(b => b.BookingTime)
            .ToListAsync();

        return pos.Select(po => po.ToDomain());
    }

    public async Task AddAsync(Domain.FlightBooking.Entities.FlightBooking booking)
    {
        var po = FlightBookingPo.FromDomain(booking);
        await _context.FlightBookings.AddAsync(po);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Domain.FlightBooking.Entities.FlightBooking booking)
    {
        var po = FlightBookingPo.FromDomain(booking);
        _context.FlightBookings.Update(po);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsByReferenceAsync(string bookingReference)
    {
        return await _context.FlightBookings.AnyAsync(b => b.BookingReference == bookingReference.ToUpper());
    }

    public async Task<int> GetBookingCountByFlightAsync(Guid flightId)
    {
        return await _context.FlightBookings
            .Where(b => b.FlightId == flightId && b.Status != (int)BookingStatus.Cancelled)
            .SumAsync(b => b.SeatsCount);
    }
}
