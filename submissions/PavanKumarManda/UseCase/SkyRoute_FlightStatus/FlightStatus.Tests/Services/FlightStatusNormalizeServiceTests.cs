#nullable enable
using System;
using System.Threading.Tasks;
using FlightStatus.Api.Models;
using FlightStatus.Api.Services;
using Xunit;

namespace FlightStatus.Tests
{
    public class FlightStatusNormalizeServiceTests
    {
        [Fact]
        public async Task NormalizeAsync_BothProvidersNull_ReturnsUnknownAndMessage()
        {
            // Arrange
            var svc = new FlightStatusNormalizeService();
            var flightNumber = "SR123";
            var flightDate = new DateTime(2026, 6, 10);

            // Act
            var result = await svc.NormalizeAsync(flightNumber, flightDate, null, null);

            // Assert
            Assert.Equal(flightNumber, result.FlightNumber);
            Assert.Equal(UnifiedFlightStatus.Unknown.ToString(), result.Status);
            Assert.Null(result.LastUpdatedTimestamp);
            Assert.Equal("No provider data available", result.Message);
        }

        [Fact]
        public async Task NormalizeAsync_AeroResponse_PopulatesFieldsAndMessage()
        {
            // Arrange
            var svc = new FlightStatusNormalizeService();
            var aero = new AeroTrackResponse
            {
                FlightNumber = "SR200",
                FlightDate = new DateTime(2026, 6, 11),
                ProviderStatus = "ON_TIME",
                ScheduledDepartureTime = new DateTimeOffset(2026, 6, 11, 8, 0, 0, TimeSpan.Zero),
                ScheduledArrivalTime = new DateTimeOffset(2026, 6, 11, 10, 0, 0, TimeSpan.Zero),
                ActualDepartureTime = new DateTimeOffset(2026, 6, 11, 8, 5, 0, TimeSpan.Zero),
                ActualArrivalTime = new DateTimeOffset(2026, 6, 11, 10, 10, 0, TimeSpan.Zero),
                Terminal = "T1",
                Gate = "G5",
                DelayReason = "Weather",
                LastUpdatedTimestamp = new DateTimeOffset(2026, 6, 11, 8, 6, 0, TimeSpan.Zero)
            };

            // Act
            var result = await svc.NormalizeAsync(aero.FlightNumber, aero.FlightDate, aero, null);

            // Assert
            Assert.Equal(aero.FlightNumber, result.FlightNumber);
            Assert.Equal(UnifiedFlightStatus.OnTime.ToString(), result.Status);
            Assert.Equal(aero.ScheduledDepartureTime, result.ScheduledDepartureTime);
            Assert.Equal(aero.ScheduledArrivalTime, result.ScheduledArrivalTime);
            Assert.Equal(aero.ActualDepartureTime, result.ActualDepartureTime);
            Assert.Equal(aero.ActualArrivalTime, result.ActualArrivalTime);
            Assert.Equal(aero.Terminal, result.Terminal);
            Assert.Equal(aero.Gate, result.Gate);
            Assert.Equal(aero.DelayReason, result.DelayReason);
            Assert.Equal(aero.LastUpdatedTimestamp, result.LastUpdatedTimestamp);
            Assert.Equal("AeroTrack", result.Message);
        }

        [Fact]
        public async Task NormalizeAsync_QuickResponse_PopulatesFieldsAndMessage()
        {
            // Arrange
            var svc = new FlightStatusNormalizeService();
            var quick = new QuickFlightResponse
            {
                FlightNumber = "SR300",
                FlightDate = new DateTime(2026, 6, 12),
                ProviderStatus = "ON_SCHEDULE",
                ScheduledDepartureTime = new DateTimeOffset(2026, 6, 12, 12, 0, 0, TimeSpan.Zero),
                ScheduledArrivalTime = new DateTimeOffset(2026, 6, 12, 14, 0, 0, TimeSpan.Zero),
                LastUpdatedTimestamp = new DateTimeOffset(2026, 6, 12, 11, 50, 0, TimeSpan.Zero)
            };

            // Act
            var result = await svc.NormalizeAsync(quick.FlightNumber, quick.FlightDate, null, quick);

            // Assert
            Assert.Equal(quick.FlightNumber, result.FlightNumber);
            Assert.Equal(UnifiedFlightStatus.OnTime.ToString(), result.Status);
            Assert.Equal(quick.ScheduledDepartureTime, result.ScheduledDepartureTime);
            Assert.Equal(quick.ScheduledArrivalTime, result.ScheduledArrivalTime);
            Assert.Equal(quick.LastUpdatedTimestamp, result.LastUpdatedTimestamp);
            Assert.Equal("QuickFlight", result.Message);
        }

        [Theory]
        [InlineData("ON_TIME", "OnTime")]
        [InlineData("LATE", "Delayed")]
        [InlineData("CANCELLED", "Cancelled")]
        [InlineData("DIVERTED", "Diverted")]
        [InlineData("unknown_status", "Unknown")]
        public async Task NormalizeAsync_AeroProviderStatus_MapsToUnified(string providerStatus, string expected)
        {
            // Arrange
            var svc = new FlightStatusNormalizeService();
            var aero = new AeroTrackResponse
            {
                FlightNumber = "SR400",
                FlightDate = new DateTime(2026, 6, 13),
                ProviderStatus = providerStatus,
                ScheduledDepartureTime = DateTimeOffset.MinValue,
                ScheduledArrivalTime = DateTimeOffset.MinValue,
                LastUpdatedTimestamp = DateTimeOffset.UtcNow
            };

            // Act
            var result = await svc.NormalizeAsync(aero.FlightNumber, aero.FlightDate, aero, null);

            // Assert
            Assert.Equal(expected, result.Status);
        }

        [Theory]
        [InlineData("ON_SCHEDULE", "OnTime")]
        [InlineData("DELAYED", "Delayed")]
        [InlineData("CANCELED", "Cancelled")]
        [InlineData("REROUTED", "Diverted")]
        [InlineData("some_other", "Unknown")]
        public async Task NormalizeAsync_QuickProviderStatus_MapsToUnified(string providerStatus, string expected)
        {
            // Arrange
            var svc = new FlightStatusNormalizeService();
            var quick = new QuickFlightResponse
            {
                FlightNumber = "SR500",
                FlightDate = new DateTime(2026, 6, 14),
                ProviderStatus = providerStatus,
                ScheduledDepartureTime = DateTimeOffset.MinValue,
                ScheduledArrivalTime = DateTimeOffset.MinValue,
                LastUpdatedTimestamp = DateTimeOffset.UtcNow
            };

            // Act
            var result = await svc.NormalizeAsync(quick.FlightNumber, quick.FlightDate, null, quick);

            // Assert
            Assert.Equal(expected, result.Status);
        }
    }
}
