namespace FitMind_API.Models.Entities
{
    public class Categories
    {
        public int Id { get; set; }  // Unique identifier for the category (Primary Key)
        public string Name { get; set; } = string.Empty;  // Name of the category (e.g., Fitness, Nutrition, etc.)
        public string Description { get; set; } = string.Empty;  // A brief description of the category
        public string? ImageUrl { get; set; }  // Optional: URL to an image representing the category (e.g., icon or banner)
        public bool IsActive { get; set; } = true;  // Status to determine if the category is active or archived
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;  // Date when the category was created
        public DateTime? UpdatedAt { get; set; }  // Date when the category was last updated (nullable)
    }
}

