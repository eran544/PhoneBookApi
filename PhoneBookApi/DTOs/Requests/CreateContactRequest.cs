using System.ComponentModel.DataAnnotations;
namespace PhoneBookApi.DTOs.Requests
{
    public class CreateContactRequest
    {
        [Required]
        [RegularExpression("^[A-Za-z]+$", ErrorMessage = "First name must contain only letters")]
        public string FirstName { get; set; } = null!;

        [RegularExpression("^[A-Za-z]+$", ErrorMessage = "Last name must contain only letters")]
        public string? LastName { get; set; }

        [Required]
        [RegularExpression("^[0-9]+$", ErrorMessage = "Phone number must contain only digits")]
        public string PhoneNumber { get; set; } = null!;

        public string? Address { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address format")]
        public string? Email { get; set; }
        public bool IsGlobal { get; set; } = false;
    }

}
