using FitMind_API.Models.DTOs;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace FitMind_API.Services
{
    public class SightengineService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public SightengineService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        // Image moderation function with graceful network error handling
        public async Task<SightengineDTO?> CheckImageAsync(IFormFile imageFile)
        {
            try
            {
                var apiUser = _configuration["Sightengine:ApiUser"];
                var apiSecret = _configuration["Sightengine:ApiSecret"];

                if (string.IsNullOrEmpty(apiUser) || string.IsNullOrEmpty(apiSecret))
                {
                    Console.WriteLine("[Sightengine] API credentials not configured. Skipping image moderation.");
                    return null;
                }

                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(5);
                client.BaseAddress = new Uri("https://api.sightengine.com/1.0/");

                using var content = new MultipartFormDataContent();
                using var streamContent = new StreamContent(imageFile.OpenReadStream());
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(imageFile.ContentType);

                content.Add(streamContent, "media", imageFile.FileName);
                content.Add(new StringContent("nudity,wad,offensive"), "models");
                content.Add(new StringContent(apiUser), "api_user");
                content.Add(new StringContent(apiSecret), "api_secret");

                var response = await client.PostAsync("check.json", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorDetails = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[Sightengine] Image API HTTP {response.StatusCode}: {errorDetails}");
                    return null;
                }

                var resultString = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<SightengineDTO>(resultString);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Sightengine Network/Offline Warning]: Image check failed - {ex.Message}. Skipping moderation.");
                return null;
            }
        }

        // Text moderation function with graceful network error handling
        public async Task<SightengineTextModerationDTO?> CheckTextAsync(string text)
        {
            try
            {
                var apiUser = _configuration["Sightengine:ApiUser"];
                var apiSecret = _configuration["Sightengine:ApiSecret"];

                if (string.IsNullOrEmpty(apiUser) || string.IsNullOrEmpty(apiSecret))
                {
                    Console.WriteLine("[Sightengine] API credentials not configured. Skipping text moderation.");
                    return null;
                }

                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(5);

                var parameters = new Dictionary<string, string>
                {
                    { "text", text },
                    { "lang", "en" },
                    { "mode", "standard,rules,ml" },
                    { "api_user", apiUser },
                    { "api_secret", apiSecret }
                };

                var response = await client.PostAsync(
                    "https://api.sightengine.com/1.0/text/check.json",
                    new FormUrlEncodedContent(parameters));

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[Sightengine] Text API HTTP {response.StatusCode}");
                    return null;
                }

                var resultString = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Sightengine response: " + resultString);
                return JsonConvert.DeserializeObject<SightengineTextModerationDTO>(resultString);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Sightengine Network/Offline Warning]: Text check failed - {ex.Message}. Skipping moderation.");
                return null;
            }
        }
    }
}
