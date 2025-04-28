namespace FitMind_API.Services
{
    public class DeepAiService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public DeepAiService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<DeepAIResult> CheckForNudity(IFormFile imageFile)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("DeepAI");

                // Verify client configuration
                if (client.BaseAddress == null)
                    throw new Exception("BaseAddress not configured");

                if (!client.DefaultRequestHeaders.Contains("api-key"))
                    throw new Exception("API key header missing");

                using var stream = imageFile.OpenReadStream();
                using var content = new MultipartFormDataContent();
                using var imageContent = new StreamContent(stream);

                content.Add(imageContent, "image", imageFile.FileName);

                // Debug output
                Console.WriteLine($"Sending to: {client.BaseAddress}api/nsfw-detector");
                Console.WriteLine($"Headers: {string.Join(", ", client.DefaultRequestHeaders)}");

                var response = await client.PostAsync("api/nsfw-detector", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"API Error: {response.StatusCode} - {errorContent}");
                }

                return await response.Content.ReadFromJsonAsync<DeepAIResult>();
            }
            catch (Exception ex)
            {
                // Log the full error
                Console.WriteLine($"Error in CheckForNudity: {ex}");
                throw;
            }
        }
    }
}


public class DeepAIResult
{
    public string Id { get; set; }
    public Output Output { get; set; }
}

public class Output
{
    public bool Nsfw_Score { get; set; } // True if NSFW
    public decimal Nudity { get; set; }   // Probability (0-1)
    public decimal Suggestive { get; set; }
    public decimal Violence { get; set; }
}