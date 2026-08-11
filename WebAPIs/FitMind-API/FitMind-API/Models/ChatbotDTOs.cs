namespace FitMind_API.Models
{
    public class ChatRequestDTO
    {
        public string Message { get; set; }
        public string? ImageBase64 { get; set; }
        public string? ImageMimeType { get; set; }
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
    }
}
