#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FlightStatus.Api.Interfaces;
using FlightStatus.Api.Models;
using FlightStatus.Api.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FlightStatus.Tests
{
    public class FlightStatusServiceTests
    {
        [Fact]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            // Arrange
            var aeroMock = new Mock<IFlightStatusProvider<AeroTrackResponse>>();
            var quickMock = new Mock<IFlightStatusProvider<QuickFlightResponse>>();
            var normalizeMock = new Mock<IFlightStatusNormalizeService>();
            var loggerMock = new Mock<ILogger<FlightStatusService>>();

            // Act
            var svc = new FlightStatusService(aeroMock.Object, quickMock.Object, normalizeMock.Object, loggerMock.Object);

            // Assert
            Assert.NotNull(svc);
        }

        [Fact]
        public async Task GetAsync_BothProvidersReturnNoData_CallsNormalizeWithNullsAndReturnsNormalized()
        {
            // Arrange
            var flightNumber = "NO_DATA";
            var flightDate = new DateTime(2026, 6, 10);

            var aeroMock = new Mock<IFlightStatusProvider<AeroTrackResponse>>();
            var quickMock = new Mock<IFlightStatusProvider<QuickFlightResponse>>();
            var normalizeMock = new Mock<IFlightStatusNormalizeService>();
            var loggerMock = new Mock<ILogger<FlightStatusService>>();

            aeroMock.Setup(x => x.GetAsync(flightNumber, flightDate)).ReturnsAsync(Array.Empty<AeroTrackResponse>());
            quickMock.Setup(x => x.GetAsync(flightNumber, flightDate)).Returns(Task.FromResult<IEnumerable<QuickFlightResponse>?>(Array.Empty<QuickFlightResponse>()));

            var expected = new FlightStatusResultResponse { FlightNumber = flightNumber };
            normalizeMock.Setup(x => x.NormalizeAsync(flightNumber, flightDate, null, null)).ReturnsAsync(expected);

            var svc = new FlightStatusService(aeroMock.Object, quickMock.Object, normalizeMock.Object, loggerMock.Object);

            // Act
            var result = (await svc.GetAsync(flightNumber, flightDate)).ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal(expected, result[0]);
            normalizeMock.Verify(x => x.NormalizeAsync(flightNumber, flightDate, null, null), Times.Once);
        }

        [Fact]
        public async Task GetAsync_AeroOnly_CallsNormalizeWithAero()
        {
            // Arrange
            var flightNumber = "AERO_ONLY";
            var flightDate = new DateTime(2026, 6, 11);

            var aero = new AeroTrackResponse
            {
                FlightNumber = flightNumber,
                FlightDate = flightDate,
                LastUpdatedTimestamp = new DateTimeOffset(2026, 6, 11, 8, 0, 0, TimeSpan.Zero)
            };

            var aeroMock = new Mock<IFlightStatusProvider<AeroTrackResponse>>();
            var quickMock = new Mock<IFlightStatusProvider<QuickFlightResponse>>();
            var normalizeMock = new Mock<IFlightStatusNormalizeService>();
            var loggerMock = new Mock<ILogger<FlightStatusService>>();

            aeroMock.Setup(x => x.GetAsync(flightNumber, flightDate)).ReturnsAsync(new[] { aero });
            quickMock.Setup(x => x.GetAsync(flightNumber, flightDate)).ReturnsAsync((IEnumerable<QuickFlightResponse>?)null);

            normalizeMock.Setup(x => x.NormalizeAsync(flightNumber, flightDate, aero, null))
                .ReturnsAsync(new FlightStatusResultResponse { FlightNumber = flightNumber });

            var svc = new FlightStatusService(aeroMock.Object, quickMock.Object, normalizeMock.Object, loggerMock.Object);

            // Act
            var result = (await svc.GetAsync(flightNumber, flightDate)).ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal(flightNumber, result[0].FlightNumber);
            normalizeMock.Verify(x => x.NormalizeAsync(flightNumber, flightDate, aero, null), Times.Once);
        }

        [Fact]
        public async Task GetAsync_QuickOnly_CallsNormalizeWithQuick()
        {
            // Arrange
            var flightNumber = "QUICK_ONLY";
            var flightDate = new DateTime(2026, 6, 12);

            var quick = new QuickFlightResponse
            {
                FlightNumber = flightNumber,
                FlightDate = flightDate,
                LastUpdatedTimestamp = new DateTimeOffset(2026, 6, 12, 9, 0, 0, TimeSpan.Zero)
            };

            var aeroMock = new Mock<IFlightStatusProvider<AeroTrackResponse>>();
            var quickMock = new Mock<IFlightStatusProvider<QuickFlightResponse>>();
            var normalizeMock = new Mock<IFlightStatusNormalizeService>();
            var loggerMock = new Mock<ILogger<FlightStatusService>>();

            aeroMock.Setup(x => x.GetAsync(flightNumber, flightDate)).ReturnsAsync(Array.Empty<AeroTrackResponse>());
            quickMock.Setup(x => x.GetAsync(flightNumber, flightDate)).ReturnsAsync((IEnumerable<QuickFlightResponse>?)new[] { quick });

            normalizeMock.Setup(x => x.NormalizeAsync(flightNumber, flightDate, null, quick))
                .ReturnsAsync(new FlightStatusResultResponse { FlightNumber = flightNumber });

            var svc = new FlightStatusService(aeroMock.Object, quickMock.Object, normalizeMock.Object, loggerMock.Object);

            // Act
            var result = (await svc.GetAsync(flightNumber, flightDate)).ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal(flightNumber, result[0].FlightNumber);
            normalizeMock.Verify(x => x.NormalizeAsync(flightNumber, flightDate, null, quick), Times.Once);
        }

        [Fact]
        public async Task GetAsync_BothProviders_AeroNewer_SelectsAero()
        {
            // Arrange
            var flightNumber = "BOTH_AERO_NEWER";
            var flightDate = new DateTime(2026, 6, 13);

            var aeroNew = new AeroTrackResponse { FlightNumber = flightNumber, FlightDate = flightDate, LastUpdatedTimestamp = DateTimeOffset.UtcNow };
            var aeroOld = new AeroTrackResponse { FlightNumber = flightNumber, FlightDate = flightDate, LastUpdatedTimestamp = DateTimeOffset.UtcNow.AddMinutes(-10) };

            var quick = new QuickFlightResponse { FlightNumber = flightNumber, FlightDate = flightDate, LastUpdatedTimestamp = DateTimeOffset.UtcNow.AddMinutes(-5) };

            var aeroMock = new Mock<IFlightStatusProvider<AeroTrackResponse>>();
            var quickMock = new Mock<IFlightStatusProvider<QuickFlightResponse>>();
            var normalizeMock = new Mock<IFlightStatusNormalizeService>();
            var loggerMock = new Mock<ILogger<FlightStatusService>>();

            // provide unsorted lists to ensure ordering logic is used
            aeroMock.Setup(x => x.GetAsync(flightNumber, flightDate)).ReturnsAsync(new[] { aeroOld, aeroNew });
            quickMock.Setup(x => x.GetAsync(flightNumber, flightDate)).ReturnsAsync(new[] { quick });

            normalizeMock.Setup(x => x.NormalizeAsync(flightNumber, flightDate, aeroNew, null))
                .ReturnsAsync(new FlightStatusResultResponse { FlightNumber = flightNumber });

            var svc = new FlightStatusService(aeroMock.Object, quickMock.Object, normalizeMock.Object, loggerMock.Object);

            // Act
            var result = (await svc.GetAsync(flightNumber, flightDate)).ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal(flightNumber, result[0].FlightNumber);
            normalizeMock.Verify(x => x.NormalizeAsync(flightNumber, flightDate, aeroNew, null), Times.Once);
        }

        [Fact]
        public async Task GetAsync_BothProviders_QuickNewer_SelectsQuick()
        {
            // Arrange
            var flightNumber = "BOTH_QUICK_NEWER";
            var flightDate = new DateTime(2026, 6, 14);

            var aero = new AeroTrackResponse { FlightNumber = flightNumber, FlightDate = flightDate, LastUpdatedTimestamp = DateTimeOffset.UtcNow.AddMinutes(-10) };
            var quickOld = new QuickFlightResponse { FlightNumber = flightNumber, FlightDate = flightDate, LastUpdatedTimestamp = DateTimeOffset.UtcNow.AddMinutes(-5) };
            var quickNew = new QuickFlightResponse { FlightNumber = flightNumber, FlightDate = flightDate, LastUpdatedTimestamp = DateTimeOffset.UtcNow };

            var aeroMock = new Mock<IFlightStatusProvider<AeroTrackResponse>>();
            var quickMock = new Mock<IFlightStatusProvider<QuickFlightResponse>>();
            var normalizeMock = new Mock<IFlightStatusNormalizeService>();
            var loggerMock = new Mock<ILogger<FlightStatusService>>();

            aeroMock.Setup(x => x.GetAsync(flightNumber, flightDate)).ReturnsAsync(new[] { aero });
            quickMock.Setup(x => x.GetAsync(flightNumber, flightDate)).ReturnsAsync(new[] { quickOld, quickNew });

            normalizeMock.Setup(x => x.NormalizeAsync(flightNumber, flightDate, null, quickNew))
                .ReturnsAsync(new FlightStatusResultResponse { FlightNumber = flightNumber });

            var svc = new FlightStatusService(aeroMock.Object, quickMock.Object, normalizeMock.Object, loggerMock.Object);

            // Act
            var result = (await svc.GetAsync(flightNumber, flightDate)).ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal(flightNumber, result[0].FlightNumber);
            normalizeMock.Verify(x => x.NormalizeAsync(flightNumber, flightDate, null, quickNew), Times.Once);
        }
    }
}
