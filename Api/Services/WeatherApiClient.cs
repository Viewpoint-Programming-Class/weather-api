namespace Api.Services;

using System.Text.Json.Serialization;

public class WeatherApiClient
{
    private const string BaseUrl = "https://api.weatherapi.com/v1/current.json";

    private readonly string apiKey;
    private readonly HttpClient client;

    public WeatherApiClient(string apiKey, HttpClient client)
    {
        this.apiKey = apiKey;
        this.client = client;
    }

    public async Task<Weather> GetWeatherAsync(string zipcode)
    {
        var url = $"{BaseUrl}?q={zipcode}&key={apiKey}&aqi=no";

        var weather = await client.GetFromJsonAsync<WeatherResponse>(url)
            ?? throw new Exception("Weather API returned null");

        return new Weather
        {
            Location = new Weather.LocationModel
            {
                City = weather.Location.Name,
                State = weather.Location.Region,
            }
        };
    }

    public class Weather
    {
        public LocationModel Location { get; set; }

        public class LocationModel
        {
            public string City { get; set; }
            public string State { get; set; }

        }
    }

    public class WeatherResponse
    {
        public LocationModel Location { get; set; }


        public class LocationModel
        {
            public string Name { get; set; }
            public string Region { get; set; }
        }
    }
}