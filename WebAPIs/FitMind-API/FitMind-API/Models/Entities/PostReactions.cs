using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitMind_API.Models.Entities
{
    public class PostReactions
    {
        public int Id { get; set; }
        //public int PostId { get; set; }
        //public int UserId { get; set; }
        public bool? IsLike { get; set; } 
        public DateTime ReactedAt { get; set; }

        // Navigation properties
        [Required]
        [ForeignKey("Post")]
        public int PostId { get; set; }


        public virtual AddPost? Post { get; set; } // Navigation property to Post

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }

        public virtual AppUsers? User { get; set; } // Navigation property to User
    }
}
