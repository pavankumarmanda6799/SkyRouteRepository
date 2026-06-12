using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Moq;
using Xunit;
using FlightStatus.Api.Providers;
using FlightStatus.Api.Interfaces;
using FlightStatus.Api.Models;

namespace FlightStatus.Tests.Providers
{
    public class AeroTrackProviderTests
    {
        [Fact]
        public void Constructor_NullEnvironment_ThrowsNullReferenceException()
        {
            // Arrange

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => new AeroTrackProvider(null!));
        }

        [Fact]
        public void Constructor_ValidEnvironment_CreatesInstance()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);

            var envMock = new Mock<IWebHostEnvironment>();
            envMock.SetupGet(e => e.ContentRootPath).Returns(tempDir);

            // Act
            var provider = new AeroTrackProvider(envMock.Object);

            // Assert
            Assert.NotNull(provider);
            Assert.IsAssignableFrom<IFlightStatusProvider<AeroTrackResponse>>(provider);

            // Cleanup
            Directory.Delete(tempDir, true);
        }

        [Fact]
        public async Task GetAsync_MatchingFlightNumberAndDate_ReturnsItems()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            var dataDir = Path.Combine(tempDir, "Data");
            Directory.CreateDirectory(dataDir);

            var fileName = "aerotrack-data.json";
            var filePath = Path.Combine(dataDir, fileName);

            var targetDate = new DateTime(2023, 6, 1, 14, 30, 0);

            var items = new List<AeroTrackResponse>
            {
                new AeroTrackResponse { FlightNumber = "AB123", FlightDate = targetDate, ProviderStatus = "OnTime" },
                // same flight number different case -> should match
                new AeroTrackResponse { FlightNumber = "ab123", FlightDate = targetDate.AddHours(2), ProviderStatus = "Delayed" },
                // different flight number -> should not match
                new AeroTrackResponse { FlightNumber = "CD456", FlightDate = targetDate, ProviderStatus = "Cancelled" },
                // same flight number but different date -> should not match
                new AeroTrackResponse { FlightNumber = "AB123", FlightDate = targetDate.AddDays(1), ProviderStatus = "OnTime" }
            };

            await File.WriteAllTextAsync(filePath, JsonSerializer.Serialize(items));

            var envMock = new Mock<IWebHostEnvironment>();
            envMock.SetupGet(e => e.ContentRootPath).Returns(tempDir);

            var provider = new AeroTrackProvider(envMock.Object);

            // Act
            var results = (await provider.GetAsync("Ab123", targetDate)).ToList();

            // Assert
            Assert.NotNull(results);
            Assert.Equal(2, results.Count);
            Assert.All(results, r => Assert.Equal("AB123", r.FlightNumber, ignoreCase: true));
            Assert.All(results, r => Assert.Equal(targetDate.Date, r.FlightDate.Date));

            // Cleanup
            Directory.Delete(tempDir, true);
        }

        [Fact]
        public async Task GetAsync_NoMatchingItems_ReturnsEmpty()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            var dataDir = Path.Combine(tempDir, "Data");
            Directory.CreateDirectory(dataDir);

            var fileName = "aerotrack-data.json";
            var filePath = Path.Combine(dataDir, fileName);

            var items = new List<AeroTrackResponse>
            {
                new AeroTrackResponse { FlightNumber = "XY999", FlightDate = new DateTime(2020,1,1), ProviderStatus = "OnTime" }
            };

            await File.WriteAllTextAsync(filePath, JsonSerializer.Serialize(items));

            var envMock = new Mock<IWebHostEnvironment>();
            envMock.SetupGet(e => e.ContentRootPath).Returns(tempDir);

            var provider = new AeroTrackProvider(envMock.Object);

            // Act
            var results = await provider.GetAsync("AB123", DateTime.UtcNow);

            // Assert
            Assert.NotNull(results);
            Assert.Empty(results);

            // Cleanup
            Directory.Delete(tempDir, true);
        }
    }
}
