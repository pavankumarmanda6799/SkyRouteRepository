using Microsoft.Extensions.Hosting;
using System.Text.Json;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace FlightStatus.Api.Providers
{
    public abstract class BaseProvider<T>
    {
        private readonly string _dataPath;

        protected BaseProvider(IWebHostEnvironment env, string fileName)
        {
            _dataPath = Path.Combine(env.ContentRootPath, "Data", fileName);
        }

        protected async Task<IEnumerable<T>> ReadAllAsync()
        {
            try
            {
                if (!File.Exists(_dataPath)) return Enumerable.Empty<T>();

                using var stream = File.OpenRead(_dataPath);
                var items = await JsonSerializer.DeserializeAsync<IEnumerable<T>>(stream, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return items ?? Enumerable.Empty<T>();
            }
            catch
            {
                // Swallow exceptions from provider read/deserialize to avoid breaking caller flow.
                return Enumerable.Empty<T>();
            }
        }
    }
}
