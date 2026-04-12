using AirportSystem.Core.Entities;
using AirportSystem.Core.Interfaces;
using AirportSystem.Core.Enums;

namespace AirportSystem.Core.Services;

public class MockDataService : IFlightRepository
{
    public List<City> Cities { get; private set; } = new();
    public List<Airport> Airports { get; private set; } = new();
    public List<Airline> Airlines { get; private set; } = new();
    public List<FlightSchema> FlightSchemas { get; private set; } = new();

    // Этот список заполняется в методе GenerateData()
    public List<FlightInstance> FlightInstances { get; private set; } = new();

    public MockDataService()
    {
        GenerateData();
    }

    private void GenerateData()
    {
        // ... (твой код генерации городов, аэропортов и авиакомпаний остается без изменений)
        var moscow = new City { Name = "Москва", Country = "Россия", IATACode = "MOW" };
        var london = new City { Name = "Лондон", Country = "Великобритания", IATACode = "LON" };
        var istanbul = new City { Name = "Стамбул", Country = "Турция", IATACode = "IST" };
        var dubai = new City { Name = "Дубай", Country = "ОАЭ", IATACode = "DXB" };
        Cities.AddRange(new[] { moscow, london, istanbul, dubai });

        Airports.Add(new Airport { Code = "SVO", Name = "Шереметьево", City = moscow, TimezoneOffset = 3, MinTransferTime = 90 });
        Airports.Add(new Airport { Code = "DME", Name = "Домодедово", City = moscow, TimezoneOffset = 3, MinTransferTime = 60 });
        Airports.Add(new Airport { Code = "LHR", Name = "Хитроу", City = london, TimezoneOffset = 0, MinTransferTime = 120 });
        Airports.Add(new Airport { Code = "IST", Name = "Стамбул Новый", City = istanbul, TimezoneOffset = 3, MinTransferTime = 100 });
        Airports.Add(new Airport { Code = "DXB", Name = "Дубай Интернешнл", City = dubai, TimezoneOffset = 4, MinTransferTime = 90 });

        var aeroflot = new Airline { IATACode = "SU", Name = "Аэрофлот" };
        var turkish = new Airline { IATACode = "TK", Name = "Turkish Airlines" };
        var emirates = new Airline { IATACode = "EK", Name = "Emirates" };
        Airlines.AddRange(new[] { aeroflot, turkish, emirates });

        var boeing = new Aircraft { Model = "Boeing 737", Owner = aeroflot };
        for (int r = 1; r <= 20; r++)
            foreach (char l in "ABCDEF")
                boeing.Seats.Add(new Seat { Row = r, Letter = l, Class = r < 4 ? ServiceClass.Business : ServiceClass.Economy });

        FlightSchemas.Add(new FlightSchema
        {
            FlightNumber = "SU2130",
            Carrier = aeroflot,
            Origin = Airports[0],
            Destination = Airports[3],
            DepartureTime = new TimeSpan(10, 0, 0),
            ArrivalOffset = new TimeSpan(4, 30, 0),
            OperatingDays = Enum.GetValues<DayOfWeek>().ToList()
        });

        FlightSchemas.Add(new FlightSchema
        {
            FlightNumber = "TK1980",
            Carrier = turkish,
            Origin = Airports[3],
            Destination = Airports[2],
            DepartureTime = new TimeSpan(15, 0, 0),
            ArrivalOffset = new TimeSpan(3, 50, 0),
            OperatingDays = new List<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday }
        });

        var random = new Random();
        for (int i = 0; i < 30; i++)
        {
            DateTime date = DateTime.Today.AddDays(i);
            foreach (var schema in FlightSchemas)
            {
                if (schema.OperatingDays.Contains(date.DayOfWeek))
                {
                    FlightInstances.Add(new FlightInstance
                    {
                        Schema = schema,
                        // Используем DateTime для DepartureTime, если в сущности так прописано
                        DepartureDate = date.Add(schema.DepartureTime),
                        Aircraft = boeing,
                        BasePrice = random.Next(15000, 50000),
                        Status = FlightStatus.Scheduled
                    });
                }
            }
        }
    }

    // ИСПРАВЛЕНО: Теперь возвращаем список FlightInstances, который мы заполнили
    public async Task<List<FlightInstance>> GetAllFlightsAsync()
    {
        return await Task.FromResult(FlightInstances);
    }

    public async Task<FlightInstance?> GetFlightByIdAsync(Guid id)
    {
        return await Task.FromResult(FlightInstances.FirstOrDefault(f => f.Id == id));
    }

    public async Task<List<Airport>> GetAirportsAsync()
    {

        return await Task.FromResult(Airports);
    }
}
