using AirportSystem.Core.Enums;
using AirportSystem.Core.Entities;

namespace AirportSystem.Core.Interfaces;

public interface IPriceCalculator
{
    // Расчет для одного пассажира на одном рейсе
    decimal CalculateFlightPrice(FlightInstance flight, ServiceClass serviceClass, SeatLocation preference);

    // Расчет для всей группы на всем маршруте
    decimal CalculateTotalItineraryPrice(Itinerary itinerary, int passengerCount, ServiceClass serviceClass, SeatLocation preference);
}
