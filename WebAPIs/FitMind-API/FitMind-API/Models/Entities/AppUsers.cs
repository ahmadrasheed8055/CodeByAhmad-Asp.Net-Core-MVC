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
            
        // Navigation Property for Tokens
        public ICollection<UserRT>? UserTokens { get; set; } = new List<UserRT>();
        // Navigation Property for Likes
        public ICollection<PostLikes>? Likes { get; set; }

        // Navigation Property for Comments
        public ICollection<PostComments>? Comments { get; set; }
    }
}
