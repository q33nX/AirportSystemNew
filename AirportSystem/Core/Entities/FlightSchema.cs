namespace AirportSystem.Core.Entities;

public class FlightSchema
{
    public string FlightNumber { get; init; } = string.Empty;
    public Airline Carrier { get; init; } = null!;
    public Airport Origin { get; init; } = null!;
    public Airport Destination { get; init; } = null!;
    public TimeSpan DepartureTime { get; init; }
    public TimeSpan ArrivalOffset { get; init; }
    public List<DayOfWeek> OperatingDays { get; init; } = new();
}
