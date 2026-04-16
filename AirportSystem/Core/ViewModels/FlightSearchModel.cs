using AirportSystem.Core.Entities;

namespace AirportSystem.Core.ViewModels
{
    public class FlightSearchModel
    {
        public City? DepartureCity { get; set; } = new ();
        public City? ArrivalCity { get; set; } = new ();
        public DateTime? DepartureDate { get; set; } = DateTime.Now;
    }
}