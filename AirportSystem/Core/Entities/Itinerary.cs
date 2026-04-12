namespace AirportSystem.Core.Entities;

public class Itinerary
{
    public List<FlightInstance> Flights { get; init; } = new();

    public decimal TotalBasePrice => Flights.Sum(f => f.BasePrice);

    public bool IsInternational => Flights.Any(f =>
        f.Schema.Origin.City.Country != f.Schema.Destination.City.Country);

    public bool IsSelfTransfer => Flights.Select(f =>
        f.Schema.Carrier.IATACode).Distinct().Count() > 1;
}
