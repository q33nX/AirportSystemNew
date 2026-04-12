namespace AirportSystem.Core.Entities;

public class Passenger
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public DateTime BirthDate { get; init; }
    public string PassportNumber { get; init; } = string.Empty;
    public string Citizenship { get; init; } = string.Empty;

    public int GetAge(DateTime flightDate)
    {
        int age = flightDate.Year - BirthDate.Year;
        if (BirthDate.Date > flightDate.AddYears(-age)) age--;
        return age;
    }
}
