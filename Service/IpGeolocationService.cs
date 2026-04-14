using hr_crm.Service.Interface;
using System.Text.Json;

namespace hr_crm.Service
{
    public class IpGeolocationService : IIpGeolocationService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<IpGeolocationService> _logger;

        public IpGeolocationService(HttpClient httpClient, ILogger<IpGeolocationService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<(double? Latitude, double? Longitude, string? City, string? Country)> GetLocationAsync(string ipAddress)
        {
            // Skip private/loopback IPs (localhost, internal network)
            if (string.IsNullOrWhiteSpace(ipAddress) ||
                ipAddress == "::1" ||
                ipAddress.StartsWith("127.") ||
                ipAddress.StartsWith("192.168.") ||
                ipAddress.StartsWith("10.") ||
                ipAddress.StartsWith("172."))
            {
                _logger.LogInformation("Skipping geolocation for private IP: {Ip}", ipAddress);
                return (null, null, null, null);
            }

            try
            {
                var response = await _httpClient.GetAsync($"http://ip-api.com/json/{ipAddress}?fields=status,lat,lon,city,country");

                if (!response.IsSuccessStatusCode)
                    return (null, null, null, null);

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (root.GetProperty("status").GetString() != "success")
                    return (null, null, null, null);

                var lat = root.GetProperty("lat").GetDouble();
                var lon = root.GetProperty("lon").GetDouble();
                var city = root.TryGetProperty("city", out var cityEl) ? cityEl.GetString() : null;
                var country = root.TryGetProperty("country", out var countryEl) ? countryEl.GetString() : null;

                return (lat, lon, city, country);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "IP geolocation failed for IP: {Ip}", ipAddress);
                return (null, null, null, null);
            }
        }
    }
}
