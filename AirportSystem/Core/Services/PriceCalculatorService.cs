using AirportSystem.Core.Interfaces;
using AirportSystem.Core.Entities;
using AirportSystem.Core.Enums;

namespace AirportSystem.Core.Services;

public class PriceCalculatorService : IPriceCalculator
{
    // Твой базовый метод расчета (оставляем логику, меняем только сигнатуру под сущности)
    public decimal CalculateFlightPrice(FlightInstance flight, ServiceClass serviceClass, SeatLocation preference)
    {
        decimal multiplier = 1.0m;
        // Используем дату вылета из рейса и текущую дату как дату бронирования
        int daysToFlight = (flight.DepartureDate.Date - DateTime.Now.Date).Days;

        // 1. Твоя логика даты бронирования
        if (daysToFlight > 60) multiplier *= 0.8m;
        else if (daysToFlight < 7) multiplier *= 1.7m;
        else if (daysToFlight < 21) multiplier *= 1.2m;

        // 2. Твоя логика праздников
        if (IsPeakDate(flight.DepartureDate))
        {
            multiplier *= 1.4m;
        }

        // 3. Твой коэффициент класса обслуживания
        decimal classMultiplier = serviceClass switch
        {
            ServiceClass.Comfort => 1.3m,
            ServiceClass.Business => 2.5m,
            ServiceClass.First => 5.0m,
            _ => 1.0m
        };

        decimal finalPrice = flight.BasePrice * multiplier * classMultiplier;

        // 4. Добавляем новую фишку: наценка за опцию "у окна"
        if (preference == SeatLocation.Window)
        {
            finalPrice += 15.0m; // Фиксированная добавка за комфорт
        }

        return Math.Round(finalPrice, 2);
    }

    // Новый метод для расчета ВСЕГО бронирования (все рейсы * все пассажиры)
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
