using AirportSystem.Core.Enums;

namespace AirportSystem.Core.Entities;

public class Fare
{
    public string Name { get; init; } = string.Empty;
    public ServiceClass Class { get; init; }
    public double PriceMultiplier { get; init; }
    public int BaggageLimit { get; init; }
    public bool IsRefundable { get; init; }
}
