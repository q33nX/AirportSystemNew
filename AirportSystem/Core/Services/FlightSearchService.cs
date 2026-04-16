using AirportSystem.Core.Entities;
using AirportSystem.Core.Interfaces;
using AirportSystem.Core.ViewModels;
using AirportSystem.Core.Enums;

namespace AirportSystem.Core.Services;

public class FlightSearchService : IFlightSearchService
{
    private readonly IFlightRepository _repository;

    public FlightSearchService(IFlightRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<Itinerary>> SearchItinerariesAsync(City from, City to, DateTime? date)
    {
        var allFlights = await _repository.GetAllFlightsAsync();
        if (allFlights == null) return new List<Itinerary>();

        var itineraries = new List<Itinerary>();
        var usedKeys = new HashSet<string>();

        // Определяем режим поиска
        var hasFrom = from != null && !string.IsNullOrEmpty(from.IATACode);
        var hasTo = to != null && !string.IsNullOrEmpty(to.IATACode);
        
        // Если нет ни одного города - не ищем
        if (!hasFrom && !hasTo)
            return new List<Itinerary>();

        // Прямые рейсы
        var directFlights = allFlights.Where(f => 
            f?.Schema != null &&
            f.Schema.Origin?.City != null &&
            f.Schema.Destination?.City != null &&
            (!hasFrom || MatchesCity(f.Schema.Origin.City, from)) &&
            (!hasTo || MatchesCity(f.Schema.Destination.City, to)) &&
            MatchesDate(f, date));

        foreach (var f in directFlights)
        {
            var key = f.Id.ToString();
            if (usedKeys.Add(key))
            {
                itineraries.Add(new Itinerary { Flights = new List<FlightInstance> { f } });
            }
        }

        // Рейсы с пересадкой - только если есть оба города И дата
        if (hasFrom && hasTo && date.HasValue)
        {
            var searchDate = date.Value.Date;
            var firstLegs = allFlights.Where(f =>
                f?.Schema?.Origin?.City != null &&
                MatchesCity(f.Schema.Origin.City, from) &&
                f.LocalDepartureTime.Date == searchDate);

            foreach (var first in firstLegs)
            {
                var secondLegs = allFlights.Where(second =>
                    second?.Schema != null &&
                    second.Schema.Origin != null &&
                    second.Schema.Destination?.City != null &&
                    second.Schema.Origin.Code.Equals(first.Schema.Destination.Code, StringComparison.OrdinalIgnoreCase) &&
                    MatchesCity(second.Schema.Destination.City, to) &&
                    second.ActualDepartureTime >= first.ActualArrivalTime.AddMinutes(first.Schema.Destination.MinTransferTime) &&
                    second.ActualDepartureTime <= first.ActualArrivalTime.AddHours(24) &&
                    second.LocalDepartureTime.Date <= searchDate.AddDays(1));

                foreach (var second in secondLegs)
                {
                    var key = $"{first.Id}_{second.Id}";
                    if (usedKeys.Add(key))
                    {
                        itineraries.Add(new Itinerary { Flights = new List<FlightInstance> { first, second } });
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

        var roundTripResults = outboundOptions
            .SelectMany(outbound => inboundOptions
                .Where(inbound => inbound.DepartureTime > outbound.ArrivalTime.AddHours(1))
                .Select(inbound => new Itinerary
                {
                    Flights = outbound.Flights.Concat(inbound.Flights).ToList()
                }))
            .OrderBy(i => i.TotalBasePrice)
            .ToList();

        return roundTripResults;
    }

    public async Task<List<Itinerary>> SearchMultiCityAsync(List<(City From, City To, DateTime? Date)> segments)
    {
        if (segments == null || segments.Count < 2)
            return new List<Itinerary>();

        var allFlights = await _repository.GetAllFlightsAsync();
        if (allFlights == null) return new List<Itinerary>();

        var firstSegment = segments[0];
        
        if (firstSegment.From == null || firstSegment.To == null || !firstSegment.Date.HasValue)
            return new List<Itinerary>();

        var firstSegmentResults = await SearchItinerariesAsync(firstSegment.From, firstSegment.To, firstSegment.Date);

        if (!firstSegmentResults.Any()) return new List<Itinerary>();

        for (int i = 1; i < segments.Count; i++)
        {
            var prevSegment = segments[i - 1];
            var currentSegment = segments[i];

            if (currentSegment.From == null || currentSegment.To == null || !currentSegment.Date.HasValue)
                return new List<Itinerary>();

            var minDate = prevSegment.Date?.Date;

            var currentSegmentResults = await SearchItinerariesAsync(currentSegment.From, currentSegment.To, currentSegment.Date);

            var validResults = firstSegmentResults
                .SelectMany(firstItinerary =>
                    currentSegmentResults
                        .Where(currentItinerary => 
                            currentItinerary.DepartureTime.Date >= minDate &&
                            currentItinerary.DepartureTime > firstItinerary.ArrivalTime.AddHours(1))
                        .Select(currentItinerary => new Itinerary
                        {
                            Flights = firstItinerary.Flights.Concat(currentItinerary.Flights).ToList()
                        }))
                .ToList();

            if (!validResults.Any()) return new List<Itinerary>();
            
            firstSegmentResults = validResults;
        }

        return firstSegmentResults.OrderBy(i => i.TotalBasePrice).ToList();
    }

    public List<Itinerary> ApplyFilters(List<Itinerary> allItineraries, FlightFilterModel filters)
    {
        var query = allItineraries.AsEnumerable();

        query = query.Where(i => i.TotalBasePrice <= filters.MaxPrice);

        if (filters.AllowedStops.Any())
        {
            query = query.Where(i => filters.AllowedStops.Contains(i.StopsCount));
        }

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
            query = query.Where(i => IsTimeSlotMatch(i, filters.SelectedTimeSlots));
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

    private static bool MatchesCity(City flightCity, City searchCity)
    {
        if (searchCity == null || string.IsNullOrEmpty(searchCity.IATACode))
            return false;
        return flightCity.IATACode.Equals(searchCity.IATACode, StringComparison.OrdinalIgnoreCase);
    }

    private static bool MatchesDate(FlightInstance f, DateTime? date)
    {
        if (!date.HasValue)
            return true;
        return f.LocalDepartureTime.Date == date.Value.Date;
    }

    private static bool IsTimeSlotMatch(Itinerary itinerary, List<string> selectedTimeSlots)
    {
        int hour = itinerary.Flights.First().LocalDepartureTime.Hour;
        return (selectedTimeSlots.Contains("Morning") && hour >= 6 && hour < 12) ||
               (selectedTimeSlots.Contains("Day") && hour >= 12 && hour < 18) ||
               (selectedTimeSlots.Contains("Evening") && hour >= 18 && hour < 24) ||
               (selectedTimeSlots.Contains("Night") && (hour >= 0 && hour < 6));
    }
}
