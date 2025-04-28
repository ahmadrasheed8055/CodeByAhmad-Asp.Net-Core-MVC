using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OpenApi;

namespace FitMind_API.Models.Entities
{
    public class AddPost
    {
        [Key]
        public int PostId { get; set; }

        [Required]
        [StringLength(255)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public bool IsPublished { get; set; }

        public int ViewCount { get; set; }

        public int LikeCount { get; set; }

        public bool IsDeleted { get; set; }

        // Relationships
        [ForeignKey("User")]
        [Required]
        public int UserId { get; set; }

        // Navigation property for the User (many-to-one)
        public virtual  AppUsers User { get; set; }

        [ForeignKey("Category")]
        [Required]
        public int CategoryId { get; set; }

        // Navigation property for Category (many-to-one)       
        public virtual  Categories Category { get; set; }

        // Image field to store image as binary data (nullable)
        public byte[]? PostImage { get; set; }  // Nullable for optional image attachment


        public ICollection<PostLikes>? Likes { get; set; }

        public ICollection<PostComments>? Comments { get; set; }
    }


}
