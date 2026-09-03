using System.ComponentModel.DataAnnotations;

namespace FitMind_API.Models.Entities
{
    public class AppUsers
    {
        [Key]
        public int Id { get; set; }

        [ StringLength(50)]
        public required string Username { get; set; }

        [EmailAddress, StringLength(100)]
        public required string Email { get; set; }

        //[Required]
        public required string PasswordHash { get; set; }  // Store Hashed Password

        public bool EmailConfirmed { get; set; } = false;

        public bool IsDeleted { get; set; } = false;

        public DateTime JoinedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public int Status { get; set; } = 1;

        //after updation profile details
        public string? UniqueName { get; set; }
        public int? UserVisibility { get; set; }
        public string? Bio { get; set; }
        public string? Phone { get; set; }
        public string? FacebookLink { get; set; }
        public string? InstagramLink { get; set; }
        public string? Location { get; set; }
        public string? Country { get; set; }
        public byte[]? ProfilePhoto { get; set; }
        public byte[]? BackgroundPhoto { get; set; }

        //update password date colum
        public DateTime? PasswordUpdateAt { get; set; }

        // Role ("User" or "Trainer")
        [StringLength(20)]
        public string Role { get; set; } = "User";

        // Trainer-Specific Details (Nullable for regular users)
        public int? SpecializationCategoryId { get; set; }
        public virtual Categories? SpecializationCategory { get; set; }

        public int? YearsOfExperience { get; set; }
        public string? Certifications { get; set; }
        public string? Availability { get; set; }
        [StringLength(30)]
        public string? WhatsAppNumber { get; set; }
            
        // Navigation Property for Tokens
        public ICollection<UserRT>? UserTokens { get; set; } = new List<UserRT>();
        // Navigation Property for Likes
        public ICollection<PostReactions>? postReactions { get; set; }

        // Navigation Property for Comments
        public ICollection<PostComments>? Comments { get; set; }

        // Navigation Properties for Follows
        public ICollection<UserFollower>? Followers { get; set; } // Users following this user
        public ICollection<UserFollower>? Following { get; set; } // Users this user follows
    }
}
