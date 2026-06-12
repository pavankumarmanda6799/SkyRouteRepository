using FlightStatus.Api.Interfaces;
using FlightStatus.Api.Models;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace FlightStatus.Api.Services
{
    public class FlightStatusService : IFlightStatusService
    {
        private readonly IFlightStatusProvider<AeroTrackResponse> _aeroProvider;
        private readonly IFlightStatusProvider<QuickFlightResponse> _quickProvider;
        private readonly IFlightStatusNormalizeService _normalize;
        private readonly ILogger<FlightStatusService> _logger;

        public FlightStatusService(
            IFlightStatusProvider<AeroTrackResponse> aeroProvider,
            IFlightStatusProvider<QuickFlightResponse> quickProvider,
            IFlightStatusNormalizeService normalize,
            ILogger<FlightStatusService> logger)
        {
            _aeroProvider = aeroProvider;
            _quickProvider = quickProvider;
            _normalize = normalize;
            _logger = logger;
        }
        public async Task<IEnumerable<FlightStatusResultResponse>> GetAsync(string flightNumber, DateTime flightDate)
        {
            IEnumerable<AeroTrackResponse> aeroList = Enumerable.Empty<AeroTrackResponse>();
            IEnumerable<QuickFlightResponse> quickList = Enumerable.Empty<QuickFlightResponse>();

            try
            {
                aeroList = await _aeroProvider.GetAsync(flightNumber, flightDate);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "AeroTrack provider failed for {flight} {date}", flightNumber, flightDate);
            }

            try
            {
                quickList = await _quickProvider.GetAsync(flightNumber, flightDate);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "QuickFlight provider failed for {flight} {date}", flightNumber, flightDate);
            }

            var aeroLatest = aeroList?.OrderByDescending(a => a.LastUpdatedTimestamp).FirstOrDefault();
            var quickLatest = quickList?.OrderByDescending(q => q.LastUpdatedTimestamp).FirstOrDefault();

            AeroTrackResponse? aeroToPass = null;
            QuickFlightResponse? quickToPass = null;

            if (aeroLatest != null && quickLatest != null)
            {
                if (aeroLatest.LastUpdatedTimestamp >= quickLatest.LastUpdatedTimestamp)
                {
                    aeroToPass = aeroLatest;
                }
                else
                {
                    quickToPass = quickLatest;
                }
            }
            else if (aeroLatest != null)
            {
                aeroToPass = aeroLatest;
            }
            else if (quickLatest != null)
            {
                quickToPass = quickLatest;
            }

            var normalized = await _normalize.NormalizeAsync(flightNumber, flightDate, aeroToPass, quickToPass);

            return new[] { normalized };
        }
    }
}
