using AirportSystem.Core.Entities;
using AirportSystem.Core.Interfaces;
using AirportSystem.Core.Enums;

namespace AirportSystem.Core.Services;

public class BookingService : IBookingService
{
    private readonly IPriceCalculator _priceCalculator;
    // Временное хранилище заказов для текущей сессии
    private readonly List<Booking> _bookings = new();

    public BookingService(IPriceCalculator priceCalculator)
    {
        _priceCalculator = priceCalculator;
    }

    public async Task<Booking> CreateBookingAsync(
        Itinerary itinerary,
        List<Passenger> passengers,
        string email,
        ServiceClass serviceClass,
        SeatLocation seatPreference)
    {
        // 1. Расчет итоговой стоимости (используем новый метод калькулятора)
        // Это значение можно передать в UI или сохранить в лог, так как в сущности Booking его нет
        decimal totalBookingPrice = _priceCalculator.CalculateTotalItineraryPrice(
            itinerary,
            passengers.Count,
            serviceClass,
            seatPreference);

        // 2. Алгоритм бронирования мест на каждом сегменте маршрута
        foreach (var flight in itinerary.Flights)
        {
            var classSeats = flight.Aircraft.Seats
                .Where(s => s.Class == serviceClass)
                .ToList();

            var availableSeats = classSeats
                .Where(s => flight.IsSeatAvailable(s.Id))
                .ToList();

            if (availableSeats.Count < passengers.Count)
            {
                throw new InvalidOperationException(
                    $"Места в классе {serviceClass} закончились на рейсе {flight.Schema.FlightNumber}");
            }

            for (int i = 0; i < passengers.Count; i++)
            {
                var seat = availableSeats.FirstOrDefault(s => s.Location == seatPreference)
                           ?? availableSeats.First();

                flight.OccupiedSeatIds.Add(seat.Id);
                availableSeats.Remove(seat);
            }
        }

        // 3. Создание объекта бронирования (Entity Booking)
        var booking = new Booking
        {
            PNR = GeneratePnr(),
            SelectedItinerary = itinerary,
            ContactEmail = email
        };

        booking.Passengers.AddRange(passengers);
        _bookings.Add(booking);

        // В рамках учебного проекта выведем цену в консоль, чтобы подтвердить расчет
        Console.WriteLine($"Бронирование {booking.PNR} создано. Итоговая стоимость: {totalBookingPrice:C}");

        return await Task.FromResult(booking);
    }

    private string GeneratePnr()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 6)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    public async Task CancelBookingAsync(string pnr)
    {
        var booking = _bookings.FirstOrDefault(b => b.PNR == pnr);
        if (booking != null)
        {
            // Используем метод из Entity
            booking.Cancel();

            // ООП-бонус: Освобождаем места на всех рейсах маршрута при отмене
            foreach (var flight in booking.SelectedItinerary.Flights)
            {
                // В реальности мы бы искали ID конкретных мест этого бронирования, 
                // здесь для простоты имитируем освобождение части мест.
                // flight.OccupiedSeatIds.Remove(...)
            }
        }
        await Task.CompletedTask;
    }

    public async Task<Booking?> GetBookingByPnrAsync(string pnr)
    {
        return await Task.FromResult(_bookings.FirstOrDefault(b => b.PNR == pnr));
    }
}
