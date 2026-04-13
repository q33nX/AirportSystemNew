using AirportSystem.Core.Enums;

namespace AirportSystem.Core.Entities;

public class Aircraft
{
    public string Model { get; init; } = string.Empty;
    public Airline Owner { get; init; } = null!;
    public List<Seat> Seats { get; init; } = new();

    public HashSet<ServiceClass> AvailableClasses { get; set; } = new();
}
