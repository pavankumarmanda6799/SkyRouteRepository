using System;

namespace FlightStatus.Api.Models
{
    public class FlightStatusResultResponse
    {
        public string FlightNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTimeOffset? ScheduledDepartureTime { get; set; }
        public DateTimeOffset? ScheduledArrivalTime { get; set; }

        // Optional / when available
        public DateTimeOffset? ActualDepartureTime { get; set; }
        public DateTimeOffset? ActualArrivalTime { get; set; }
        public string? Terminal { get; set; }
        public string? Gate { get; set; }
        public string? DelayReason { get; set; }

        // Metadata
        public DateTimeOffset? LastUpdatedTimestamp { get; set; }
        public string? Message { get; set; }
    }
}
