using System.ComponentModel.DataAnnotations;

namespace HomeBankingApi.models
{
    public class User
    {

        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "The first name is required.")]
        [MaxLength(24, ErrorMessage = "The name cannot exceed 24 characters.")]
        [RegularExpression("^[a-zA-ZáéíóúÁÉÍÓÚñÑ ]+$", ErrorMessage = "The name can only contain letters.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "The last name is required.")]
        [MaxLength(24, ErrorMessage = "The last name cannot exceed 24 characters.")]
        [RegularExpression("^[a-zA-ZáéíóúÁÉÍÓÚñÑ ]+$", ErrorMessage = "The last name can only contain letters.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "The email is required.")]
        [EmailAddress(ErrorMessage = "The email format is invalid.")]
        [MaxLength(64, ErrorMessage = "The email cannot exceed 64 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "The email must be in a valid format.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "The password is required.")]
        [MinLength(8, ErrorMessage = "The password must be at least 8 characters.")]
        public string PasswordHash { get; set; }

        [Phone(ErrorMessage = "The phone number format is invalid.")]
        [MaxLength(15, ErrorMessage = "The phone number cannot exceed 15 characters.")]
        [RegularExpression(@"^\+?[0-9]{7,15}$", ErrorMessage = "The phone number must be in a valid format.")]
        public string Phone { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime LastUpdatedDate { get; set; } = DateTime.UtcNow;

        public bool IsVerified { get; set; } = false;

    }
}
