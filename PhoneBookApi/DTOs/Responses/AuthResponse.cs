using PhoneBookApi.Models;

namespace PhoneBookApi.DTOs.Responses
{
    public class AuthResponse
    {
        public string token { get; set; } = null!;
        public int expiresIn { get; set; }
        public string username { get; set; } = null!;
        public Role? Role { get; set; }
    }
}
