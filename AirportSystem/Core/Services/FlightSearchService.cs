using AirportSystem.Core.Entities;
using AirportSystem.Core.Interfaces;

namespace AirportSystem.Core.Services;

public class FlightSearchService
{
    private readonly IFlightRepository _repository;
    private readonly IPriceCalculator _priceCalculator;

    public FlightSearchService(IFlightRepository repository, IPriceCalculator priceCalculator)
    {
        _repository = repository;
        _priceCalculator = priceCalculator;
    }

    public async Task<List<FlightInstance>> SearchFlights(string fromCity, string toCity, DateTime date)
    {
        var allFlights = await _repository.GetAllFlightsAsync();

        // Фильтрация по городам и дате
        return allFlights.Where(f =>
            f.Schema.Origin.City.Name.Contains(fromCity, StringComparison.OrdinalIgnoreCase) &&
            f.Schema.Destination.City.Name.Contains(toCity, StringComparison.OrdinalIgnoreCase) &&
            f.DepartureDate.Date == date.Date
        ).ToList();
    }

    public async Task<List<FlightInstance>> GetAvailableFlightsAsync()
    {
        // Здесь в будущем можно добавить фильтрацию по статусу "Активен", 
        // сортировку по цене или логику кеширования
        var flights = await _repository.GetAllFlightsAsync();
        return flights.ToList();
    }

    public async Task<List<City>> GetAvailableCitiesAsync()
    {
        var airports = await _repository.GetAirportsAsync();
        return airports
            .Select(a => a.City)
            .Distinct()
            .OrderBy(c => c.Name)
            .ToList();
    }
}
