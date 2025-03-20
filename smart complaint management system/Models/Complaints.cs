using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace smart_complaint_management_system.Models
{
    public class Complaints
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Location { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        [Required]
        public string ComplaintType { get; set; } 

        public byte[]? Photo { get; set; }

        [Required]
        public string ComplaintDescription { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string? Status { get; set; } = "Pending"; 

        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }

        public int? EmployeeId { get; set; }  

        [ForeignKey("EmployeeId")]
        public Employees AssignedEmployee { get; set; } 
    }
}
