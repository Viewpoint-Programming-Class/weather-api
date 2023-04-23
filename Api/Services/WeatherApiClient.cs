namespace Api.Services;

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

    public Task<Weather?> GetWeatherAsync(string zipcode)
    {
        var url = $"{BaseUrl}?q={zipcode}&key={apiKey}&aqi=no";

        return client.GetFromJsonAsync<Weather>(url);
    }

    public class Weather
    {        
    }
}