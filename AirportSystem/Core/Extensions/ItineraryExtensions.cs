using AirportSystem.Core.Entities;
using AirportSystem.Core.ViewModels;

namespace AirportSystem.Core.Extensions;

public static class ItineraryExtensions
{
    /// <summary>
    /// Проверяет, соответствует ли маршрут критериям фильтра
    /// </summary>
    public static bool MatchesFilter(this Itinerary itinerary, FlightFilterModel filter)
    {
        if (itinerary.TotalBasePrice > filter.MaxPrice)
            return false;

        if (filter.AllowedStops.Any() && !filter.AllowedStops.Contains(itinerary.StopsCount))
            return false;

        if (filter.SelectedAirlines.Any())
        {
            var airlineCodes = filter.SelectedAirlines.Select(a => a.IATACode).ToHashSet(StringComparer.OrdinalIgnoreCase);
            if (!itinerary.Flights.Any(f => airlineCodes.Contains(f.Schema.Carrier.IATACode)))
                return false;
        }

        if (filter.SelectedDays.Any())
        {
            var firstFlightDay = itinerary.Flights.First().LocalDepartureTime.DayOfWeek;
            if (!filter.SelectedDays.Contains(firstFlightDay))
                return false;
        }

        if (filter.SelectedTimeSlots.Any())
        {
            if (!ItMatchesTimeSlot(itinerary, filter.SelectedTimeSlots))
                return false;
        }

        if (filter.SelectedClasses.Any())
        {
            if (!itinerary.Flights.All(f => f.Aircraft.AvailableClasses.Overlaps(filter.SelectedClasses)))
                return false;
        }

        return true;
    }

    private static bool ItMatchesTimeSlot(Itinerary itinerary, List<string> selectedTimeSlots)
    {
        int hour = itinerary.Flights.First().LocalDepartureTime.Hour;
        return (selectedTimeSlots.Contains("Morning") && hour >= 6 && hour < 12) ||
               (selectedTimeSlots.Contains("Day") && hour >= 12 && hour < 18) ||
               (selectedTimeSlots.Contains("Evening") && hour >= 18 && hour < 24) ||
               (selectedTimeSlots.Contains("Night") && (hour >= 0 && hour < 6));
    }
}
