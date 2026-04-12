using AirportSystem.Components;
using AirportSystem.Core.Interfaces;
using AirportSystem.Core.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped<IFlightRepository, MockDataService>();
builder.Services.AddScoped<IPriceCalculator, PriceCalculatorService>();
builder.Services.AddScoped<FlightSearchService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<BookingStateService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
