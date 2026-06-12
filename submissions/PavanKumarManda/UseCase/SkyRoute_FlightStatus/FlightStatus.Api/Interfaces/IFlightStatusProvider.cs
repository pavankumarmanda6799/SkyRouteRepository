namespace FlightStatus.Api.Interfaces
{
    public interface IFlightStatusProvider<T>
    {
        /// <summary>
        /// Return matching records for the given flight number and flight date.
        /// Providers return their concrete model type.
        /// </summary>
        Task<IEnumerable<T>> GetAsync(string flightNumber, DateTime flightDate);
    }
}
