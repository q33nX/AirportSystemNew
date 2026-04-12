namespace AirportSystem.Core.Entities;

public class Aircraft
{
    public string Model { get; init; } = string.Empty;
    public Airline Owner { get; init; } = null!;
    public List<Seat> Seats { get; init; } = new();
}
