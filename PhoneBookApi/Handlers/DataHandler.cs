using MongoDB.Driver;
using PhoneBookApi.DTOs;
using PhoneBookApi.Models;
using System.Linq;

namespace PhoneBookApi.Handlers
{
    public class DataHandler(IMongoDatabase database, ILogger<DataHandler> logger)
    {
        private readonly IMongoDatabase _database = database;
        private readonly IMongoCollection<User> usersCollection = database.GetCollection<User>("Users");
        private readonly ILogger<DataHandler> _logger = logger;

        public async Task<User?> RegisterUser(RegisterRequest request)
        {
            try
            {
                User? user = null;
                if ((await usersCollection.FindAsync(u => u.Username == request.Username || u.Email == request.Email)).Any())
                {
                    return null;
                }
                user = new User()
                {
                    Username = request.Username,
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                };
                await usersCollection.InsertOneAsync(user);
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while registering user");
                return null;
            }
        }

        public bool TryGetUser(LoginRequest request, out User? user)
        {
            var username = request.Username;
            user = user = usersCollection
                .AsQueryable()
                .FirstOrDefault(u => u.Username == username);
            if (user != null)
            {
                bool PasswordMatch = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
                if (!PasswordMatch)
                {
                    user = null;
                    _logger.LogInformation("user {username} logged with incorrect password", username);
                }
            }

            return user != null;
        }
    }
}
