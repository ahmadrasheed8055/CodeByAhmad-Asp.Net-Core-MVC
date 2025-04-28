namespace FitMind_API.Models.DTOs
{
    public class AddPostDTO
    {
        public int PostId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public bool IsPublished { get; set; }

        public int ViewCount { get; set; }

        public int LikeCount { get; set; }

        public bool IsDeleted { get; set; }

        public int UserId { get; set; }

        public int CategoryId { get; set; }

        public IFormFile? PostImage { get; set; }
    }
}
