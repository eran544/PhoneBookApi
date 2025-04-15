using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace PhoneBookApi.Models
{
    public class Contact
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        [BsonIgnoreIfNull]
        public ObjectId? UserId { get; set; }

        [Required]
        [RegularExpression("^[A-Za-z]+$", ErrorMessage = "First name must contain only letters")]
        public string FirstName { get; set; } = null!;

        [RegularExpression("^[A-Za-z]+$", ErrorMessage = "Last name must contain only letters")]
        public string? LastName { get; set; }

        [Required]
        [RegularExpression("^[0-9]+$", ErrorMessage = "Phone number must contain only digits")]
        public string PhoneNumber { get; set; } = null!;

        public string? Address { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

}
