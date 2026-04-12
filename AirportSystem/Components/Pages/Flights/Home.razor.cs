using AirportSystem.Core.Entities;

namespace AirportSystem.Components.Pages // Проверьте ваш namespace
{
    public partial class Home
    {
        // Перечисляемое для режимов поиска (избавляемся от строк)
        private enum SearchMode { Roundtrip, MultiCity }

        private SearchMode _currentMode = SearchMode.Roundtrip;

        private List<FlightSchema> Segments = new()
        {
            new FlightSchema(),
            new FlightSchema()
        };

        private void SetSearchMode(SearchMode mode)
        {
            _currentMode = mode;
        }

        private void AddSegment()
        {
            if (Segments.Count < 5)
            {
                Segments.Add(new FlightSchema());
            }
        }

        private void RemoveSegment(int index)
        {
            if (Segments.Count > 2 && index < Segments.Count)
            {
                Segments.RemoveAt(index);
            }
        }
    }
}
