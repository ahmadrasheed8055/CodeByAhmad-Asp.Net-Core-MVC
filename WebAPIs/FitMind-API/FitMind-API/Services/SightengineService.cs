using FitMind_API.Models.DTOs;
using Microsoft.AspNetCore.Http.HttpResults;
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

        public async Task<SightengineDTO> CheckImageAsync(IFormFile imageFile)
        {
            var apiUser = _configuration["Sightengine:ApiUser"];
            var apiSecret = _configuration["Sightengine:ApiSecret"];

            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri("https://api.sightengine.com/1.0/");

            using var content = new MultipartFormDataContent();
            using var streamContent = new StreamContent(imageFile.OpenReadStream());
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(imageFile.ContentType);

            content.Add(streamContent, "media", imageFile.FileName);
            content.Add(new StringContent("nudity,wad,offensive"), "models"); // you can also add 'wad', 'face-attributes' etc.
            content.Add(new StringContent(apiUser), "api_user");
            content.Add(new StringContent(apiSecret), "api_secret");

            var response = await client.PostAsync("check.json", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorDetails = await response.Content.ReadAsStringAsync();
                throw new Exception($"Sightengine API error: {errorDetails}");
            }

            var resultString = await response.Content.ReadAsStringAsync();
            SightengineDTO result = JsonConvert.DeserializeObject<SightengineDTO>(resultString);
            if (result == null)
            {
                throw new Exception("Result not found");
            }


            return result; // raw JSON string (you can later parse)
        }
    }
}
