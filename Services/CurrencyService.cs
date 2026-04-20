using System.Net.Http.Json;
using Microsoft.Extensions.Caching.Memory;

namespace TechMoveSystems.Services;

public interface ICurrencyService
{
    Task<decimal> GetLiveUsdToZarRateAsync(CancellationToken cancellationToken = default);
}

public class CurrencyService(HttpClient httpClient, IMemoryCache cache) : ICurrencyService
{
    private const decimal FallbackRate = 19.00m;
    private const string CacheKey = "currency:usd-zar";

    public async Task<decimal> GetLiveUsdToZarRateAsync(CancellationToken cancellationToken = default)
    {
        if (cache.TryGetValue(CacheKey, out decimal cachedRate) && cachedRate > 0)
        {
            return cachedRate;
        }

        try
        {
            var response = await httpClient.GetFromJsonAsync<ExchangeData>("USD", cancellationToken);
            if (response?.Rates is not null && response.Rates.TryGetValue("ZAR", out var zarRate) && zarRate > 0)
            {
                var roundedRate = decimal.Round(zarRate, 4, MidpointRounding.AwayFromZero);
                cache.Set(CacheKey, roundedRate, TimeSpan.FromMinutes(10));
                return roundedRate;
            }
        }
        catch
        {
        }

        cache.Set(CacheKey, FallbackRate, TimeSpan.FromMinutes(2));
        return FallbackRate;
    }
}

public class ExchangeData
{
    public Dictionary<string, decimal> Rates { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
