using Domain.FlightBooking.Entities;

namespace Domain.FlightBooking.Repositories;

public interface IAirportRepository
{
    Task<Airport?> GetByCodeAsync(string code);
    Task<IEnumerable<Airport>> GetByCountryAsync(string country);
    Task<IEnumerable<Airport>> GetActivateAirportsAsync();
    Task AddAsync(Airport airport);
    Task UpdateAsync(Airport airport);
    Task<bool> ExistsAsync(string code);
}

public interface IFlightRepository
{
    Task<Flight?> GetByIdAsync(Guid id);
    Task<Flight?> GetByFlightNumberAsync(string flightNumber, DateTime departureDate);
    Task<IEnumerable<Flight>> SearchAsync(string departureAirport, string arrivalAirport, DateTime departureDate);
    Task<IEnumerable<Flight>> GetByAirlineAsync(string airlineCode);
    Task AddAsync(Flight flight);
    Task UpdateAsync(Flight flight);
    Task<bool> ExistsAsync(string flightNumber, DateTime departureDate);
}

public interface IPassengerRepository
{
    Task<Passenger?> GetByIdAsync(Guid id);
    Task<Passenger?> GetByPassportAsync(string passportNumber);
    Task<Passenger?> GetByEmailAsync(string email);
    Task<IEnumerable<Passenger>> SearchAsync(string firstName, string lastName);
    Task AddAsync(Passenger passenger);
    Task UpdateAsync(Passenger passenger);
    Task<bool> ExistsByPassportAsync(string passportNumber);
    Task<bool> ExistsByEmailAsync(string email);
}

public interface IFlightBookingRepository
{
    Task<Entities.FlightBooking?> GetByIdAsync(Guid id);
    Task<Entities.FlightBooking?> GetByReferenceAsync(string bookingReference);
    Task<IEnumerable<Entities.FlightBooking>> GetByPassengerAsync(Guid passengerId);
    Task<IEnumerable<Entities.FlightBooking>> GetByFlightAsync(Guid flightId);
    Task<IEnumerable<Entities.FlightBooking>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task AddAsync(Entities.FlightBooking booking);
    Task UpdateAsync(Entities.FlightBooking booking);
    Task<bool> ExistsByReferenceAsync(string bookingReference);
    Task<int> GetBookingCountByFlightAsync(Guid flightId);
}
