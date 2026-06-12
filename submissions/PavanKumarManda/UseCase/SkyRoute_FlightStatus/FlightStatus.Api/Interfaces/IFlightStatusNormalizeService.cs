namespace FlightStatus.Api.Interfaces
{
    public interface IFlightStatusNormalizeService
    {
        Task<Models.FlightStatusResultResponse> NormalizeAsync(string flightNumber, DateTime flightDate,
            Models.AeroTrackResponse? aeroResponse,
            Models.QuickFlightResponse? quickResponse);
    }
}
