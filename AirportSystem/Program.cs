using AirportSystem.Components;
using AirportSystem.Core.Interfaces;
using AirportSystem.Core.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// MockDataService ������ in-memory ������ � ������ ��� ����������, ����� ������ ���� ������ ��� ����� ����������.
builder.Services.AddSingleton<IFlightRepository, MockDataService>();

builder.Services.AddScoped<IPriceCalculator, PriceCalculatorService>();
builder.Services.AddScoped<IFlightSearchService, FlightSearchService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
