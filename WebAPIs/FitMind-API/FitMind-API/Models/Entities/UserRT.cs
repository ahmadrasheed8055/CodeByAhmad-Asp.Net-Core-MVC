using System.ComponentModel.DataAnnotations;

namespace FitMind_API.Models.Entities
{
    public class UserRT
    {
        [Key] // Primary key
        public int Id { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Token is required")]
        [StringLength(100, ErrorMessage = "Token length cannot exceed 100 characters")]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "Status is required")]
        public int Status { get; set; }

        [Required(ErrorMessage = "Expiry date is required")]
        public DateTime ExpiryDate { get; set; } 

        public DateTime InsertedAt { get; set; }
        public DateTime UpdatedAt { get; set; } 
    }
}
