using PhoneBookApi.DTOs;

namespace PhoneBookApi.Repositories
{
    public interface IAuth
    {
        Task<string> Register(RegisterRequest request);
    }
}
