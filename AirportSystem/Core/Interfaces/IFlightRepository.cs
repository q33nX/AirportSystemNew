using AirportSystem.Core.Entities;

namespace AirportSystem.Core.Interfaces;

public interface IFlightRepository
{
    Task<List<FlightInstance>> GetAllFlightsAsync();
    Task<FlightInstance?> GetFlightByIdAsync(Guid id);
    Task<List<Airport>> GetAirportsAsync();
}
