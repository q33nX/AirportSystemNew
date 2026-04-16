using AirportSystem.Core.Entities;
using AirportSystem.Core.ViewModels;
using Microsoft.AspNetCore.Components;

namespace AirportSystem.Components.Pages // Проверьте ваш namespace
{
    public partial class Home
    {
        // Перечисляемое для режимов поиска (избавляемся от строк)
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
            StateHasChanged();
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
    }
}
