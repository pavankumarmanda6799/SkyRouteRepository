namespace FlightStatus.Api.Models
{
    public class QuickFlightResponse
    {
        public string FlightNumber { get; set; } = string.Empty;
        public DateTime FlightDate { get; set; }
        public string ProviderStatus { get; set; } = string.Empty;
        public DateTimeOffset ScheduledDepartureTime { get; set; }
        public DateTimeOffset ScheduledArrivalTime { get; set; }
        public DateTimeOffset LastUpdatedTimestamp { get; set; }
    }
}
