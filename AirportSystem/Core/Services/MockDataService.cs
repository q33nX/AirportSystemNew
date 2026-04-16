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
        var citiesList = new List<City> {
            new City { Name = "Москва", Country = "Россия", IATACode = "MOW" },
            new City { Name = "Лондон", Country = "Великобритания", IATACode = "LON" },
            new City { Name = "Стамбул", Country = "Турция", IATACode = "IST" },
            new City { Name = "Дубай", Country = "ОАЭ", IATACode = "DXB" },
            new City { Name = "Париж", Country = "Франция", IATACode = "PAR" },
            new City { Name = "Токио", Country = "Япония", IATACode = "TYO" },
            new City { Name = "Нью-Йорк", Country = "США", IATACode = "NYC" },
            new City { Name = "Берлин", Country = "Германия", IATACode = "BER" },
            new City { Name = "Рим", Country = "Италия", IATACode = "ROM" },
            new City { Name = "Мадрид", Country = "Испания", IATACode = "MAD" },
            new City { Name = "Астана", Country = "Казахстан", IATACode = "NQZ" },
            new City { Name = "Бангкок", Country = "Таиланд", IATACode = "BKK" }
        };
        Cities.AddRange(citiesList);

        // 2. Аэропорты
        var airportsList = new List<Airport> {
            new Airport { Code = "SVO", Name = "Шереметьево", City = citiesList[0], TimezoneOffset = 3, MinTransferTime = 30 },
            new Airport { Code = "LHR", Name = "Хитроу", City = citiesList[1], TimezoneOffset = 0, MinTransferTime = 45 },
            new Airport { Code = "IST", Name = "Стамбул Новый", City = citiesList[2], TimezoneOffset = 3, MinTransferTime = 35 },
            new Airport { Code = "DXB", Name = "Дубай Интернешнл", City = citiesList[3], TimezoneOffset = 4, MinTransferTime = 40 },
            new Airport { Code = "CDG", Name = "Шарль-де-Голль", City = citiesList[4], TimezoneOffset = 1, MinTransferTime = 40 },
            new Airport { Code = "HND", Name = "Ханеда", City = citiesList[5], TimezoneOffset = 9, MinTransferTime = 50 },
            new Airport { Code = "JFK", Name = "Джон Кеннеди", City = citiesList[6], TimezoneOffset = -5, MinTransferTime = 60 },
            new Airport { Code = "BER", Name = "Бранденбург", City = citiesList[7], TimezoneOffset = 1, MinTransferTime = 35 },
            new Airport { Code = "FCO", Name = "Фьюмичино", City = citiesList[8], TimezoneOffset = 1, MinTransferTime = 35 },
            new Airport { Code = "MAD", Name = "Барахас", City = citiesList[9], TimezoneOffset = 1, MinTransferTime = 35 },
            new Airport { Code = "NQZ", Name = "Нурсултан Назарбаев", City = citiesList[10], TimezoneOffset = 5, MinTransferTime = 40 },
            new Airport { Code = "BKK", Name = "Суварнабхуми", City = citiesList[11], TimezoneOffset = 7, MinTransferTime = 45 }
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

        // 4. Самолеты (с предзаполнением AvailableClasses)
        var airbusA320 = new Aircraft { Model = "Airbus A320", Owner = airlinesList[0] };
        for (int r = 1; r <= 25; r++)
        {
            var sClass = r <= 3 ? ServiceClass.Business : ServiceClass.Economy;
            airbusA320.AvailableClasses.Add(sClass);
            foreach (char l in "ABCDEF")
                airbusA320.Seats.Add(new Seat { Row = r, Letter = l, Class = sClass, Location = SeatLocation.Window });
        }

        var boeing777 = new Aircraft { Model = "Boeing 777", Owner = airlinesList[2] };
        for (int r = 1; r <= 45; r++)
        {
            var sClass = r <= 4 ? ServiceClass.First : (r <= 12 ? ServiceClass.Business : ServiceClass.Economy);
            boeing777.AvailableClasses.Add(sClass);
            foreach (char l in "ABCDEF")
                boeing777.Seats.Add(new Seat { Row = r, Letter = l, Class = sClass, Location = SeatLocation.Aisle });
        }

        var boeing737 = new Aircraft { Model = "Boeing 737-800", Owner = airlinesList[3] };
        boeing737.AvailableClasses.Add(ServiceClass.Economy);
        for (int r = 1; r <= 30; r++)
            foreach (char l in "ABCDEF")
                boeing737.Seats.Add(new Seat { Row = r, Letter = l, Class = ServiceClass.Economy, Location = SeatLocation.Middle });

        // 5. Шаблоны рейсов (40 маршрутов)
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

        // 6. Экземпляры рейсов (FlightInstances) на 30 дней (итого ~1200 записей)
        foreach (var schema in FlightSchemas)
        {
            for (int day = 0; day < 30; day++)
            {
                DateTime date = DateTime.Today.AddDays(day);
                Aircraft selectedAircraft = schema.Carrier.IATACode switch
                {
                    "EK" or "QR" => boeing777,
                    "DP" => boeing737,
                    _ => airbusA320
                };

                FlightInstances.Add(new FlightInstance
                {
                    Schema = schema,
                    // ИСПРАВЛЕНО: DepartureDate должен хранить ТОЛЬКО дату (без времени).
                    DepartureDate = date.Date,
                    Aircraft = selectedAircraft,
                    BasePrice = schema.Carrier.IATACode switch
                    {
                        "DP" => random.Next(3500, 14000),
                        "EK" or "QR" => random.Next(45000, 260000),
                        _ => random.Next(12000, 55000)
                    },
                    Status = FlightStatus.Scheduled
                });
            }
        }
    }

    // Возвращаем Task-результаты явно
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
