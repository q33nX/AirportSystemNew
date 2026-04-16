using AirportSystem.Core.Enums;

namespace AirportSystem.Core.Entities;

public class FlightInstance
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public FlightSchema Schema { get; init; } = null!;

    // Это дата изначального планирования (например, 15.05.2026) — хранит только дату
    public DateTime DepartureDate { get; init; }

    // НОВОЕ ПОЛЕ: Если рейс задержан или перенесен, записываем новое время сюда.
    // Если null — значит летим строго по расписанию (Schema.DepartureTime).
    public DateTime? OverriddenDepartureTime { get; set; }
    public DateTime? OverriddenArrivalTime { get; set; }

    // Удобное свойство для UI и поиска
    public DateTime ActualDepartureTime =>
        OverriddenDepartureTime ?? DepartureDate.Add(Schema.DepartureTime);

    public DateTime ActualArrivalTime =>
        OverriddenArrivalTime ?? ActualDepartureTime.Add(Schema.ArrivalOffset);

    // Исправлено: длительность вычисляется с учётом фактических (возможно переопределённых) времён
    public TimeSpan ActualDuration => ActualArrivalTime - ActualDepartureTime;

    // Время вылета по часам аэропорта отправления
    public DateTime LocalDepartureTime =>
        ActualDepartureTime.AddHours(Schema.Origin.TimezoneOffset);

    // Время прилета по часам аэропорта прибытия
    public DateTime LocalArrivalTime =>
        ActualArrivalTime.AddHours(Schema.Destination.TimezoneOffset);

    // Полезно для UI: Проверка, прилетаем ли мы на следующий день (относительно местного времени вылета)
    public bool ArrivesNextDay => LocalArrivalTime.Date > LocalDepartureTime.Date;

    public Aircraft Aircraft { get; init; } = null!;
    public decimal BasePrice { get; set; }
    public FlightStatus Status { get; set; } = FlightStatus.Scheduled;
    public HashSet<string> OccupiedSeatIds { get; init; } = new();

    private readonly object _seatLock = new();

    public bool IsSeatAvailable(string seatId) => !OccupiedSeatIds.Contains(seatId);

    public bool TryReserveSeat(string seatId)
    {
        lock (_seatLock)
        {
            if (OccupiedSeatIds.Contains(seatId)) return false;
            OccupiedSeatIds.Add(seatId);
            return true;
        }
    }

    public bool ReleaseSeat(string seatId)
    {
        lock (_seatLock)
        {
            return OccupiedSeatIds.Remove(seatId);
        }
    }
}
