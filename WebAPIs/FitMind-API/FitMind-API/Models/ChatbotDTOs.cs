namespace FitMind_API.Models
{
    public class ChatRequestDTO
    {
        public string Message { get; set; }
        public string? ImageBase64 { get; set; }
        public string? ImageMimeType { get; set; }
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public List<ChatHistory> History { get; set; } = new List<ChatHistory>();
    }

    public class ChatHistory
    {
        public string Role { get; set; } // "user" or "model"
        public string Content { get; set; }
    }

    public class ChatResponseDTO
    {
        public string Response { get; set; }
        public string? DetectedIntent { get; set; }
        public string? IntentDisplayName { get; set; }
        public double? ConfidenceScore { get; set; }
        public bool IsSafetyAlert { get; set; } = false;
        public string? SafetyWarning { get; set; }
    }

    public class CustomAIPredictionResult
    {
        public string Intent { get; set; }
        public string DisplayName { get; set; }
        public double Confidence { get; set; }
        public bool SafetyFlag { get; set; }
        public string? WarningPrefix { get; set; }
        public List<string> DomainRules { get; set; } = new List<string>();
    }
}
