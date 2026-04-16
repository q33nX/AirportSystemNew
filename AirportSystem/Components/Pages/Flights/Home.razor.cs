using AirportSystem.Core.Entities;
using AirportSystem.Core.ViewModels;

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
            }
        }

        private void RemoveSegment(FlightSearchModel segment)
        {
            Segments.Remove(segment);
        }
    }
}
