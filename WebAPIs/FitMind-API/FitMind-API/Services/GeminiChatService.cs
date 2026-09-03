using System.Net.Http;
using System.Text;
using System.Text.Json;
using FitMind_API.Data;
using FitMind_API.Models;
using Microsoft.EntityFrameworkCore;

namespace FitMind_API.Services
{
    public class GeminiChatService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly FMDBContext _context;
        private readonly string _systemPrompt = "You are an expert fitness, nutrition, and wellness assistant for FitMind-Forum (FitJoin). Your purpose is to help users with workout routines, diet plans, exercise forms, food images, and health documents. If a user uploads an image, document, or file, analyze it strictly in a fitness, nutrition, or health context (e.g. food nutrition estimate, workout form check, health/diet PDF). If the file/image is completely unrelated to health or fitness (e.g., code snippets, vehicles, electronics, memes), politely decline to analyze it.";

        // High-performance active models tested with 100% reliability
        private static readonly string[] CandidateModels = new[]
        {
            "gemini-3.6-flash",
            "gemini-3.5-flash",
            "gemini-flash-lite-latest",
            "gemini-3.5-flash-lite",
            "gemini-3.7-flash"
        };

        public GeminiChatService(HttpClient httpClient, IConfiguration configuration, FMDBContext context)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _context = context;
        }

        public async Task<ChatResponseDTO> GenerateChatResponseAsync(ChatRequestDTO request)
        {
            var apiKey = _configuration["Gemini:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new Exception("Gemini API key is not configured.");
            }

            // Step 1: Query Custom AI Engine (FitJoin Python Microservice) for Intent & Domain Knowledge
            CustomAIPredictionResult? aiPrediction = null;
            if (!string.IsNullOrWhiteSpace(request.Message))
            {
                try
                {
                    var aiRequestPayload = JsonSerializer.Serialize(new { text = request.Message });
                    var aiContent = new StringContent(aiRequestPayload, Encoding.UTF8, "application/json");
                    
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
                    var aiResponse = await _httpClient.PostAsync("http://127.0.0.1:8000/predict-intent", aiContent, cts.Token);
                    
                    if (aiResponse.IsSuccessStatusCode)
                    {
                        var aiJson = await aiResponse.Content.ReadAsStringAsync();
                        aiPrediction = JsonSerializer.Deserialize<CustomAIPredictionResult>(aiJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        Console.WriteLine($"[FitJoin-AI] Detected Intent: {aiPrediction?.Intent} ({aiPrediction?.DisplayName}) with confidence {aiPrediction?.Confidence:P1}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[FitJoin-AI] Custom AI service query non-fatal fallback: {ex.Message}");
                }
            }

            var contents = new List<object>();

            // Sanitize history: ensure it starts with "user" and strictly alternates
            if (request.History != null && request.History.Count > 0)
            {
                var validHistory = request.History
                    .Where(h => !string.IsNullOrWhiteSpace(h.Content))
                    .TakeLast(6)
                    .ToList();

                // Skip leading model turns if any
                while (validHistory.Count > 0 && validHistory[0].Role != "user")
                {
                    validHistory.RemoveAt(0);
                }

                foreach (var msg in validHistory)
                {
                    contents.Add(new
                    {
                        role = msg.Role == "model" ? "model" : "user",
                        parts = new[] { new { text = msg.Content } }
                    });
                }

                // If history ends with 'user', remove it so incoming message takes its place
                if (contents.Count > 0)
                {
                    var last = validHistory.LastOrDefault();
                    if (last != null && last.Role == "user")
                    {
                        contents.RemoveAt(contents.Count - 1);
                    }
                }
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

            // Dynamically enrich system prompt with Custom AI Knowledge Base & matching verified FitJoin trainers
            string dynamicSystemPrompt = _systemPrompt;

            if (aiPrediction != null)
            {
                dynamicSystemPrompt += $"\n\n[CUSTOM AI INTENT DETECTED]: {aiPrediction.DisplayName} (Confidence: {aiPrediction.Confidence * 100:F1}%)";
                if (aiPrediction.DomainRules != null && aiPrediction.DomainRules.Any())
                {
                    dynamicSystemPrompt += $"\n[DOMAIN KNOWLEDGE RULES TO ADHERE TO]:\n- " + string.Join("\n- ", aiPrediction.DomainRules);
                }

                if (aiPrediction.SafetyFlag && !string.IsNullOrWhiteSpace(aiPrediction.WarningPrefix))
                {
                    dynamicSystemPrompt += $"\n[URGENT SAFETY PROTOCOL]: Prioritize the user's health. State: '{aiPrediction.WarningPrefix}' and advise consulting a medical professional.";
                }
            }

            try
            {
                var trainerQuery = _context.AppUsers
                    .Include(u => u.SpecializationCategory)
                    .Where(u => u.Role == "Trainer" && !u.IsDeleted);

                if (request.CategoryId.HasValue && request.CategoryId.Value > 0)
                {
                    trainerQuery = trainerQuery.Where(u => u.SpecializationCategoryId == request.CategoryId.Value);
                }
                else if (!string.IsNullOrWhiteSpace(request.CategoryName))
                {
                    var catLower = request.CategoryName.ToLower().Trim();
                    trainerQuery = trainerQuery.Where(u => u.SpecializationCategory != null && u.SpecializationCategory.Name.ToLower().Contains(catLower));
                }

                var matchingTrainers = await trainerQuery.Take(2).ToListAsync();

                // If no direct category match, pick up to 1-2 featured trainers if prompt is fitness/workout related
                if (!matchingTrainers.Any())
                {
                    matchingTrainers = await _context.AppUsers
                        .Include(u => u.SpecializationCategory)
                        .Where(u => u.Role == "Trainer" && !u.IsDeleted)
                        .OrderByDescending(u => u.YearsOfExperience ?? 0)
                        .Take(2)
                        .ToListAsync();
                }

                if (matchingTrainers.Any())
                {
                    var trainerDetails = string.Join("\n", matchingTrainers.Select(t =>
                    {
                        var cleanPhone = (t.WhatsAppNumber ?? "").Replace("+", "").Replace(" ", "").Replace("-", "");
                        var waLink = !string.IsNullOrEmpty(cleanPhone) ? $"https://wa.me/{cleanPhone}" : "";
                        return $"- **{t.Username}** | Specialization: {t.SpecializationCategory?.Name ?? "General Fitness"} | Experience: {t.YearsOfExperience ?? 5} Years | Certifications: {t.Certifications ?? "Certified Personal Trainer"} | WhatsApp: {t.WhatsAppNumber ?? ""} (Direct Link: {waLink})";
                    }));

                    dynamicSystemPrompt += $"\n\n[VERIFIED FITJOIN TRAINER(S) AVAILABLE FOR 1-ON-1 COACHING]:\n{trainerDetails}\n\n[INSTRUCTION]: Provide clear, high-quality, encouraging fitness advice. If appropriate, recommend reaching out to the verified FitJoin trainer(s) above for personalized coaching plans or form checks, including their name and WhatsApp link.";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GeminiChatService] Trainer lookup non-fatal error: {ex.Message}");
            }

            var payload = new
            {
                system_instruction = new
                {
                    parts = new { text = dynamicSystemPrompt }
                },
                contents = contents
            };

            var jsonPayload = JsonSerializer.Serialize(payload);

            string? lastError = null;

            foreach (var model in CandidateModels)
            {
                try
                {
                    var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";
                    var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                    var response = await _httpClient.PostAsync(url, content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseData = await response.Content.ReadAsStringAsync();
                        using var document = JsonDocument.Parse(responseData);
                        var root = document.RootElement;
                        var textResponse = root
                            .GetProperty("candidates")[0]
                            .GetProperty("content")
                            .GetProperty("parts")[0]
                            .GetProperty("text")
                            .GetString();

                        return new ChatResponseDTO
                        {
                            Response = textResponse ?? "Sorry, I couldn't generate a response.",
                            DetectedIntent = aiPrediction?.Intent,
                            IntentDisplayName = aiPrediction?.DisplayName,
                            ConfidenceScore = aiPrediction?.Confidence,
                            IsSafetyAlert = aiPrediction?.SafetyFlag ?? false,
                            SafetyWarning = aiPrediction?.WarningPrefix
                        };
                    }

                    var errorBody = await response.Content.ReadAsStringAsync();
                    lastError = errorBody;
                    Console.WriteLine($"[Gemini] Model '{model}' returned HTTP {(int)response.StatusCode}: {errorBody}. Trying fallback model...");

                    // If 503 (High demand), 429 (Rate limit), or 404 (Model not found), try next fallback model
                    if ((int)response.StatusCode == 503 || (int)response.StatusCode == 429 || (int)response.StatusCode == 404)
                    {
                        continue;
                    }

                    // For client validation errors, break immediately
                    break;
                }
                catch (HttpRequestException ex)
                {
                    lastError = ex.Message;
                    Console.WriteLine($"[Gemini] Network exception with model '{model}': {ex.Message}");
                }
            }

            throw new Exception($"Gemini API error across candidate models. Last response: {lastError}");
        }
    }
}
