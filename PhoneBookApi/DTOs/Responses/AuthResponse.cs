using PhoneBookApi.Models;

namespace PhoneBookApi.DTOs.Responses
{
    public class AuthResponse : BaseResponse
    {
        public string Token { get; set; } = null!;
        public int ExpiresIn { get; set; }
        public string Username { get; set; } = null!;
    }
}
