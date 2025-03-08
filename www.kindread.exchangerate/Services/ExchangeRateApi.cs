
using KindRed.Exchange.Services;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;

namespace KindRed.Exchange.Services;
public class ExchangeRateApi : ICurrencyService {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly string? _apiKey;
        private readonly string? _apiUrl;

        public ExchangeRateApi(
            HttpClient httpClient,
            IMemoryCache cache,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _cache = cache;
            _apiKey = configuration.GetSection("ExchangeRateApi")?["ApiKey"];
            _apiUrl = configuration.GetSection("ExchangeRateApi")?["ApiUrl"];

        //validating key & url of ExchangeRate-API are provided
        if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_apiUrl)) {
                throw new InvalidOperationException("either API key or API url was not configured!");
            }
        }

        public async Task<decimal> ConvertCurrencyAsync(decimal amount, string baseCurrency, string targetCurrency)
        {
            //perform some validation of the request
            if (amount <= 0)
                throw new ArgumentException("Amount must be greater than zero.", nameof(amount));

            if (baseCurrency == targetCurrency)
                return amount;  

            //checking if conversion rate are in the caching before hitting the external URL
            var cacheKey = $"{baseCurrency}-{targetCurrency}";
            if (!_cache.TryGetValue(cacheKey, out decimal rate))
            {
                // invoking exchangeRate-API to perform conversion
                rate = await GetConversionRateAsync(baseCurrency, targetCurrency);
                _cache.Set(cacheKey, rate, TimeSpan.FromHours(1));  
            }

            return amount * rate;
        }

        private async Task<decimal> GetConversionRateAsync(string baseCurrency, string targetCurrency)
        {
            var url = string.Format(_apiUrl + _apiKey + "/latest/" + baseCurrency);
            var response = await _httpClient.GetStringAsync(url);

            var jsonResponse = JObject.Parse(response);
            if (jsonResponse["result"] == null || jsonResponse["result"]?.ToString() != "success")
            {

                throw new InvalidOperationException($"Unable to get conversion rates for {baseCurrency}");
            }

            var rate = jsonResponse["conversion_rates"]?[targetCurrency]?.Value<decimal>();
            if (rate == null)
            {
                throw new InvalidOperationException($"Unable to find conversion rate for {baseCurrency} to {targetCurrency}");
            }

            return rate.Value;

        }
    }

 