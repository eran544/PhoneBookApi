using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace PhoneBookApi.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        public string Username { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;

        public string Email { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;

        [BsonIgnoreIfNull]
        public Role? Role { get; set; }  // Only set for admin
    }

    public enum Role
    {
        Admin,
        IT,
        User
    }
}
