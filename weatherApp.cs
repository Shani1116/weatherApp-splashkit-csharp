using System;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using SplashKitSDK;

namespace WeatherApp
{
    public class Program
    {
        private static readonly HttpClient client = new HttpClient();
        private static string cityName = "unknown";
        private static string weatherInfo = "Enter city and press Enter";

        public static void Main()
        {
            Window window = new Window("Weather App", 540, 360);

            Font font = new Font("input", "arial.ttf");
            Bitmap background = new Bitmap("background", "background.jpg");

            // Define the area where text should appear
            Rectangle inputRect = SplashKit.RectangleFrom(200.0, 80.0, 200.0, 30.0);
            Rectangle outputRect = SplashKit.RectangleFrom(50.0, 120.0, 440.0, 200.0);

            // OpenWeatherMap API key
            string apiKey = "4xxxxxxxxxxxxxxxxxxxxxxxxxe";

            do {
                SplashKit.ProcessEvents();

                DateTime currentTime = DateTime.Now;
                string dateString = currentTime.ToString("yyyy-MM-dd");
                string timeString = currentTime.ToString("HH:mm:ss");

                // Click anywhere on the screen to trigger the text input
                if (SplashKit.MouseClicked(MouseButton.LeftButton)) {
                    // Start reading text in the rectangle area
                    SplashKit.StartReadingText(inputRect);
                }

                // Check if the text input is done
                if (!SplashKit.ReadingText()) {
                    // Check if the text input was cancelled
                    if (SplashKit.TextEntryCancelled()) {
                        cityName = "unknown";
                    } else {
                        // Get the city name from the text input
                        cityName = SplashKit.TextInput();
                        // Fetch weather info if the city name is not empty
                        if (!string.IsNullOrEmpty(cityName)) {
                            weatherInfo = FetchWeather(cityName, apiKey);
                        }
                    }
                }

                // Draw the screen...
                window.Clear(Color.White);
                window.DrawBitmap(background, 0, 0);

                window.DrawRectangle(Color.Black, inputRect);
                window.DrawText("Enter City Name:", Color.Black, font, 18, 60, inputRect.Y);

                if (SplashKit.ReadingText()) {
                    SplashKit.DrawCollectedText(Color.Black, font, 18, SplashKit.OptionDefaults());
                }

                window.DrawText("Date: " + dateString, Color.Black, font, 18, 10, 10);
                window.DrawText("Time: " + timeString, Color.Black, font, 18, 10, 40);

                // Draw the city name and weather info in the output rectangle
                DrawMultiLineText(window, font, outputRect, $"City: {cityName}\n{weatherInfo}");

                // Show it...
                window.Refresh(60);
            } while (!window.CloseRequested);

            SplashKit.CloseAllWindows();
        }

        // Method to fetch weather information from OpenWeatherMap API
        public static string FetchWeather(string cityName, string apiKey) {
            string apiURL = $"http://api.openweathermap.org/data/2.5/weather?q={cityName}&appid={apiKey}&units=metric";

            try {
                string response = client.GetStringAsync(apiURL).Result;
                JObject weatherData = JObject.Parse(response);

                // Extract the data
                string description = weatherData["weather"][0]["description"].ToString();
                double temperature = double.Parse(weatherData["main"]["temp"].ToString());
                int humidity = int.Parse(weatherData["main"]["humidity"].ToString());

                // Return the formatted weather info
                return $"Weather: {description}\nTemperature: {temperature}°C\nHumidity: {humidity}%";
            } catch (HttpRequestException) {
                return "Error fetching weather data!";
            }
        }

        private static void DrawMultiLineText(Window window, Font font, Rectangle rect, string text) {
            string[] lines = text.Split('\n');
            double y = rect.Y + 10; // Starting Y position within the rectangle
            const double lineHeight = 22; // Adjust line height as needed

            foreach (string line in lines) {
                window.DrawText(line, Color.Black, font, 18, rect.X + 10, y);
                y += lineHeight;
            }
        }
    }
}
