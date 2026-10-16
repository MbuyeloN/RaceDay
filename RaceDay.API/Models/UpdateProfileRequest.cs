using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Models
{
    public class UpdateProfileRequest
    {
        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = "";

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = "";

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = "";

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }
    }
}
