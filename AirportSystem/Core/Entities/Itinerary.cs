using AirportSystem.Core.Enums;

namespace AirportSystem.Core.Entities;

public class Itinerary
{
    // Список всех перелетов в этом маршруте
    public List<FlightInstance> Flights { get; init; } = new();

    // Флаги для топ-3 рейсов
    public bool IsCheapest { get; set; }
    public bool IsFastest { get; set; }
    public bool IsMostComfortable { get; set; }

    // Общая стоимость всех сегментов
    public decimal TotalBasePrice => Flights.Sum(f => f.BasePrice);

    // Лучший класс обслуживания среди всех сегментов
    public ServiceClass BestServiceClass => Flights.Any(f => f.Aircraft?.AvailableClasses != null)
        ? Flights.Max(f => f.Aircraft.AvailableClasses.Max(c => c))
        : ServiceClass.Economy;

    // Время вылета (самый первый рейс) - по UTC
    public DateTime DepartureTime => Flights.Any() ? Flights.First().ActualDepartureTime : DateTime.MinValue;

    // Время прилета (самый последний рейс) - по UTC
    public DateTime ArrivalTime => Flights.Any() ? Flights.Last().ActualArrivalTime : DateTime.MinValue;

    // Общее время в пути (от взлета первого до посадки последнего)
    public TimeSpan TotalDuration => ArrivalTime - DepartureTime;

    // Свойство для UI: Количество пересадок
    public int StopsCount => Flights.Count - 1;

    // Максимальный уровень комфорта среди всех сегментов (0=Economy, 1=Comfort, 2=Business, 3=First)
    public int MaxComfortLevel => Flights.Any(f => f.Aircraft?.AvailableClasses != null)
        ? Flights.Max(f => f.Aircraft.AvailableClasses.Max(c => (int)c))
        : 0;

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
