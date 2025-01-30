using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PracticeWithC1stApproach.Models
{
    public class EmployeeModel
    {
        [Key]
        public int EmployeeId { get; set; }  // Primary key

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string FirstName { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string LastName { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string Position { get; set; }  // Job title, e.g., Manager, Developer

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Salary { get; set; }

        [Required]
        public DateTime HireDate { get; set; }  // Date of joining

        [EmailAddress]
        [Column(TypeName = "varchar(100)")]
        public string Email { get; set; }

        [Phone]
        [Column(TypeName = "varchar(100)")]
        public string PhoneNumber { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string Address { get; set; }

        public bool IsActive { get; set; } = true;  // Whether the employee is currently active
    }
}
