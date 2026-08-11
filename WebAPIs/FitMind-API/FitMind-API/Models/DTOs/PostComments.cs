namespace FitMind_API.Models.DTOs
{
    public class PostComments
    {
        public int CommentId { get; set; }

        public int PostId { get; set; }

        public int UserId { get; set; }

        public string CommentContent { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public bool IsDeleted { get; set; }

        // 👇 user Things
        public string UserName { get; set; } = string.Empty;
        public string? UserImage { get; set; }

        // 👇 Comments System Extension
        public int? ParentCommentId { get; set; }
        public int RepliesCount { get; set; }
        public int LikeCount { get; set; }
        public int DislikeCount { get; set; }
        public bool? IsReactedByMe { get; set; } // true for like, false for dislike, null for none
    }
}
