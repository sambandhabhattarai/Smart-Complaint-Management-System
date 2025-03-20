using System.ComponentModel.DataAnnotations;

namespace smart_complaint_management_system.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be exactly 10 digits.")]
        public string? Phone { get; set; }

        public DateTime? DateofBirth { get; set; }

        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
        ErrorMessage = "Password must be at least 8 characters long and include a letter, a number, and a special character.")]
        public string? Password { get; set; }

        public byte[]? Photo { get; set; }
        public ICollection<Complaints>? Complaints { get; set; }
    }
}
