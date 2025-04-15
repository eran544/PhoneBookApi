using PhoneBookApi.DTOs.Requests;

namespace PhoneBookApi.Repositories
{
    public interface IAuth
    {
        Task<string> Register(RegisterRequest request);
    }
}
