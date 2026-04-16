using AirportSystem.Core.Entities;
using AirportSystem.Core.ViewModels;
using AirportSystem.Core.Enums;
using AirportSystem.Core.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace AirportSystem.Components.Pages
{
    public partial class Home
    {
        [Inject]
        private IJSRuntime? JSRuntime { get; set; }

        private string GetClassDescription(ServiceClass sClass) => sClass switch
        {
            ServiceClass.Economy => "Стандарт",
            ServiceClass.Comfort => "Комфорт",
            ServiceClass.Business => "Бизнес",
            ServiceClass.First => "Первый класс",
            _ => ""
        };

        private enum SearchMode { Roundtrip, MultiCity }

        private SearchMode _currentMode = SearchMode.Roundtrip;

        private List<FlightSearchModel> Segments = new()
        {
            new FlightSearchModel(),
            new FlightSearchModel()
        };

        private void SetSearchMode(SearchMode mode)
        {
            _currentMode = mode;
        }

        private void AddSegment()
        {
            if (Segments.Count < 5)
            {
                Segments.Add(new FlightSearchModel());
                UpdateSegmentMinDates();
            }
        }

        private void RemoveSegment(FlightSearchModel segment)
        {
            Segments.Remove(segment);
            UpdateSegmentMinDates();
        }

        private void OnSegmentDateChanged(FlightSearchModel segment, DateTime? date)
        {
            segment.DepartureDate = date;
            UpdateSegmentMinDates();
        }

        private void UpdateSegmentMinDates()
        {
            for (int i = 0; i < Segments.Count; i++)
            {
                if (i == 0)
                {
                    Segments[i].MinDate = DateTime.Now.Date;
                }
                else
                {
                    var prevDate = Segments[i - 1].DepartureDate?.Date;
                    Segments[i].MinDate = prevDate ?? DateTime.Now.Date;
                }
            }
        }

        private async Task SaveRouteToPdf()
        {
            if (_selectedItinerary == null || JSRuntime == null) return;

            var firstFlight = _selectedItinerary.Flights.First();
            var lastFlight = _selectedItinerary.Flights.Last();

            var ticketData = new TicketData
            {
                PNR = GeneratePnr(),
                OriginCity = firstFlight.Schema.Origin.City.Name,
                OriginCode = firstFlight.Schema.Origin.Code,
                DestinationCity = lastFlight.Schema.Destination.City.Name,
                DestinationCode = lastFlight.Schema.Destination.Code,
                DepartureDate = firstFlight.LocalDepartureTime,
                ArrivalDate = lastFlight.LocalArrivalTime,
                FlightNumber = firstFlight.Schema.FlightNumber,
                CarrierName = firstFlight.Schema.Carrier.Name,
                AircraftModel = firstFlight.Aircraft.Model,
                ServiceClass = _selectedServiceClass,
                AdultsCount = _adultsCount,
                ChildrenCount = _childrenCount,
                BasePrice = _selectedItinerary.TotalBasePrice,
                TotalPrice = CalculateTotalPrice(),
                HasExtraBaggage = _hasExtraBaggage,
                HasInsurance = _hasInsurance
            };

            var pdfBytes = PdfTicketGenerator.GenerateTicket(ticketData);
            var base64 = Convert.ToBase64String(pdfBytes);
            await JSRuntime.InvokeVoidAsync("downloadFile", $"ticket_{ticketData.PNR}.pdf", base64);
        }

        private static readonly string _pnrChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private string GeneratePnr()
        {
            var random = new Random();
            return new string(Enumerable.Repeat(_pnrChars, 6)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}