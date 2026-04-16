using AirportSystem.Core.Enums;

namespace AirportSystem.Core.Configuration;

/// <summary>
/// Конфигурация распределения мест по классам обслуживания для конкретной модели самолёта
/// </summary>
public class CabinConfiguration
{
    /// <summary>Название модели самолёта</summary>
    public string AircraftModel { get; init; } = string.Empty;

    /// <summary>Группы мест по рядам (ряд_начало, ряд_конец, класс)</summary>
    public List<SeatGroup> SeatGroups { get; init; } = new();
}

/// <summary>
/// Группа мест в самолёте
/// </summary>
public class SeatGroup
{
    /// <summary>Первый ряд группы</summary>
    public int StartRow { get; init; }

    /// <summary>Последний ряд группы</summary>
    public int EndRow { get; init; }

    /// <summary>Класс обслуживания</summary>
    public ServiceClass ServiceClass { get; init; }

    public SeatGroup(int startRow, int endRow, ServiceClass serviceClass)
    {
        StartRow = startRow;
        EndRow = endRow;
        ServiceClass = serviceClass;
    }
}

/// <summary>
/// Стандартные конфигурации салонов для популярных самолётов
/// </summary>
public static class AircraftCabinConfigurations
{
    /// <summary>
    /// Airbus A320: Business (ряды 1-3), Comfort (ряды 4-8), Economy (ряды 9-25)
    /// </summary>
    public static CabinConfiguration AirbusA320 => new()
    {
        AircraftModel = "Airbus A320",
        SeatGroups = new List<SeatGroup>
        {
            new(1, 3, ServiceClass.Business),
            new(4, 8, ServiceClass.Comfort),
            new(9, 25, ServiceClass.Economy)
        }
    };

    /// <summary>
    /// Boeing 777: First (ряды 1-4), Business (ряды 5-10), Comfort (ряды 11-16), Economy (ряды 17-45)
    /// </summary>
    public static CabinConfiguration Boeing777 => new()
    {
        AircraftModel = "Boeing 777",
        SeatGroups = new List<SeatGroup>
        {
            new(1, 4, ServiceClass.First),
            new(5, 10, ServiceClass.Business),
            new(11, 16, ServiceClass.Comfort),
            new(17, 45, ServiceClass.Economy)
        }
    };

    /// <summary>
    /// Boeing 737-800: Comfort (ряды 1-4), Economy (ряды 5-30)
    /// </summary>
    public static CabinConfiguration Boeing737800 => new()
    {
        AircraftModel = "Boeing 737-800",
        SeatGroups = new List<SeatGroup>
        {
            new(1, 4, ServiceClass.Comfort),
            new(5, 30, ServiceClass.Economy)
        }
    };

    /// <summary>
    /// Получить конфигурацию по модели самолёта
    /// </summary>
    public static CabinConfiguration? Get(string model)
    {
        return model switch
        {
            "Airbus A320" => AirbusA320,
            "Boeing 777" => Boeing777,
            "Boeing 737-800" => Boeing737800,
            _ => null
        };
    }
}
