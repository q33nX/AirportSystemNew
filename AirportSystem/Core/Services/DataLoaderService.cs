using System.Text.Json;
using AirportSystem.Core.Entities;
using AirportSystem.Core.Enums;

namespace AirportSystem.Core.Services;

public class DataLoaderService
{
    private readonly IWebHostEnvironment _environment;
    private AirportDataRoot? _data;

    public DataLoaderService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<AirportDataRoot> LoadDataAsync()
    {
        if (_data != null) return _data;

        var filePath = Path.Combine(_environment.ContentRootPath, "wwwroot", "data", "airport-data.json");
        
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Data file not found: {filePath}");

        var json = await File.ReadAllTextAsync(filePath);
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        
        _data = JsonSerializer.Deserialize<AirportDataRoot>(json, options) 
            ?? throw new InvalidOperationException("Failed to deserialize airport data");
        
        return _data;
    }

    public List<City> GetCities()
    {
        return _data?.Cities.Select(c => new City
        {
            Name = c.Name,
            Country = c.Country,
            IATACode = c.IATACode
        }).ToList() ?? new List<City>();
    }

    public List<Airport> GetAirports()
    {
        if (_data == null) return new List<Airport>();

        var cities = GetCities().ToDictionary(c => c.IATACode);
        
        return _data.Airports.Select(a => new Airport
        {
            Code = a.Code,
            Name = a.Name,
            City = cities.GetValueOrDefault(a.CityCode) ?? new City { Name = a.CityCode, IATACode = a.CityCode, Country = "Unknown" },
            TimezoneOffset = a.TimezoneOffset,
            MinTransferTime = a.MinTransferTime
        }).ToList();
    }

    public List<Airline> GetAirlines()
    {
        return _data?.Airlines.Select(a => new Airline
        {
            IATACode = a.IATACode,
            Name = a.Name
        }).ToList() ?? new List<Airline>();
    }

    public List<Aircraft> GetAircrafts()
    {
        if (_data == null) return new List<Aircraft>();

        var airlines = GetAirlines().ToDictionary(a => a.IATACode);
        
        return _data.Aircrafts.Select(ac =>
        {
            var aircraft = new Aircraft
            {
                Model = ac.Model,
                Owner = airlines.GetValueOrDefault(ac.OwnerIataCode) ?? new Airline { IATACode = ac.OwnerIataCode }
            };

            foreach (var className in ac.AvailableClasses)
            {
                if (Enum.TryParse<ServiceClass>(className, out var serviceClass))
                {
                    aircraft.AvailableClasses.Add(serviceClass);
                }
            }

            if (ac.Seats.Any())
            {
                foreach (var seatDto in ac.Seats)
                {
                    var seat = new Seat
                    {
                        Row = seatDto.Row,
                        Letter = seatDto.Letter,
                        Class = Enum.TryParse<ServiceClass>(seatDto.Class, out var sc) ? sc : ServiceClass.Economy,
                        Location = Enum.TryParse<SeatLocation>(seatDto.Location, out var sl) ? sl : SeatLocation.Middle,
                        HasExtraLegroom = seatDto.HasExtraLegroom
                    };
                    aircraft.Seats.Add(seat);
                }
            }

            return aircraft;
        }).ToList();
    }

    public List<FlightSchema> GetFlightSchemas()
    {
        if (_data == null) return new List<FlightSchema>();

        var airlines = GetAirlines().ToDictionary(a => a.IATACode);
        var airports = GetAirports().ToDictionary(a => a.Code);

        return _data.FlightSchemas.Select(fs =>
        {
            var schema = new FlightSchema
            {
                FlightNumber = fs.FlightNumber,
                Carrier = airlines.GetValueOrDefault(fs.CarrierIataCode) ?? new Airline { IATACode = fs.CarrierIataCode },
                Origin = airports.GetValueOrDefault(fs.OriginCode) ?? new Airport { Code = fs.OriginCode },
                Destination = airports.GetValueOrDefault(fs.DestinationCode) ?? new Airport { Code = fs.DestinationCode },
                DepartureTime = TimeSpan.Parse(fs.DepartureTime),
                ArrivalOffset = TimeSpan.FromMinutes(fs.ArrivalOffsetMinutes),
                OperatingDays = fs.OperatingDays.Select(d => (DayOfWeek)(d == 7 ? 0 : d)).ToList()
            };
            return schema;
        }).ToList();
    }
}