using System.Net.Http;
using System.Text;
using System.Text.Json;
using FitMind_API.Models;

namespace FitMind_API.Services
{
    public class GeminiChatService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _systemPrompt = "You are an expert fitness, nutrition, and wellness assistant for FitMind-Forum (FitJoin). Your purpose is to help users with workout routines, diet plans, exercise forms, food images, and health documents. If a user uploads an image, document, or file, analyze it strictly in a fitness, nutrition, or health context (e.g. food nutrition estimate, workout form check, health/diet PDF). If the file/image is completely unrelated to health or fitness (e.g., code snippets, vehicles, electronics, memes), politely decline to analyze it.";

        public GeminiChatService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<string> GenerateChatResponseAsync(ChatRequestDTO request)
        {
            var apiKey = _configuration["Gemini:ApiKey"];
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-flash-latest:generateContent?key={apiKey}";

            var contents = new List<object>();

            // Add history
            foreach (var msg in request.History)
            {
                contents.Add(new
                {
                    role = msg.Role,
                    parts = new[] { new { text = msg.Content } }
                });
            }

            // Build user message parts (text + optional image/file)
            var userParts = new List<object>();

            if (!string.IsNullOrWhiteSpace(request.Message))
            {
                userParts.Add(new { text = request.Message });
            }
            else if (!string.IsNullOrWhiteSpace(request.ImageBase64))
            {
                userParts.Add(new { text = "Please analyze this attached image/file." });
            }

            if (!string.IsNullOrWhiteSpace(request.ImageBase64) && !string.IsNullOrWhiteSpace(request.ImageMimeType))
            {
                userParts.Add(new
                {
                    inline_data = new
                    {
                        mime_type = request.ImageMimeType,
                        data = request.ImageBase64
                    }
                });
            }

            contents.Add(new
            {
                role = "user",
                parts = userParts
            });

            var payload = new
            {
                system_instruction = new
                {
                    parts = new { text = _systemPrompt }
                },
                contents = contents
            };

            var jsonPayload = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Gemini API error: {error}");
            }

            var responseData = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(responseData);
            var root = document.RootElement;
            var textResponse = root
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return textResponse ?? "Sorry, I couldn't generate a response.";
        }
    }
}
