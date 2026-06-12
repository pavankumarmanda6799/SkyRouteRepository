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

namespace FlightStatus.Tests
{
    public class QuickFlightProviderTests
    {
        [Fact]
        public void Constructor_ValidEnv_CreatesInstance()
        {
            // Arrange
            var envMock = new Mock<IWebHostEnvironment>(MockBehavior.Strict);
            envMock.SetupGet(e => e.ContentRootPath).Returns(".");

            // Act
            var provider = new QuickFlightProvider(envMock.Object);

            // Assert
            Assert.NotNull(provider);
            Assert.IsAssignableFrom<IFlightStatusProvider<QuickFlightResponse>>(provider);
        }

        [Fact]
        public async Task GetAsync_NoDataFile_ReturnsEmpty()
        {
            // Arrange
            var tempRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempRoot);

            var envMock = new Mock<IWebHostEnvironment>(MockBehavior.Strict);
            envMock.SetupGet(e => e.ContentRootPath).Returns(tempRoot);

            var provider = new QuickFlightProvider(envMock.Object);

            // Act
            var result = await provider.GetAsync("ABC123", DateTime.UtcNow);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);

            // Cleanup
            Directory.Delete(tempRoot, true);
        }

        [Fact]
        public async Task GetAsync_MatchingFlightNumberAndDate_ReturnsMatches_IgnoringCase_AndDateOnly()
        {
            // Arrange
            var tempRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var dataDir = Path.Combine(tempRoot, "Data");
            Directory.CreateDirectory(dataDir);

            var items = new[]
            {
                new QuickFlightResponse { FlightNumber = "AB123", FlightDate = new DateTime(2023, 6, 1, 10, 0, 0), ProviderStatus = "OnTime", ScheduledDepartureTime = new DateTimeOffset(new DateTime(2023,6,1,10,0,0)) },
                new QuickFlightResponse { FlightNumber = "ab123", FlightDate = new DateTime(2023, 6, 1, 23, 59, 59), ProviderStatus = "Delayed", ScheduledDepartureTime = new DateTimeOffset(new DateTime(2023,6,1,23,59,59)) },
                new QuickFlightResponse { FlightNumber = "XY999", FlightDate = new DateTime(2023, 6, 02), ProviderStatus = "Cancelled", ScheduledDepartureTime = new DateTimeOffset(new DateTime(2023,6,2)) }
            };

            var json = JsonSerializer.Serialize(items, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            File.WriteAllText(Path.Combine(dataDir, "quickflight-data.json"), json);

            var envMock = new Mock<IWebHostEnvironment>(MockBehavior.Strict);
            envMock.SetupGet(e => e.ContentRootPath).Returns(tempRoot);

            var provider = new QuickFlightProvider(envMock.Object);

            // Act
            var result = (await provider.GetAsync("AB123", new DateTime(2023,6,1))).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, r => Assert.Equal("AB123", r.FlightNumber, ignoreCase: true));
            Assert.All(result, r => Assert.Equal(new DateTime(2023,6,1).Date, r.FlightDate.Date));

            // Cleanup
            Directory.Delete(tempRoot, true);
        }

        [Fact]
        public async Task GetAsync_NoMatchingFlight_ReturnsEmpty()
        {
            // Arrange
            var tempRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var dataDir = Path.Combine(tempRoot, "Data");
            Directory.CreateDirectory(dataDir);

            var items = new[]
            {
                new QuickFlightResponse { FlightNumber = "AA111", FlightDate = new DateTime(2023, 1, 1), ProviderStatus = "OnTime", ScheduledDepartureTime = new DateTimeOffset(new DateTime(2023,1,1)) }
            };

            var json = JsonSerializer.Serialize(items);
            File.WriteAllText(Path.Combine(dataDir, "quickflight-data.json"), json);

            var envMock = new Mock<IWebHostEnvironment>(MockBehavior.Strict);
            envMock.SetupGet(e => e.ContentRootPath).Returns(tempRoot);

            var provider = new QuickFlightProvider(envMock.Object);

            // Act
            var result = await provider.GetAsync("ZZ999", new DateTime(2023,1,1));

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);

            // Cleanup
            Directory.Delete(tempRoot, true);
        }
    }
}
