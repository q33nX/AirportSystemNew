using AirportSystem.Core.Enums;

namespace AirportSystem.Core.Entities;

public class Booking
{
    public string PNR { get; init; } = string.Empty;
    public Itinerary SelectedItinerary { get; init; } = null!;
    public List<Passenger> Passengers { get; init; } = new();
    public BookingStatus Status { get; private set; } = BookingStatus.Active;
    public string ContactEmail { get; set; } = string.Empty;

    public void Cancel() => Status = BookingStatus.Cancelled;
}
