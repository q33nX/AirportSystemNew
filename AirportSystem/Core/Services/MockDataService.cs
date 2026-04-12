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
        var random = new Random();

        // 1. Города
        var moscow = new City { Name = "Москва", Country = "Россия", IATACode = "MOW" };
        var london = new City { Name = "Лондон", Country = "Великобритания", IATACode = "LON" };
        var istanbul = new City { Name = "Стамбул", Country = "Турция", IATACode = "IST" };
        var dubai = new City { Name = "Дубай", Country = "ОАЭ", IATACode = "DXB" };
        var paris = new City { Name = "Париж", Country = "Франция", IATACode = "PAR" };
        var tokyo = new City { Name = "Токио", Country = "Япония", IATACode = "TYO" };
        var ny = new City { Name = "Нью-Йорк", Country = "США", IATACode = "NYC" };
        var berlin = new City { Name = "Берлин", Country = "Германия", IATACode = "BER" };
        var rome = new City { Name = "Рим", Country = "Италия", IATACode = "ROM" };
        var madrid = new City { Name = "Мадрид", Country = "Испания", IATACode = "MAD" };
        var astana = new City { Name = "Астана", Country = "Казахстан", IATACode = "NQZ" };
        var bangkok = new City { Name = "Бангкок", Country = "Таиланд", IATACode = "BKK" };

        Cities.AddRange(new[] { moscow, london, istanbul, dubai, paris, tokyo, ny, berlin, rome, madrid, astana, bangkok });

        // 2. Аэропорты
        var airportsList = new List<Airport> {
        new Airport { Code = "SVO", Name = "Шереметьево", City = moscow, TimezoneOffset = 3 },
        new Airport { Code = "DME", Name = "Домодедово", City = moscow, TimezoneOffset = 3 },
        new Airport { Code = "LHR", Name = "Хитроу", City = london, TimezoneOffset = 0 },
        new Airport { Code = "IST", Name = "Стамбул Новый", City = istanbul, TimezoneOffset = 3 },
        new Airport { Code = "DXB", Name = "Дубай Интернешнл", City = dubai, TimezoneOffset = 4 },
        new Airport { Code = "CDG", Name = "Шарль-де-Голль", City = paris, TimezoneOffset = 1 },
        new Airport { Code = "HND", Name = "Ханеда", City = tokyo, TimezoneOffset = 9 },
        new Airport { Code = "JFK", Name = "Джон Кеннеди", City = ny, TimezoneOffset = -5 },
        new Airport { Code = "BER", Name = "Бранденбург", City = berlin, TimezoneOffset = 1 },
        new Airport { Code = "FCO", Name = "Фьюмичино", City = rome, TimezoneOffset = 1 },
        new Airport { Code = "MAD", Name = "Барахас", City = madrid, TimezoneOffset = 1 },
        new Airport { Code = "NQZ", Name = "Нурсултан Назарбаев", City = astana, TimezoneOffset = 5 },
        new Airport { Code = "BKK", Name = "Суварнабхуми", City = bangkok, TimezoneOffset = 7 }
    };
        Airports.AddRange(airportsList);

        // 3. Авиакомпании
        var airlinesList = new List<Airline> {
        new Airline { IATACode = "SU", Name = "Аэрофлот" },
        new Airline { IATACode = "TK", Name = "Turkish Airlines" },
        new Airline { IATACode = "EK", Name = "Emirates" },
        new Airline { IATACode = "DP", Name = "Победа" },
        new Airline { IATACode = "LH", Name = "Lufthansa" },
        new Airline { IATACode = "QR", Name = "Qatar Airways" },
        new Airline { IATACode = "AF", Name = "Air France" },
        new Airline { IATACode = "BA", Name = "British Airways" },
        new Airline { IATACode = "S7", Name = "S7 Airlines" }
    };
        Airlines.AddRange(airlinesList);

        // 4. Самолеты (Генерация сидений прямо внутри)

        // -- Airbus A320 (Бизнес + Эконом)
        var airbusA320 = new Aircraft { Model = "Airbus A320", Owner = airlinesList[0] };
        for (int r = 1; r <= 25; r++)
            foreach (char l in "ABCDEF")
                airbusA320.Seats.Add(new Seat { Row = r, Letter = l, Class = r <= 3 ? ServiceClass.Business : ServiceClass.Economy });

        // -- Boeing 777 (Первый + Бизнес + Эконом)
        var boeing777 = new Aircraft { Model = "Boeing 777", Owner = airlinesList[2] };
        for (int r = 1; r <= 45; r++)
            foreach (char l in "ABCDEF")
                boeing777.Seats.Add(new Seat { Row = r, Letter = l, Class = r <= 4 ? ServiceClass.First : (r <= 12 ? ServiceClass.Business : ServiceClass.Economy) });

        // -- Boeing 737 (Только Эконом для Лоукостеров)
        var boeing737 = new Aircraft { Model = "Boeing 737-800", Owner = airlinesList[3] };
        for (int r = 1; r <= 30; r++)
            foreach (char l in "ABCDEF")
                boeing737.Seats.Add(new Seat { Row = r, Letter = l, Class = ServiceClass.Economy });

        // 5. Шаблоны рейсов (40 разных маршрутов)
        for (int i = 0; i < 40; i++)
        {
            var airl = airlinesList[random.Next(airlinesList.Count)];
            var from = airportsList[random.Next(airportsList.Count)];
            var to = airportsList[random.Next(airportsList.Count)];

            if (from == to) { i--; continue; }

            FlightSchemas.Add(new FlightSchema
            {
                FlightNumber = $"{airl.IATACode}{random.Next(100, 9999)}",
                Carrier = airl,
                Origin = from,
                Destination = to,
                DepartureTime = new TimeSpan(random.Next(0, 24), random.Next(0, 6) * 10, 0),
                ArrivalOffset = TimeSpan.FromMinutes(random.Next(90, 850)),
                OperatingDays = Enum.GetValues<DayOfWeek>().ToList()
            });
        }

        // 6. Генерация экземпляров (FlightInstances) на 30 дней
        foreach (var schema in FlightSchemas)
        {
            for (int day = 0; day < 30; day++)
            {
                DateTime date = DateTime.Today.AddDays(day);

                // Динамическое ценообразование
                int price = schema.Carrier.IATACode switch
                {
                    "DP" => random.Next(3500, 14000),
                    "EK" or "QR" => random.Next(45000, 260000),
                    "BA" or "AF" or "LH" => random.Next(28000, 95000),
                    _ => random.Next(12000, 55000)
                };

                // Назначение самолета
                Aircraft selectedAircraft = schema.Carrier.IATACode switch
                {
                    "EK" or "QR" => boeing777,
                    "DP" => boeing737,
                    _ => airbusA320
                };

                FlightInstances.Add(new FlightInstance
                {
                    Schema = schema,
                    DepartureDate = date.Date.Add(schema.DepartureTime),
                    Aircraft = selectedAircraft,
                    BasePrice = price,
                    Status = FlightStatus.Scheduled
                });
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

        return Airports;
    }
}
