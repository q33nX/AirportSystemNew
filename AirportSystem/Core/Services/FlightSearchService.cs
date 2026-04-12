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

    public async Task<List<FlightInstance>> SearchFlights(City from, City to, DateTime? date)
    {
        // Жёсткое условие: если вообще всё пусто — выходим сразу
        if (from == null && to == null && !date.HasValue)
        {
            return new List<FlightInstance>();
        }

        var allFlights = await _repository.GetAllFlightsAsync();

        if (allFlights == null) return new List<FlightInstance>();

        return allFlights.Where(f =>
        {
            // Проверка навигации (чтобы цепочка f.Schema... не выдала NullReference)
            if (f.Schema?.Origin?.City == null || f.Schema?.Destination?.City == null)
                return false;

            // Если город вылета не задан — true (пропускаем). Если задан — сравниваем IATA.
            bool matchesOrigin = from == null ||
                f.Schema.Origin.City.IATACode.Equals(from.IATACode, StringComparison.OrdinalIgnoreCase);

            // Аналогично для города прибытия
            bool matchesDestination = to == null ||
                f.Schema.Destination.City.IATACode.Equals(to.IATACode, StringComparison.OrdinalIgnoreCase);

            // Если дата не задана — true. Если задана — сравниваем только день/месяц/год.
            bool matchesDate = !date.HasValue || f.DepartureDate.Date == date.Value.Date;

            // Рейс подходит, если он соответствует всем ЗАПОЛНЕННЫМ полям
            return matchesOrigin && matchesDestination && matchesDate;
        }).ToList();
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
