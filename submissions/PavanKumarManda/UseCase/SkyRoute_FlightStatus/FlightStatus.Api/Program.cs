using FlightStatus.Api.Providers;
using FlightStatus.Api.Interfaces;
using FlightStatus.Api.Models;
using FlightStatus.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// CORS: allow requests from Angular dev server
var corsPolicyName = builder.Configuration["CorsSettings:PolicyName"];
builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicyName!, policy =>
    {
        policy.WithOrigins(builder.Configuration["CorsSettings:AllowedOrigin"]!)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
// Register providers

builder.Services.AddSingleton<IFlightStatusProvider<AeroTrackResponse>,AeroTrackProvider>();
builder.Services.AddSingleton<IFlightStatusProvider<QuickFlightResponse>,QuickFlightProvider>();
builder.Services.AddScoped<IFlightStatusService, FlightStatusService>();
builder.Services.AddScoped<IFlightStatusNormalizeService, FlightStatusNormalizeService>();
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Enable CORS
app.UseCors(corsPolicyName!);
app.MapHealthChecks("/health");
app.UseAuthorization();

app.MapGet("/flights/status", async (IFlightStatusService flightStatusService, string? flightNumber, DateTime? flightDate) =>
{
    //validations
    if (string.IsNullOrWhiteSpace(flightNumber))
    {
        return Results.BadRequest("flightNumber is required");
    }

    if (!flightDate.HasValue)
    {
        return Results.BadRequest("flightDate is required");
    }

    var result = await flightStatusService.GetAsync(flightNumber, flightDate.Value);
    return Results.Ok(result);
});

app.Run();
