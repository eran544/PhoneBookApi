using System.ComponentModel.DataAnnotations;

namespace PhoneBookApi.DTOs.Requests
{
    public class RegisterRequest
    {
        [Required]
        [MinLength(3, ErrorMessage = "Username must be at least 3 characters")]
        public string Username { get; set; } = null!;

        [Required]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = null!;

        [Required]
        [EmailAddress(ErrorMessage = "Email format is invalid")]
        public string Email { get; set; } = null!;

        [Required]
        [RegularExpression("^[A-Za-z]+$", ErrorMessage = "First name must contain only letters")]
        public string FirstName { get; set; } = null!;

        [Required]
        [RegularExpression("^[A-Za-z]+$", ErrorMessage = "Last name must contain only letters")]
        public string LastName { get; set; } = null!;
    }

}
