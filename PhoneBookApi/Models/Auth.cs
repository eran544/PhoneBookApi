using PhoneBookApi.DTOs;
using PhoneBookApi.Handlers;
using PhoneBookApi.Repositories;

namespace PhoneBookApi.Models
{
    public class Auth(DataHandler dataHandler, JwtHandler jwtHandler) : IAuth
    {
        private readonly DataHandler _dataHandler = dataHandler;
        private readonly JwtHandler _jwtHandler = jwtHandler;
        public async Task<string> Register(RegisterRequest request)
        {
            if (await _dataHandler.RegisterUser(request) != null)
            {

            }
            throw new NotImplementedException();
        }
    }
}
