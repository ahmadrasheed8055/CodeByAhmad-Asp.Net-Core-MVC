namespace FitMind_API.Models.DTOs
{
    public class GetAllPostsDTO
    {
        public int PostId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsPublished { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? UserImage { get; set; }
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? PostImage { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PublishAt { get; set; } //-> new colum
        public int ViewCount { get; set; }
        public int? LikeCount { get; set; }
        public int? DislikeCount { get; set; }

        public bool? IsReactedByMe { get; set; } // To check the reaction status of the user
        public bool? IsSavedByMe { get; set; } // Check if the user has saved this post
        public bool IsHidden { get; set; } // Indicates if the post is hidden by the author
    }
}
