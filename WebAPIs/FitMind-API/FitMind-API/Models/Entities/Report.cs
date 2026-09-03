using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitMind_API.Models.Entities
{
    public class Report
    {
        [Key]
        public int ReportId { get; set; }

        [Required]
        [StringLength(50)]
        public string TargetType { get; set; } // "Post" or "Comment"

        [Required]
        public int TargetId { get; set; }

        [Required]
        [ForeignKey("ReporterUser")]
        public int ReporterUserId { get; set; }

        public virtual AppUsers ReporterUser { get; set; }

        [Required]
        [StringLength(500)]
        public string Reason { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // "Pending", "Resolved", "Dismissed"

        [ForeignKey("ReviewedByAdmin")]
        public int? ReviewedByAdminId { get; set; }

        public virtual AdminUser? ReviewedByAdmin { get; set; }
    }
}
