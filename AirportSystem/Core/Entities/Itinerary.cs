namespace AirportSystem.Core.Entities;

public class Itinerary
{
    // Список всех перелетов в этом маршруте
    public List<FlightInstance> Flights { get; init; } = new();

    // Общая стоимость всех сегментов
    public decimal TotalBasePrice => Flights.Sum(f => f.BasePrice);

    // Время вылета (самый первый рейс)
    public DateTime DepartureTime => Flights.Any() ? Flights.First().LocalDepartureTime : DateTime.MinValue;

    // Время прилета (самый последний рейс)
    public DateTime ArrivalTime => Flights.Any() ? Flights.Last().LocalArrivalTime : DateTime.MinValue;

    // Общее время в пути (от взлета первого до посадки последнего)
    public TimeSpan TotalDuration => ArrivalTime - DepartureTime;

    // Свойство для UI: Количество пересадок
    public int StopsCount => Flights.Count - 1;

    // Проверка на международный статус (у тебя уже была эта отличная логика)
    public bool IsInternational => Flights.Any(f =>
        f.Schema.Origin.City.Country != f.Schema.Destination.City.Country);

    // Проверка на самостоятельную пересадку
    public bool IsSelfTransfer => Flights.Select(f =>
        f.Schema.Carrier.IATACode).Distinct().Count() > 1;

    // Удобный метод для получения списка всех городов пересадок
    public List<string> GetTransferCities()
    {
        if (Flights.Count <= 1) return new List<string>();

        // Берем города назначения для всех рейсов, кроме последнего
        return Flights.Take(Flights.Count - 1)
                      .Select(f => f.Schema.Destination.City.Name)
                      .ToList();
    }
}
