using System.ComponentModel.DataAnnotations;

namespace PhoneBookApi.DTOs.Requests
{
    public class UpdateContactRequest
    {
        [RegularExpression("^[A-Za-z]+$")]
        public string? FirstName { get; set; }

        [RegularExpression("^[A-Za-z]+$")]
        public string? LastName { get; set; }

        [RegularExpression("^[0-9]+$")]
        public string? PhoneNumber { get; set; }

        public string? Address { get; set; }

        [EmailAddress]
        public string? Email { get; set; }
    }

}
