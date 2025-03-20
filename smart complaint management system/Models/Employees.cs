using System.ComponentModel.DataAnnotations;

namespace smart_complaint_management_system.Models
{
    public class Employees
    {
        [Key]
        public int EmployeeId { get; set; }

        [Required]
        public string EmployeeName { get; set; }  

        [Required, EmailAddress]
        public string EmployeeEmail { get; set; }

        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be exactly 10 digits.")]
        public string PhoneNumber { get; set; }

        [Required]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
        ErrorMessage = "Password must be at least 8 characters long and include a letter, a number, and a special character.")]
        public string Password { get; set; }

        [Required]
        public bool IsHandlingComplaint { get; set; }

        [Required]
        public string Category { get; set; }

        public ICollection<Complaints> Complaints { get; set; } = new List<Complaints>();
    }
}
