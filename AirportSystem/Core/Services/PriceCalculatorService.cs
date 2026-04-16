using AirportSystem.Core.Interfaces;
using AirportSystem.Core.Entities;
using AirportSystem.Core.Enums;
using AirportSystem.Core.Constants;

namespace AirportSystem.Core.Services;

public class PriceCalculatorService : IPriceCalculator
{
    public decimal CalculateFlightPrice(FlightInstance flight, ServiceClass serviceClass, SeatLocation preference)
    {
        decimal multiplier = 1.0m;
        int daysToFlight = (flight.DepartureDate.Date - DateTime.Now.Date).Days;

        // Динамика цены в зависимости от дней до вылета
        if (daysToFlight > 60)
            multiplier *= PricingConstants.EarlyBookingDiscount;
        else if (daysToFlight < 7)
            multiplier *= PricingConstants.UrgentBookingMultiplier;
        else if (daysToFlight < 21)
            multiplier *= PricingConstants.ShortTermMultiplier;

        // Сезонные наценки
        if (IsPeakDate(flight.DepartureDate))
            multiplier *= PricingConstants.PeakDateMultiplier;

        // Коэффициент класса обслуживания
        decimal classMultiplier = serviceClass switch
        {
            ServiceClass.Comfort => PricingConstants.ComfortClassMultiplier,
            ServiceClass.Business => PricingConstants.BusinessClassMultiplier,
            ServiceClass.First => PricingConstants.FirstClassMultiplier,
            _ => 1.0m
        };

        decimal finalPrice = flight.BasePrice * multiplier * classMultiplier;

        // Наценка за место у окна
        if (preference == SeatLocation.Window)
            finalPrice += PricingConstants.WindowSeatSurcharge;

        return Math.Round(finalPrice, 2);
    }

    public decimal CalculateTotalItineraryPrice(Itinerary itinerary, int passengerCount, ServiceClass serviceClass, SeatLocation preference)
    {
        decimal total = 0;
        foreach (var flight in itinerary.Flights)
        {
            total += CalculateFlightPrice(flight, serviceClass, preference);
        }
        return total * passengerCount;
    }

    private bool IsPeakDate(DateTime date)
    {
        bool isNewYear = (date.Month == 12 && date.Day >= 20) || (date.Month == 1 && date.Day <= 10);
        bool isMayHolidays = (date.Month == 5 && date.Day <= 10);
        bool isSummerHighSeason = (date.Month >= 6 && date.Month <= 8);

        return isNewYear || isMayHolidays || isSummerHighSeason;
    }
}
