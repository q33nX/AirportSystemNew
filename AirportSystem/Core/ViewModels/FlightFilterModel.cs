using AirportSystem.Core.Entities;
using AirportSystem.Core.Enums;

namespace AirportSystem.Core.ViewModels;

public class FlightFilterModel
{
    public List<int> AllowedStops { get; set; } = new();
    public decimal MaxPrice { get; set; } = 250000;
    public List<Airline> SelectedAirlines { get; set; } = new();
    public List<DayOfWeek> SelectedDays { get; set; } = new();
    public List<ServiceClass> SelectedClasses { get; set; } = new();
    public List<string> SelectedTimeSlots { get; set; } = new();
}