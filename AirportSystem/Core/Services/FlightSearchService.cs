using AirportSystem.Core.Entities;
using AirportSystem.Core.Interfaces;
using AirportSystem.Core.ViewModels;
using AirportSystem.Core.Enums;

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

    public async Task<List<Itinerary>> SearchItinerariesAsync(City from, City to, DateTime? date)
    {
        if (from == null && to == null && !date.HasValue)
            return new List<Itinerary>();                                                       

        var allFlights = await _repository.GetAllFlightsAsync();
        if (allFlights == null) return new List<Itinerary>();

        var itineraries = new List<Itinerary>();

        // Локальное множество ключей для дедупликации комбинаций
        var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // 2. Ищем прямые рейсы
        var directFlights = allFlights.Where(f =>
        {
            if (f?.Schema == null) return false;
            if (f.Schema.Origin?.City == null || f.Schema.Destination?.City == null) return false;

            bool matchesOrigin = from == null ||
                f.Schema.Origin.City.IATACode.Equals(from.IATACode, StringComparison.OrdinalIgnoreCase);

            bool matchesDestination = to == null ||
                f.Schema.Destination.City.IATACode.Equals(to.IATACode, StringComparison.OrdinalIgnoreCase);

            bool matchesDate = !date.HasValue ||
                f.LocalDepartureTime.Date == date.Value.Date;

            return matchesOrigin && matchesDestination && matchesDate;
        }).ToList();

        // Добавляем прямые рейсы в результат (с дедупликацией)
        foreach (var f in directFlights)
        {
            var key = f.Id.ToString();
            if (keys.Add(key))
            {
                itineraries.Add(new Itinerary { Flights = new List<FlightInstance> { f } });
            }
        }

        // 3. Рейсы с пересадкой ищем ТОЛЬКО если заданы оба города и дата
        if (from != null && to != null && date.HasValue)
        {
            var searchDate = date.Value.Date;

            var firstLegs = allFlights.Where(f =>
                f?.Schema != null &&
                f.Schema.Origin?.City != null &&
                f.Schema.Origin.City.IATACode.Equals(from.IATACode, StringComparison.OrdinalIgnoreCase) &&
                f.LocalDepartureTime.Date == searchDate);

            foreach (var first in firstLegs)
            {
                var secondLegs = allFlights.Where(second =>
                    second?.Schema != null &&
                    second.Schema.Origin != null &&
                    second.Schema.Destination?.City != null &&
                    // сравниваем коды аэропортов нечувствительно к регистру
                    second.Schema.Origin.Code.Equals(first.Schema.Destination.Code, StringComparison.OrdinalIgnoreCase) &&
                    second.Schema.Destination.City.IATACode.Equals(to.IATACode, StringComparison.OrdinalIgnoreCase) &&
                    // Стык: минимум MinTransferTime, максимум 24 часа
                    second.LocalDepartureTime >= first.LocalArrivalTime.AddMinutes(first.Schema.Destination.MinTransferTime) &&
                    second.LocalDepartureTime <= first.LocalArrivalTime.AddHours(24)
                );

                foreach (var second in secondLegs)
                {
                    var key = $"{first.Id}_{second.Id}";
                    if (keys.Add(key))
                    {
                        itineraries.Add(new Itinerary
                        {
                            Flights = new List<FlightInstance> { first, second }
                        });
                    }
                }
            }
        }

        return itineraries.OrderBy(i => i.TotalBasePrice).ToList();
    }

    public async Task<List<Itinerary>> SearchItinerariesAsync(City from, City to, DateTime? departureDate, DateTime? returnDate)
    {
        var outboundOptions = await SearchItinerariesAsync(from, to, departureDate);

        if (!returnDate.HasValue) return outboundOptions;

        var inboundOptions = await SearchItinerariesAsync(to, from, returnDate);
        var roundTripResults = new List<Itinerary>();

        foreach (var outbound in outboundOptions)
        {
            foreach (var inbound in inboundOptions)
            {
                if (inbound.DepartureTime > outbound.ArrivalTime.AddHours(1))
                {
                    roundTripResults.Add(new Itinerary
                    {
                        Flights = outbound.Flights.Concat(inbound.Flights).ToList()
                    });
                }
            }
        }
        return roundTripResults.OrderBy(i => i.TotalBasePrice).ToList();
    }

    // Фильтры
    public List<Itinerary> ApplyFilters(List<Itinerary> allItineraries, FlightFilterModel filters)
    {
        var query = allItineraries.AsEnumerable();

        query = query.Where(i => i.TotalBasePrice <= filters.MaxPrice);

        if (filters.SelectedAirlines.Any())
        {
            var selectedCodes = filters.SelectedAirlines.Select(a => a.IATACode).ToHashSet(StringComparer.OrdinalIgnoreCase);
            query = query.Where(i => i.Flights.Any(f => selectedCodes.Contains(f.Schema.Carrier.IATACode)));
        }

        if (filters.SelectedDays.Any())
        {
            query = query.Where(i => filters.SelectedDays.Contains(i.Flights.First().LocalDepartureTime.DayOfWeek));
        }

        if (filters.SelectedTimeSlots.Any())
        {
            query = query.Where(i =>
            {
                int hour = i.Flights.First().LocalDepartureTime.Hour;
                return (filters.SelectedTimeSlots.Contains("Morning") && hour >= 6 && hour < 12) ||
                       (filters.SelectedTimeSlots.Contains("Day") && hour >= 12 && hour < 18) ||
                       (filters.SelectedTimeSlots.Contains("Evening") && hour >= 18 && hour < 24) ||
                       (filters.SelectedTimeSlots.Contains("Night") && (hour >= 0 && hour < 6));
            });
        }

        if (filters.SelectedClasses.Any())
        {
            query = query.Where(i => i.Flights.All(f =>
                f.Aircraft.AvailableClasses.Overlaps(filters.SelectedClasses)));
        }

        return query.ToList();
    }

    public async Task<List<FlightInstance>> GetAvailableFlightsAsync()
    {
        var flights = await _repository.GetAllFlightsAsync();
        return flights.ToList();
    }

    public async Task<List<City>> GetAvailableCitiesAsync()
    {
        var airports = await _repository.GetAirportsAsync();
        return airports
            .Select(a => a.City)
            .GroupBy(c => c.IATACode, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .OrderBy(c => c.Name)
            .ToList();
    }
}
