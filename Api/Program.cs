using Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

builder.Services.AddSingleton<WeatherApiClient>(container => 
{
    var key = container.GetRequiredService<IConfiguration>().GetValue<string>("WeatherApi:Token");

    ArgumentNullException.ThrowIfNull(key, "WeatherApi:Token is required");

    return new WeatherApiClient(key, container.GetRequiredService<HttpClient>());
});

var app = builder.Build();

app.MapGet("/", () =>
{
    return "Please enter a zip code, e.g. /12345";
});


app.MapGet("/{zipcode}", (string zipcode, WeatherApiClient apiClient) =>
{
    return apiClient.GetWeatherAsync(zipcode);
});

app.Run();
