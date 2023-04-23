using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using Api.Models;
using Api.Services;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

builder.Services
    .AddAuthentication(configureOptions: options =>
    {
        options.DefaultAuthenticateScheme = "Local";
    })
    .AddJwtBearer("Local", configuration =>
    {
        configuration.TokenValidationParameters = new TokenValidationParameters
        {

            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = true,

            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Authentication:Secret"]))
        };
    });

builder.Services.AddAuthorization(configurator => configurator
    .AddPolicy("staff_policy", policy => policy.RequireRole("staff"))
);

builder.Services.AddHttpClient();

builder.Services.AddSingleton<WeatherApiClient>(container =>
{
    var key = container.GetRequiredService<IConfiguration>().GetValue<string>("WeatherApi:Token");

    ArgumentNullException.ThrowIfNull(key, "WeatherApi:Token is required");

    return new WeatherApiClient(key, container.GetRequiredService<HttpClient>());
});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/login", (LoginRequest login, IConfiguration configuration) =>
{
    var secret = configuration["Authentication:Secret"];

    ArgumentNullException.ThrowIfNull(secret, "Authentication:Secret is required");

    var token = GenrateToken(secret, login.Role);

    return Results.Ok(new { token });
});

app.MapGet("/", () =>
{
    var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

    return Results.File(path + ".\\frontend\\index.html", "text/html");
});

app.MapGet("/{zipcode:long}", async (string zipcode, WeatherApiClient apiClient) =>
{
    return await apiClient.GetWeatherAsync(zipcode);
}).RequireAuthorization("staff_policy");

app.MapGet("/{anythingelse}", (string anythingelse) =>
{
    return Results.NotFound($"{anythingelse} is not a valid zip code. Please try again.");
}).RequireAuthorization();

app.Run();

string GenrateToken(string secret, string role)
{
    var tokenHandler = new JwtSecurityTokenHandler();

    var key = Encoding.ASCII.GetBytes(secret);

    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.Role, role)
        }),
        Expires = DateTime.UtcNow.AddDays(7),
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
    };

    var token = tokenHandler.CreateToken(tokenDescriptor);

    return tokenHandler.WriteToken(token);
}