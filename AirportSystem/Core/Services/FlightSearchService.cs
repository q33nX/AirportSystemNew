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
        // 1. Жёсткое условие: если вообще всё пусто — выходим сразу
        if (from == null && to == null && !date.HasValue)
            return new List<Itinerary>();

        var allFlights = await _repository.GetAllFlightsAsync();
        if (allFlights == null) return new List<Itinerary>();

        var itineraries = new List<Itinerary>();

        // 2. Ищем прямые рейсы (адаптированный старый алгоритм)
        var directFlights = allFlights.Where(f =>
        {
            // Проверка на null внутри схемы
            if (f.Schema?.Origin?.City == null || f.Schema?.Destination?.City == null)
                return false;

            // Гибкое соответствие (если null — значит поле не учитываем)
            bool matchesOrigin = from == null ||
                f.Schema.Origin.City.IATACode.Equals(from.IATACode, StringComparison.OrdinalIgnoreCase);

            bool matchesDestination = to == null ||
                f.Schema.Destination.City.IATACode.Equals(to.IATACode, StringComparison.OrdinalIgnoreCase);

            bool matchesDate = !date.HasValue ||
                f.LocalDepartureTime.Date == date.Value.Date;

            return matchesOrigin && matchesDestination && matchesDate;
        }).ToList();

        // Добавляем прямые рейсы в результат
        foreach (var f in directFlights)
        {
            itineraries.Add(new Itinerary { Flights = new List<FlightInstance> { f } });
        }

        // 3. Рейсы с пересадкой ищем ТОЛЬКО если заданы оба города и дата
        // (Логически пересадки не имеют смысла при частичном поиске)
        if (from != null && to != null && date.HasValue)
        {
            var searchDate = date.Value.Date;

            var firstLegs = allFlights.Where(f =>
                f.Schema?.Origin?.City != null &&
                f.Schema.Origin.City.IATACode.Equals(from.IATACode, StringComparison.OrdinalIgnoreCase) &&
                f.LocalDepartureTime.Date == searchDate);

            foreach (var first in firstLegs)
            {
                var secondLegs = allFlights.Where(second =>
                    second.Schema?.Origin != null &&
                    second.Schema?.Destination?.City != null &&
                    second.Schema.Origin.Code == first.Schema.Destination.Code &&
                    second.Schema.Destination.City.IATACode.Equals(to.IATACode, StringComparison.OrdinalIgnoreCase) &&
                    // Стык: минимум MinTransferTime, максимум 24 часа
                    second.LocalDepartureTime >= first.LocalArrivalTime.AddMinutes(first.Schema.Destination.MinTransferTime) &&
                    second.LocalDepartureTime <= first.LocalArrivalTime.AddHours(24)
                );

                foreach (var second in secondLegs)
                {
                    itineraries.Add(new Itinerary
                    {
                        Flights = new List<FlightInstance> { first, second }
                    });
                }
            }
        }

        return itineraries.OrderBy(i => i.TotalBasePrice).ToList();
    }

    public async Task<List<Itinerary>> SearchItinerariesAsync(City from, City to, DateTime? departureDate, DateTime? returnDate)
    {
        // Поиск "Туда"
        var outboundOptions = await SearchItinerariesAsync(from, to, departureDate);

        // Если обратной даты нет — возвращаем только "Туда"
        if (!returnDate.HasValue) return outboundOptions;

        // Поиск "Обратно"
        var inboundOptions = await SearchItinerariesAsync(to, from, returnDate);
        var roundTripResults = new List<Itinerary>();

        foreach (var outbound in outboundOptions)
        {
            foreach (var inbound in inboundOptions)
            {
                // Проверка: вылет обратно должен быть строго после прилета "туда"
                // Используем ArrivalTime (время прилета последнего сегмента первого билета)
                if (inbound.DepartureTime > outbound.ArrivalTime.AddHours(1))
                {
                    roundTripResults.Add(new Itinerary
                    {
                        // Склеиваем списки рейсов в один маршрут
                        Flights = outbound.Flights.Concat(inbound.Flights).ToList()
                    });
                }
            }
        }
        return roundTripResults.OrderBy(i => i.TotalBasePrice).ToList();
    }

    // Фильтры теперь тоже работают с Itinerary!
    public List<Itinerary> ApplyFilters(List<Itinerary> allItineraries, FlightFilterModel filters)
    {
        var query = allItineraries.AsEnumerable();

        // 1. Цена (используем общую цену всей коробки/маршрута)
        query = query.Where(i => i.TotalBasePrice <= filters.MaxPrice);

        // 2. Авиакомпании
        // Если в маршруте есть хотя бы один рейс от выбранной авиакомпании — оставляем
        if (filters.SelectedAirlines.Any())
        {
            var selectedCodes = filters.SelectedAirlines.Select(a => a.IATACode).ToHashSet();
            query = query.Where(i => i.Flights.Any(f => selectedCodes.Contains(f.Schema.Carrier.IATACode)));
        }

        // 3. Дни недели (смотрим по ПЕРВОМУ рейсу в маршруте)
        if (filters.SelectedDays.Any())
        {
            query = query.Where(i => filters.SelectedDays.Contains(i.Flights.First().LocalDepartureTime.DayOfWeek));
        }

        // 4. Время суток (сложная логика с Morning/Day/Evening/Night)
        // Проверяем время вылета ПЕРВОГО сегмента
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

        // 5. Класс обслуживания
        // Тут важный момент: в маршруте Itinerary мы проверяем, чтобы ВСЕ рейсы 
        // поддерживали хотя бы один из выбранных классов.
        if (filters.SelectedClasses.Any())
        {
            query = query.Where(i => i.Flights.All(f =>
                f.Aircraft.AvailableClasses.Overlaps(filters.SelectedClasses)));
        }

        //// 6. Количество пересадок (бонус, если нужно)
        //if (filters.MaxTransfers.HasValue)
        //{
        //    query = query.Where(i => (i.Flights.Count - 1) <= filters.MaxTransfers.Value);
        //}

        return query.ToList();
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
