using FlightStatus.Api.Interfaces;
using FlightStatus.Api.Models;
using Microsoft.Extensions.Hosting;
using System.Text.Json;

namespace FlightStatus.Api.Providers
{
    public class QuickFlightProvider : BaseProvider<QuickFlightResponse>, IFlightStatusProvider<QuickFlightResponse>
    {
        public QuickFlightProvider(IWebHostEnvironment env) : base(env, "quickflight-data.json") { }

        public async Task<IEnumerable<QuickFlightResponse>> GetAsync(string flightNumber, DateTime flightDate)
        {
            var items = await ReadAllAsync();
            return items.Where(i => string.Equals(i.FlightNumber, flightNumber, StringComparison.OrdinalIgnoreCase)
                && i.FlightDate.Date == flightDate.Date);
        }
    }
}
