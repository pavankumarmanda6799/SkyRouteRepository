using FlightStatus.Api.Interfaces;
using FlightStatus.Api.Models;
using Microsoft.Extensions.Hosting;

namespace FlightStatus.Api.Providers
{
    public class AeroTrackProvider : BaseProvider<AeroTrackResponse>, IFlightStatusProvider<AeroTrackResponse>
    {
        public AeroTrackProvider(IWebHostEnvironment env) : base(env, "aerotrack-data.json") { }

        public async Task<IEnumerable<AeroTrackResponse>> GetAsync(string flightNumber, DateTime flightDate)
        {
            var items = await ReadAllAsync();
            return items.Where(i => string.Equals(i.FlightNumber, flightNumber, StringComparison.OrdinalIgnoreCase)
                && i.FlightDate.Date == flightDate.Date);
        }
    }
}
