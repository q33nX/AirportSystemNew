using AirportSystem.Core.Enums;

namespace AirportSystem.Core.Entities;

public class Seat
{
    public string Id => $"{Row}{Letter}";
    public int Row { get; init; }
    public char Letter { get; init; }
    public ServiceClass Class { get; init; }
    public SeatLocation Location { get; init; }
    public bool HasExtraLegroom { get; init; }
}
