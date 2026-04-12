namespace AirportSystem.Core.Entities;

public class Airport
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public City City { get; init; } = null!;
    public int TimezoneOffset { get; init; }
    public int MinTransferTime { get; init; }
}
