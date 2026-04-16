using System.Collections.Concurrent;
        using AirportSystem.Core.Entities;
using AirportSystem.Core.Interfaces;
using AirportSystem.Core.Enums;

namespace AirportSystem.Core.Services;

public class BookingService : IBookingService
{
    private readonly IPriceCalculator _priceCalculator;
    // Потокобезопасное хранилище бронирований по PNR
    private readonly ConcurrentDictionary<string, Booking> _bookings = new();

    // Лок для операций с местами (грубая, но корректная синхронизация)
    private readonly object _seatLock = new();

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
        if (itinerary == null) throw new ArgumentNullException(nameof(itinerary));
        if (passengers == null || passengers.Count == 0) throw new ArgumentException("Нет пассажиров", nameof(passengers));

        // 1. Расчет итоговой стоимости (используем новый метод калькулятора)
        decimal totalBookingPrice = _priceCalculator.CalculateTotalItineraryPrice(
            itinerary,
            passengers.Count,
            serviceClass,
            seatPreference);

        // 2. Алгоритм бронирования мест на каждом сегменте маршрута
        var reservedSeats = new Dictionary<Guid, List<string>>();

        foreach (var flight in itinerary.Flights)
        {
            lock (_seatLock)
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

                reservedSeats[flight.Id] = new List<string>();

                for (int i = 0; i < passengers.Count; i++)
                {
                    var seat = availableSeats.FirstOrDefault(s => s.Location == seatPreference)
                               ?? availableSeats.First();

                    if (flight.OccupiedSeatIds.Contains(seat.Id))
                    {
                        // Откат ранее зарезервированных мест в этом запросе
                        foreach (var kv in reservedSeats)
                        {
                            var f = itinerary.Flights.FirstOrDefault(ff => ff.Id == kv.Key);
                            if (f != null)
                            {
                                foreach (var sId in kv.Value)
                                    f.OccupiedSeatIds.Remove(sId);
                            }
                        }
                        throw new InvalidOperationException($"Seat {seat.Id} уже занят на рейсе {flight.Schema.FlightNumber}");
                    }

                    flight.OccupiedSeatIds.Add(seat.Id);
                    reservedSeats[flight.Id].Add(seat.Id);

                    availableSeats.Remove(seat);
                }
            }
        }

        // 3. Создание объекта бронирования (Entity Booking) с информацией о местах
        var booking = new Booking
        {
            PNR = GenerateUniquePnr(),
            SelectedItinerary = itinerary,
            ContactEmail = email,
            ReservedSeatsPerFlight = reservedSeats
        };

        booking.Passengers.AddRange(passengers);

        // Сохраняем бронирование в потокобезопасное хранилище
        _bookings.TryAdd(booking.PNR, booking);

        Console.WriteLine($"Бронирование {booking.PNR} создано. Итоговая стоимость: {totalBookingPrice:C}");

        return await Task.FromResult(booking);
    }

    private static readonly string _pnrChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private string GeneratePnr()
    {
        var random = new Random();
        return new string(Enumerable.Repeat(_pnrChars, 6)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    private string GenerateUniquePnr()
    {
        for (int attempts = 0; attempts < 10_000; attempts++)
        {
            var pnr = GeneratePnr();
            if (!_bookings.ContainsKey(pnr)) return pnr;
        }
        return Guid.NewGuid().ToString("N").Substring(0, 6).ToUpperInvariant();
    }

    public async Task CancelBookingAsync(string pnr)
    {
        if (string.IsNullOrWhiteSpace(pnr)) return;

        if (_bookings.TryGetValue(pnr, out var booking))
        {
            booking.Cancel();

            lock (_seatLock)
            {
                foreach (var kv in booking.ReservedSeatsPerFlight)
                {
                    var flight = booking.SelectedItinerary.Flights.FirstOrDefault(f => f.Id == kv.Key);
                    if (flight == null) continue;

                    foreach (var seatId in kv.Value)
                    {
                        flight.OccupiedSeatIds.Remove(seatId);
                    }
                }
            }

            _bookings.TryRemove(pnr, out _);
        }

        await Task.CompletedTask;
    }

    public async Task<Booking?> GetBookingByPnrAsync(string pnr)
    {
        if (string.IsNullOrWhiteSpace(pnr)) return null;
        _bookings.TryGetValue(pnr, out var booking);
        return await Task.FromResult(booking);
    }
}
