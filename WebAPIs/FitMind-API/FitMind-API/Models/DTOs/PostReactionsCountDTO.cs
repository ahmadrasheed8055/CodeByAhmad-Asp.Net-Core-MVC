namespace FitMind_API.Models.DTOs
{
    public class PostReactionsCountDTO
    {
        public int PostId { get; set; }
        public int TotalLikes { get; set; }
        public int TotalDislikes { get; set; }
    }
}
