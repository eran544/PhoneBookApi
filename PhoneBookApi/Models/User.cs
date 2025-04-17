using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace PhoneBookApi.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }
        [BsonElement("username")]
        public string Username { get; set; } = null!;
        [BsonElement("passwordHash")]
        public string PasswordHash { get; set; } = null!;
        [BsonElement("email")]
        [EmailAddress]
        public string Email { get; set; } = null!;
        [BsonElement("firstName")]
        public string FirstName { get; set; } = null!;
        [BsonElement("lastName")]
        public string LastName { get; set; } = null!;

        [BsonIgnoreIfNull]
        [BsonElement("role")]
        public Role? Role { get; set; }  // Only set for admin, if role = null assums User
        [BsonElement("createdAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; }
    }

    public enum Role
    {
        Admin,
        User // If in future we would like to add the role into the FWT token or for any other purpose.
    }
}
