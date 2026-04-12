using AirportSystem.Core.Entities;

namespace AirportSystem.Core.Services;

public class BookingStateService
{
    // Хранит выбранный маршрут (Itinerary)
    public Itinerary? SelectedItinerary { get; set; }
}
