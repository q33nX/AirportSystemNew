namespace AirportSystem.Core.Entities;

public class CityDto
{
    public string Name { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string IATACode { get; set; } = string.Empty;
}

public class AirportDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string CityCode { get; set; } = string.Empty;
    public int TimezoneOffset { get; set; }
    public int MinTransferTime { get; set; }
}

public class AirlineDto
{
    public string IATACode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class SeatDto
{
    public int Row { get; set; }
    public char Letter { get; set; }
    public string Class { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public bool HasExtraLegroom { get; set; }
}

public class AircraftDto
{
    public string Model { get; set; } = string.Empty;
    public string OwnerIataCode { get; set; } = string.Empty;
    public List<SeatDto> Seats { get; set; } = new();
    public List<string> AvailableClasses { get; set; } = new();
}

public class FlightSchemaDto
{
    public string FlightNumber { get; set; } = string.Empty;
    public string CarrierIataCode { get; set; } = string.Empty;
    public string OriginCode { get; set; } = string.Empty;
    public string DestinationCode { get; set; } = string.Empty;
    public string DepartureTime { get; set; } = string.Empty;
    public int ArrivalOffsetMinutes { get; set; }
    public List<int> OperatingDays { get; set; } = new();
}

public class AirportDataRoot
{
    public List<CityDto> Cities { get; set; } = new();
    public List<AirportDto> Airports { get; set; } = new();
    public List<AirlineDto> Airlines { get; set; } = new();
    public List<AircraftDto> Aircrafts { get; set; } = new();
    public List<FlightSchemaDto> FlightSchemas { get; set; } = new();
}