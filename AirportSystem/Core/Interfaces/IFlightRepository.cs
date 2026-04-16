using AirportSystem.Core.Entities;

namespace AirportSystem.Core.Interfaces;

/// <summary>
/// Репозиторий для доступа к данным о рейсах и аэропортах
/// </summary>
public interface IFlightRepository
{
    /// <summary>Получить все рейсы</summary>
    Task<List<FlightInstance>> GetAllFlightsAsync();

    /// <summary>Получить рейс по ID</summary>
    Task<FlightInstance?> GetFlightByIdAsync(Guid id);

    /// <summary>Получить все аэропорты</summary>
    Task<List<Airport>> GetAirportsAsync();
}
