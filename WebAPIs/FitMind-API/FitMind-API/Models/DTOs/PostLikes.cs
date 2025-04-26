namespace FitMind_API.Models.DTOs
{
    public class PostLikes
    {
        public int LikeId { get; set; }

        public int PostId { get; set; }

        public int UserId { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
