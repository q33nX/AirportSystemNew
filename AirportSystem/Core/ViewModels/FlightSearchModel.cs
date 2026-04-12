namespace AirportSystem.Core.ViewModels
{
    public class FlightSearchModel
    {
        public string DepartureCity { get; set; } = string.Empty;
        public string ArrivalCity { get; set; } = string.Empty;
        public DateTime? FlightDate { get; set; } = DateTime.Now;
    }
}