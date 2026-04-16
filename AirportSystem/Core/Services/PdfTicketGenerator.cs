using AirportSystem.Core.Enums;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AirportSystem.Core.Services;

public class TicketData
{
    public string PNR { get; set; } = string.Empty;
    public string OriginCity { get; set; } = string.Empty;
    public string OriginCode { get; set; } = string.Empty;
    public string DestinationCity { get; set; } = string.Empty;
    public string DestinationCode { get; set; } = string.Empty;
    public DateTime DepartureDate { get; set; }
    public DateTime ArrivalDate { get; set; }
    public string FlightNumber { get; set; } = string.Empty;
    public string CarrierName { get; set; } = string.Empty;
    public string AircraftModel { get; set; } = string.Empty;
    public ServiceClass ServiceClass { get; set; }
    public int AdultsCount { get; set; }
    public int ChildrenCount { get; set; }
    public decimal BasePrice { get; set; }
    public decimal TotalPrice { get; set; }
    public bool HasExtraBaggage { get; set; }
    public bool HasInsurance { get; set; }
}

public static class PdfTicketGenerator
{
    public static byte[] GenerateTicket(TicketData data)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(c => ComposeHeader(c, data));
                page.Content().Element(c => ComposeContent(c, data));
                page.Footer().AlignCenter().Text("Авиабилет забронирован через систему AirportSystem");
            });
        }).GeneratePdf();
    }

    private static void ComposeHeader(IContainer container, TicketData data)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().Text("АВИАБИЛЕТ / BOARDING PASS").Bold().FontSize(16);
                col.Item().Text($"PNR: {data.PNR}").Bold().FontSize(14).FontColor(Colors.Blue.Darken2);
            });
            row.ConstantItem(100).Height(50).Background(Colors.Grey.Lighten3).AlignCenter().Text("AIRPORT\nSYSTEM");
        });
    }

    private static void ComposeContent(IContainer container, TicketData data)
    {
        container.PaddingVertical(20).Column(col =>
        {
            // Маршрут
            col.Item().Background(Colors.Blue.Lighten5).Padding(15).Column(route =>
            {
                route.Item().Text("МАРШРУТ").Bold().FontSize(12);
                route.Item().Text($"{data.OriginCity} ({data.OriginCode}) → {data.DestinationCity} ({data.DestinationCode})").FontSize(14).Bold();
            });

            col.Item().PaddingVertical(10);

            // Рейс
            col.Item().Table(table =>
            {
                table.ColumnsDefinition(cols =>
                {
                    cols.RelativeColumn(2);
                    cols.RelativeColumn(3);
                });

                table.Cell().Text("Авиакомпания:").Bold();
                table.Cell().Text(data.CarrierName);
                table.Cell().Text("Номер рейса:").Bold();
                table.Cell().Text(data.FlightNumber);
                table.Cell().Text("Самолёт:").Bold();
                table.Cell().Text(data.AircraftModel);
            });

            col.Item().PaddingVertical(10);

            // Даты
            col.Item().Table(table =>
            {
                table.ColumnsDefinition(cols =>
                {
                    cols.RelativeColumn();
                    cols.RelativeColumn();
                });

                table.Cell().Text("Вылет:").Bold();
                table.Cell().Text(data.DepartureDate.ToString("dd MMMM yyyy, HH:mm"));
                table.Cell().Text("Прилёт:").Bold();
                table.Cell().Text(data.ArrivalDate.ToString("dd MMMM yyyy, HH:mm"));
            });

            col.Item().PaddingVertical(10);

            // Класс и пассажиры
            col.Item().Table(table =>
            {
                table.ColumnsDefinition(cols =>
                {
                    cols.RelativeColumn();
                    cols.RelativeColumn();
                });

                table.Cell().Text("Класс обслуживания:").Bold();
                table.Cell().Text(GetServiceClassName(data.ServiceClass));
                table.Cell().Text("Взрослые:").Bold();
                table.Cell().Text($"{data.AdultsCount}");
                table.Cell().Text("Дети:").Bold();
                table.Cell().Text($"{data.ChildrenCount}");
            });

            col.Item().PaddingVertical(10);

            // Дополнительные услуги
            col.Item().Background(Colors.Grey.Lighten4).Padding(10).Column(services =>
            {
                services.Item().Text("ДОПОЛНИТЕЛЬНЫЕ УСЛУГИ").Bold();
                services.Item().Text(data.HasExtraBaggage ? "✓ Дополнительный багаж" : "✗ Дополнительный багаж");
                services.Item().Text(data.HasInsurance ? "✓ Страховка" : "✗ Страховка");
            });

            col.Item().PaddingVertical(20);

            // Итого
            col.Item().Background(Colors.Green.Lighten4).Padding(15).Column(price =>
            {
                price.Item().Text("ИТОГО К ОПЛАТЕ").Bold().FontSize(12);
                price.Item().Text($"{data.TotalPrice:N0} ₽").FontSize(20).Bold().FontColor(Colors.Green.Darken2);
            });
        });
    }

    private static string GetServiceClassName(ServiceClass serviceClass) => serviceClass switch
    {
        ServiceClass.Economy => "Эконом (Economy)",
        ServiceClass.Comfort => "Комфорт (Comfort)",
        ServiceClass.Business => "Бизнес (Business)",
        ServiceClass.First => "Первый класс (First Class)",
        _ => "Эконом"
    };
}