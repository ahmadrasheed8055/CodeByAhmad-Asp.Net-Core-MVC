namespace FitMind_API.Models.DTOs
{
    public class CommentReactionDTO
    {
        public int CommentId { get; set; }
        public int UserId { get; set; }
        public bool? IsLike { get; set; } = null;
    }
}
