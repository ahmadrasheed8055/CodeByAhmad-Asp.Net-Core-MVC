using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FitMind_API.Models.Entities
{
    public class PostComments
    {
        [Key]
        public int CommentId { get; set; }

        [StringLength(1000)]
        public string? CommentContent { get; set; }

        public DateTime CreatedAt { get; set; }

        public bool IsDeleted { get; set; }

        //Relationships
        [Required]
        [ForeignKey("Post")]
        public int PostId { get; set; }
        public virtual AddPost? Post { get; set; } // Navigation property to Post

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }

        public virtual AppUsers? User { get; set; } // Navigation property to User

        // Comments System Extension:
        // Nullable ParentCommentId indicates this is a reply to another comment. If null, it's a root comment.
        public int? ParentCommentId { get; set; }

        [ForeignKey("ParentCommentId")]
        public virtual PostComments? ParentComment { get; set; }

        // Navigation property for fetching nested replies of this comment
        public virtual ICollection<PostComments> Replies { get; set; } = new List<PostComments>();

        // Navigation property for comment reactions (likes/dislikes)
        [InverseProperty("Comment")]
        public virtual ICollection<CommentReactions> CommentReactions { get; set; } = new List<CommentReactions>();    }
}
