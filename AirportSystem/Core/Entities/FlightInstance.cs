using AirportSystem.Core.Enums;

namespace AirportSystem.Core.Entities;

public class FlightInstance
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public FlightSchema Schema { get; init; } = null!;

    // Это дата изначального планирования (например, 15.05.2026)
    public DateTime DepartureDate { get; init; }

    // НОВОЕ ПОЛЕ: Если рейс задержан или перенесен, записываем новое время сюда.
    // Если null — значит летим строго по расписанию (Schema.DepartureTime).
    public DateTime? OverriddenDepartureTime { get; set; }
    public DateTime? OverriddenArrivalTime { get; set; }

    // Удобное свойство для UI и поиска
    public DateTime ActualDepartureTime =>
        OverriddenDepartureTime ?? DepartureDate.Add(Schema.DepartureTime);

    public DateTime ActualArrivalTime =>
        OverriddenArrivalTime ?? DepartureDate.Add(Schema.DepartureTime).Add(Schema.ArrivalOffset);

    public TimeSpan ActualDuration => ActualArrivalTime - ActualDepartureTime;

    public Aircraft Aircraft { get; init; } = null!;
    public decimal BasePrice { get; set; }
    public FlightStatus Status { get; set; } = FlightStatus.Scheduled;
    public HashSet<string> OccupiedSeatIds { get; init; } = new();

    public bool IsSeatAvailable(string seatId) => !OccupiedSeatIds.Contains(seatId);
}
