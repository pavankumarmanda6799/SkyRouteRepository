using FlightStatus.Api.Interfaces;
using FlightStatus.Api.Models;

namespace FlightStatus.Api.Services
{
    public class FlightStatusNormalizeService : IFlightStatusNormalizeService
    {
        public Task<FlightStatusResultResponse> NormalizeAsync(string flightNumber, DateTime flightDate,
            AeroTrackResponse? aeroResponse,
            QuickFlightResponse? quickResponse)
        {
            if (aeroResponse == null && quickResponse == null)
            {
                return Task.FromResult(new FlightStatusResultResponse
                {
                    FlightNumber = flightNumber,
                    Status = UnifiedFlightStatus.Unknown.ToString(),
                    LastUpdatedTimestamp = null,
                    Message = "No provider data available"
                });
            }

            if (aeroResponse != null)
            {
                var a = aeroResponse;
                return Task.FromResult(new FlightStatusResultResponse
                {
                    FlightNumber = a.FlightNumber,
                    Status = MapAeroStatus(a.ProviderStatus),
                    ScheduledDepartureTime = a.ScheduledDepartureTime,
                    ScheduledArrivalTime = a.ScheduledArrivalTime,
                    ActualDepartureTime = a.ActualDepartureTime,
                    ActualArrivalTime = a.ActualArrivalTime,
                    Terminal = a.Terminal,
                    Gate = a.Gate,
                    DelayReason = a.DelayReason,
                    LastUpdatedTimestamp = a.LastUpdatedTimestamp,
                    Message = "AeroTrack"
                });
            }

            var q = quickResponse!;
            return Task.FromResult(new FlightStatusResultResponse
            {
                FlightNumber = q.FlightNumber,
                Status = MapQuickStatus(q.ProviderStatus),
                ScheduledDepartureTime = q.ScheduledDepartureTime,
                ScheduledArrivalTime = q.ScheduledArrivalTime,
                LastUpdatedTimestamp = q.LastUpdatedTimestamp,
                Message = "QuickFlight"
            });
        }

        private static string MapAeroStatus(string providerStatus)
        {
            return providerStatus?.ToUpperInvariant() switch
            {
                "ON_TIME" => UnifiedFlightStatus.OnTime.ToString(),
                "LATE" => UnifiedFlightStatus.Delayed.ToString(),
                "CANCELLED" => UnifiedFlightStatus.Cancelled.ToString(),
                "DIVERTED" => UnifiedFlightStatus.Diverted.ToString(),
                _ => UnifiedFlightStatus.Unknown.ToString(),
            };
        }

        private static string MapQuickStatus(string providerStatus)
        {
            return providerStatus?.ToUpperInvariant() switch
            {
                "ON_SCHEDULE" => UnifiedFlightStatus.OnTime.ToString(),
                "DELAYED" => UnifiedFlightStatus.Delayed.ToString(),
                "CANCELED" => UnifiedFlightStatus.Cancelled.ToString(),
                "REROUTED" => UnifiedFlightStatus.Diverted.ToString(),
                _ => UnifiedFlightStatus.Unknown.ToString(),
            };
        }
    }
}
