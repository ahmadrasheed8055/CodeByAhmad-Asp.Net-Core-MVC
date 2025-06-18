namespace FitMind_API.Models.DTOs
{
    public class GetDraftedPostDTO
    {
        public int PostId { get; set; }
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } 

        public DateTime? UpdatedAt { get; set; }


        public bool IsPublished { get; set; }


        public int UserId { get; set; }

        public int CategoryId { get; set; }

      
    }
}
