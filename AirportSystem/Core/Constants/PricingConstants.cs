namespace AirportSystem.Core.Constants;

public static class PricingConstants
{
    // Скидка за раннее бронирование (>60 дней)
    public const decimal EarlyBookingDiscount = 0.8m;

    // Наценка за срочное бронирование (<7 дней)
    public const decimal UrgentBookingMultiplier = 1.7m;

    // Наценка за бронирование за 7-21 день
    public const decimal ShortTermMultiplier = 1.2m;

    // Наценка за праздничные дни
    public const decimal PeakDateMultiplier = 1.4m;

    // Коэффициенты классов обслуживания
    public const decimal ComfortClassMultiplier = 1.3m;
    public const decimal BusinessClassMultiplier = 2.5m;
    public const decimal FirstClassMultiplier = 5.0m;

    // Наценка за место у окна
    public const decimal WindowSeatSurcharge = 15.0m;
}

public static class SearchConstants
{
    // Минимальное время пересадки в минутах
    public const int MinTransferTimeMinutes = 30;

    // Максимальное время пересадки в часах
    public const int MaxTransferTimeHours = 24;

    // Минимальный интервал между рейсами туда-обратно в часах
    public const int MinRoundTripIntervalHours = 1;

    // Лимит сегментов для мультигорода
    public const int MaxMultiCitySegments = 5;
    public const int MinMultiCitySegments = 2;
}
