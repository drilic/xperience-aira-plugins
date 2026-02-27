using System.ComponentModel;
using System.Text.Json;

namespace DancingGoat.Plugins;

/// <summary>
/// Semantic Kernel plugin that provides current weather data.
/// Uses Open-Meteo API (free, no API key required).
/// Demonstrates: live HTTP calls, multiple kernel functions, TargetProviders filtering.
/// </summary>
[Description("Provides real-time weather data from the Open-Meteo API.")]
public class WeatherPlugin : IAiraPlugin
{
    private static readonly HttpClient Http = new();

    [KernelFunction("get_current_weather")]
    [Description("Gets the current weather for a given city or location. Returns temperature, wind speed, humidity, and weather condition.")]
    public async Task<string> GetCurrentWeatherAsync(
        [Description("The city or location name, e.g. 'Novi Sad' or 'London'")] string location,
        CancellationToken ct = default)
    {
        var weather = await FetchWeatherAsync(location, ct);
        if (weather == null)
            return $"Could not find location: {location}";

        return $"Weather in {weather.Name}, {weather.Country}: {weather.Condition}, "
             + $"{weather.Temperature}°C (feels like {weather.FeelsLike}°C), "
             + $"humidity {weather.Humidity}%, wind {weather.WindSpeed} km/h";
    }

    [KernelFunction("get_weather_forecast_summary")]
    [Description("Gets a short forecast summary for a given city: today's high/low temperature and expected conditions.")]
    public async Task<string> GetForecastSummaryAsync(
        [Description("The city or location name, e.g. 'Belgrade' or 'Paris'")] string location,
        CancellationToken ct = default)
    {
        var geo = await GeocodeAsync(location, ct);
        if (geo == null)
            return $"Could not find location: {location}";

        var url = $"https://api.open-meteo.com/v1/forecast?latitude={geo.Value.Lat}&longitude={geo.Value.Lon}"
                + "&daily=temperature_2m_max,temperature_2m_min,weather_code&timezone=auto&forecast_days=1";
        var json = await Http.GetStringAsync(url, ct);
        using var doc = JsonDocument.Parse(json);
        var daily = doc.RootElement.GetProperty("daily");

        var high = daily.GetProperty("temperature_2m_max")[0].GetDouble();
        var low = daily.GetProperty("temperature_2m_min")[0].GetDouble();
        var code = daily.GetProperty("weather_code")[0].GetInt32();

        return $"Forecast for {geo.Value.Name}, {geo.Value.Country} today: "
             + $"{WeatherCodeToDescription(code)}, high {high}°C, low {low}°C";
    }

    private async Task<WeatherData?> FetchWeatherAsync(string location, CancellationToken ct)
    {
        var geo = await GeocodeAsync(location, ct);
        if (geo == null)
            return null;

        var url = $"https://api.open-meteo.com/v1/forecast?latitude={geo.Value.Lat}&longitude={geo.Value.Lon}"
                + "&current=temperature_2m,relative_humidity_2m,apparent_temperature,wind_speed_10m,weather_code";
        var json = await Http.GetStringAsync(url, ct);
        using var doc = JsonDocument.Parse(json);
        var current = doc.RootElement.GetProperty("current");

        return new WeatherData
        {
            Name = geo.Value.Name,
            Country = geo.Value.Country,
            Temperature = current.GetProperty("temperature_2m").GetDouble(),
            FeelsLike = current.GetProperty("apparent_temperature").GetDouble(),
            Humidity = current.GetProperty("relative_humidity_2m").GetInt32(),
            WindSpeed = current.GetProperty("wind_speed_10m").GetDouble(),
            Condition = WeatherCodeToDescription(current.GetProperty("weather_code").GetInt32())
        };
    }

    private async Task<GeoResult?> GeocodeAsync(string location, CancellationToken ct)
    {
        var url = $"https://geocoding-api.open-meteo.com/v1/search?name={Uri.EscapeDataString(location)}&count=1";
        var json = await Http.GetStringAsync(url, ct);
        using var doc = JsonDocument.Parse(json);

        if (!doc.RootElement.TryGetProperty("results", out var results) || results.GetArrayLength() == 0)
            return null;

        var place = results[0];
        return new GeoResult
        {
            Lat = place.GetProperty("latitude").GetDouble(),
            Lon = place.GetProperty("longitude").GetDouble(),
            Name = place.GetProperty("name").GetString() ?? location,
            Country = place.TryGetProperty("country", out var c) ? c.GetString() ?? "" : ""
        };
    }

    private static string WeatherCodeToDescription(int code) => code switch
    {
        0 => "Clear sky",
        1 => "Mainly clear",
        2 => "Partly cloudy",
        3 => "Overcast",
        45 or 48 => "Foggy",
        51 or 53 or 55 => "Drizzle",
        61 or 63 or 65 => "Rain",
        66 or 67 => "Freezing rain",
        71 or 73 or 75 => "Snow",
        77 => "Snow grains",
        80 or 81 or 82 => "Rain showers",
        85 or 86 => "Snow showers",
        95 => "Thunderstorm",
        96 or 99 => "Thunderstorm with hail",
        _ => $"Weather code {code}"
    };

    private struct GeoResult
    {
        public double Lat;
        public double Lon;
        public string Name;
        public string Country;
    }

    private class WeatherData
    {
        public string Name { get; set; } = "";
        public string Country { get; set; } = "";
        public double Temperature { get; set; }
        public double FeelsLike { get; set; }
        public int Humidity { get; set; }
        public double WindSpeed { get; set; }
        public string Condition { get; set; } = "";
    }
}
