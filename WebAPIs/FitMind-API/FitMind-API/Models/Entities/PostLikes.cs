using Microsoft.Extensions.Hosting;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FitMind_API.Models.Entities
{
    public class PostLikes
    {
        [Key]
        public int LikeId { get; set; }

        [Required]
        [ForeignKey("Post")]
        public int PostId { get; set; }

      
        public virtual AddPost? Post { get; set; } // Navigation property to Post

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }

        public virtual AppUsers? User { get; set; } // Navigation property to User

        public DateTime CreatedAt { get; set; }
    }
}
