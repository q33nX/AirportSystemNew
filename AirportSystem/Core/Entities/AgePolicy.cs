using AirportSystem.Core.Enums;

namespace AirportSystem.Core.Entities;

public class AgePolicy
{
    public PassengerCategory Category { get; init; }
    public double DiscountMultiplier { get; init; }
    public bool RequiresSeat { get; init; }
}
