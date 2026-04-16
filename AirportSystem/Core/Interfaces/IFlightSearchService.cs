using AirportSystem.Core.Entities;
using AirportSystem.Core.ViewModels;

namespace AirportSystem.Core.Interfaces;

public interface IFlightSearchService
{
    Task<List<Itinerary>> SearchItinerariesAsync(City from, City to, DateTime? date);
    Task<List<Itinerary>> SearchItinerariesAsync(City from, City to, DateTime? departureDate, DateTime? returnDate);
    Task<List<Itinerary>> SearchMultiCityAsync(List<(City From, City To, DateTime? Date)> segments);
    List<Itinerary> ApplyFilters(List<Itinerary> allItineraries, FlightFilterModel filters);
    List<Itinerary> MarkTopFlights(List<Itinerary> itineraries);
    Task<List<FlightInstance>> GetAvailableFlightsAsync();
    Task<List<City>> GetAvailableCitiesAsync();
}
