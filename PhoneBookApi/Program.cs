using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using PhoneBookApi.Data;
using PhoneBookApi.Filters;
using PhoneBookApi.Handlers;
using PhoneBookApi.Models;
using System.Text;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add .env variables (loaded by Docker or local env)
        builder.Configuration.AddEnvironmentVariables();

        var configuration = builder.Configuration;
        var services = builder.Services;

        // Load MongoDB settings
        var mongoUser = Environment.GetEnvironmentVariable("ADMIN_USERNAME") ?? configuration["ADMIN_USERNAME"] ?? "Admin";
        var mongoPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD") ?? configuration["ADMIN_PASSWORD"] ?? "password";
        var mongoPort = Environment.GetEnvironmentVariable("MONGO_PORT") ?? configuration["MONGO_PORT"] ?? "27017";
        var mongoDb = Environment.GetEnvironmentVariable("MONGO_DB_NAME") ?? configuration["MONGO_DB_NAME"] ?? "PhoneBookDb";

        var mongoConnectionString = $"mongodb://{mongoUser}:{mongoPassword}@mongo:{mongoPort}";

        var mongoSettings = new MongoDbSettings
        {
            ConnectionString = mongoConnectionString,
            DatabaseName = mongoDb
        };

        services.AddSingleton(mongoSettings);
        services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoSettings.ConnectionString));
        services.AddSingleton(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            return client.GetDatabase(mongoSettings.DatabaseName);
        });

        // Load JWT settings
        var jwtSecret = Environment.GetEnvironmentVariable("Jwt__SecretKey") ?? configuration["Jwt:SecretKey"];
        var jwtIssuer = Environment.GetEnvironmentVariable("Jwt__Issuer") ?? configuration["Jwt:Issuer"];
        var jwtAudience = Environment.GetEnvironmentVariable("Jwt__Audience") ?? configuration["Jwt:Audience"];

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret!)),
                    ValidateIssuer = true,
                    ValidIssuer = jwtIssuer,
                    ValidateAudience = true,
                    ValidAudience = jwtAudience,
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddControllers(options =>
        {
            options.Filters.Add<ValidateModelAttribute>();
        }).AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        });

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        services.AddSingleton<DataHandler>();
        services.AddSingleton<JwtHandler>();

        var app = builder.Build();

        // Seed admin user
        SeedAdminUser(app.Services, configuration);

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }

    private static void SeedAdminUser(IServiceProvider services, IConfiguration configuration)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();
        var users = db.GetCollection<User>("Users");

        var email = Environment.GetEnvironmentVariable("ADMIN_EMAIL") ?? configuration["ADMIN_EMAIL"] ?? "admin@admin.com";
        var username = Environment.GetEnvironmentVariable("ADMIN_USERNAME") ?? configuration["ADMIN_USERNAME"] ?? "Admin";
        var password = Environment.GetEnvironmentVariable("ADMIN_PASSWORD") ?? configuration["ADMIN_PASSWORD"] ?? "SpecialAdminUser1!";

        if (string.IsNullOrWhiteSpace(password))
        {
            Console.WriteLine("ADMIN_PASSWORD not set — skipping admin seed.");
            return;
        }

        var existing = users.Find(u => u.Email == email).FirstOrDefault();
        if (existing != null)
        {
            Console.WriteLine("Admin already exists.");
            return;
        }

        var admin = new User
        {
            Username = username,
            Email = email,
            FirstName = "Admin",
            LastName = "User",
            Role = Role.Admin,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            CreatedAt = DateTime.UtcNow
        };

        users.InsertOne(admin);
        Console.WriteLine("Admin user seeded.");
    }
}
