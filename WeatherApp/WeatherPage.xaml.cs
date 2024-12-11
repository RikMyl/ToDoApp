using Newtonsoft.Json;
using System.Net.Http;

namespace WeatherApp;

public partial class WeatherPage : ContentPage
{
    private const string ApiKey = "aa33fb27bb37681d0ea5cfef814651cf";
    private const string GeocodingApiUrl = "https://api.openweathermap.org/geo/1.0/direct";
    private const string WeatherApiUrl = "https://api.openweathermap.org/data/3.0/onecall";
    private static readonly HttpClient client = new HttpClient();

    public WeatherPage()
    {
        InitializeComponent();
    }
    private async void OnGetWeatherClicked(object sender, EventArgs e)
    {
        string city = CityEntry.Text?.Trim();

        if (string.IsNullOrEmpty(city))
        {
            await DisplayAlert("Error", "Please enter a city!", "OK");
            return;
        }

        try
        {
            // Hae koordinaatit kaupungista
            var coordinates = await GetCoordinatesAsync(city);

            if (coordinates != null)
            {
                // Hae säätiedot koordinaattien perusteella
                var weatherData = await GetWeatherDataAsync(coordinates.Lat, coordinates.Lon);

                if (weatherData != null && weatherData.Current != null)
                {
                    // Tarkista, että säädata on kunnossa
                    if (weatherData.Current.Weather != null && weatherData.Current.Weather.Count > 0)
                    {
                        string info = $"City: {city}\n" +
                                      $"Current Temperature: {weatherData.Current.Temp}°C\n" +
                                      $"Weather: {weatherData.Current.Weather[0].Description}\n" +
                                      $"Wind Speed: {weatherData.Current.WindSpeed} m/s\n"; // Tuulen nopeus

                        if (weatherData.Current.WindSpeed > 4.0)
                        {
                            info += $"\n⚠️ Wind speed exceeds 4 m/s! Current wind speed: {weatherData.Current.WindSpeed} m/s";
                        }

                        // Päivitä UI
                        WeatherInfoLabel.Text = info;
                    }
                    else
                    {
                        await DisplayAlert("Error", "Weather data is missing or malformed.", "OK");
                        Console.WriteLine("Weather data is missing or malformed.");
                    }
                }
                else
                {
                    await DisplayAlert("Error", "Weather data not found!", "OK");
                    Console.WriteLine("Weather data not found!");
                }
            }
            else
            {
                await DisplayAlert("Error", "City not found!", "OK");
                Console.WriteLine("City not found!");
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine("Request failed: " + ex.Message);
            await DisplayAlert("Error", "Network error, please try again later.", "OK");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Unexpected error: " + ex.Message);
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async Task<Coordinates> GetCoordinatesAsync(string city)
    {
        string url = $"{GeocodingApiUrl}?q={city}&limit=1&appid={ApiKey}";
        Console.WriteLine("Requesting Geocoding API with URL: " + url);

        var response = await client.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Geocoding API Response: " + json); // Tulosta JSON-vastaus lokiin
            var results = JsonConvert.DeserializeObject<List<Coordinates>>(json);

            if (results == null || results.Count == 0)
            {
                Console.WriteLine("No coordinates found for city: " + city);
                return null;
            }

            return results[0];
        }
        else
        {
            Console.WriteLine($"Geocoding API failed: {response.StatusCode} - {response.ReasonPhrase}");
            return null;
        }
    }

    private async Task<WeatherData> GetWeatherDataAsync(double lat, double lon)
    {
        string url = $"{WeatherApiUrl}?lat={lat}&lon={lon}&exclude=minutely,hourly,alerts&units=metric&appid={ApiKey}";
        Console.WriteLine("Requesting Weather API with URL: " + url);

        var response = await client.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Weather API Response: " + json); // Tarkista JSON-vastaus lokista

            try
            {
                // Deserialisoi JSON ja varmista sen rakenne
                return JsonConvert.DeserializeObject<WeatherData>(json);
            }
            catch (JsonException jsonEx)
            {
                Console.WriteLine("JSON Deserialization Error: " + jsonEx.Message);
                await DisplayAlert("Error", "Error parsing weather data!", "OK");
            }
        }
        else
        {
            Console.WriteLine($"Weather API failed: {response.StatusCode} - {response.ReasonPhrase}");
        }
        return null;
    }
}

public class Coordinates
{
    [JsonProperty("lat")]
    public double Lat { get; set; }

    [JsonProperty("lon")]
    public double Lon { get; set; }
}

public class WeatherData
{
    [JsonProperty("current")]
    public CurrentWeather Current { get; set; }
}

public class CurrentWeather
{
    [JsonProperty("temp")]
    public double Temp { get; set; }

    [JsonProperty("wind_speed")]
    public double WindSpeed { get; set; }

    [JsonProperty("weather")]
    public List<Weather> Weather { get; set; }
}

public class Weather
{
    [JsonProperty("description")]
    public string? Description { get; set; }
}