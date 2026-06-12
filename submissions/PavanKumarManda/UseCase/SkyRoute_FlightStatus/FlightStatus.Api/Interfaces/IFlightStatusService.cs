using FlightStatus.Api.Models;
namespace FlightStatus.Api.Interfaces
{
    public interface IFlightStatusService
    {
        Task<IEnumerable<FlightStatusResultResponse>> GetAsync(string flightNumber, DateTime flightDate);
    }
}
