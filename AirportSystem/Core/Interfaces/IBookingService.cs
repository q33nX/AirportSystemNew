using AirportSystem.Core.Entities;
using AirportSystem.Core.Enums;

namespace AirportSystem.Core.Interfaces;

public interface IBookingService
{
    // Принимаем список пассажиров и их предпочтения по местам
    Task<Booking> CreateBookingAsync(
        Itinerary itinerary,
        List<Passenger> passengers,
        string email,
        ServiceClass serviceClass,
        SeatLocation seatPreference);

    Task<Booking?> GetBookingByPnrAsync(string pnr);

    Task CancelBookingAsync(string pnr);
}
