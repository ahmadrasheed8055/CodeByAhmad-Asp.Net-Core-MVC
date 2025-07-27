namespace FitMind_API.Models.DTOs
{
    public class PostReactionsDTO
    {
        public int PostId { get; set; }
        public int UserId { get; set; }
        public bool? IsLike { get; set; } = null;
    }
}
