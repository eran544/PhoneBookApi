using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using PhoneBookApi.Handlers;
using PhoneBookApi.Models;
using System.IO;

namespace PhoneBookApi.Tests.Helpers
{
    public abstract class MongoTestBase
    {
        protected readonly IConfiguration Config;
        protected readonly IMongoDatabase Db;
        protected readonly MongoClient Client;
        protected readonly DataHandler Handler;

        protected MongoTestBase()
        {
            Config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Test.json", optional: false)
                .Build();

            var connectionString = Config["MongoDbSettings:ConnectionString"]!;
            var dbName = Config["MongoDbSettings:DatabaseName"]!;

            Client = new MongoClient(connectionString);
            Db = Client.GetDatabase(dbName);

            // Clean state before each test class runs (you can move this to [SetUp] if needed)
            Db.DropCollection("Users");
            Db.DropCollection("Contacts");

            SeedAdminUser(); // insert admin user to the users DB

            var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<DataHandler>();
            Handler = new DataHandler(Db, logger);
        }

        private void SeedAdminUser()
        {
            var users = Db.GetCollection<User>("Users");

            var existing = users.Find(u => u.Id == ObjectId.Parse("67fd395b5d6182d5f93f1060")).FirstOrDefault();
            if (existing != null) return;

            var admin = new User
            {
                Id = ObjectId.Parse("67fd395b5d6182d5f93f1060"),
                Username = "Admin",
                Email = "admin@admin.com",
                PasswordHash = "$2a$11$9jC54awnUvrTBcdFggYo4O9LyOEDXGGfaS3SCQZceTmj2P9SWpgmu",
                Role = Role.Admin,
                FirstName = "Eran",
                LastName = "Salomon",
                CreatedAt = DateTimeOffset.FromUnixTimeMilliseconds(1744708950478).UtcDateTime,
            };

            users.InsertOne(admin);
        }

    }
}