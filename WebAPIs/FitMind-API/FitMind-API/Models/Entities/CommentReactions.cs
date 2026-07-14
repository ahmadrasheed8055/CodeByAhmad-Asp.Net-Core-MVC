using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitMind_API.Models.Entities
{
    public class CommentReactions
    {
        [Key]
        public int Id { get; set; }

        public bool? IsLike { get; set; }

        public DateTime ReactedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        [Required]
        [ForeignKey("Comment")]
        public int CommentId { get; set; }

        public virtual PostComments? Comment { get; set; }

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }

        public virtual AppUsers? User { get; set; }
    }
}
