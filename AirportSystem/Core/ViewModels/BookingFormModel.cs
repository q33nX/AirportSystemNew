using System.ComponentModel.DataAnnotations;
using AirportSystem.Core.Enums;

namespace AirportSystem.Core.ViewModels;

public class BookingFormModel
{
    [Required(ErrorMessage = "Email обязателен")]
    [EmailAddress(ErrorMessage = "Некорректный формат")]
    public string ContactEmail { get; set; } = string.Empty;

    public ServiceClass SelectedClass { get; set; } = ServiceClass.Economy;
    public SeatLocation SeatPreference { get; set; } = SeatLocation.Window;

    public List<PassengerDetailModel> Passengers { get; set; } = new() { new() };
}

public class PassengerDetailModel
{
    [Required(ErrorMessage = "Введите имя")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите фамилию")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите паспорт")]
    public string PassportNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Дата рождения обязательно к заполнению")]
    public DateTime BirthDate { get; set; } = DateTime.Today.AddYears(-20);

    [Required(ErrorMessage = "Укажите гражданство")]
    public string Citizenship { get; set; } = "Belarus";
}
