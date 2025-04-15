using Xunit;
using FluentAssertions;
using System.Security.Claims;
using MongoDB.Bson;
using Microsoft.Extensions.Configuration;
using PhoneBookApi.Handlers;
using PhoneBookApi.Models;

namespace PhoneBookApi.Tests
{
    [Collection("MongoTest")]
    public class JwtHandlerTests
    {
        private readonly JwtHandler _jwtHandler;
        private readonly string adminPassword;

        public JwtHandlerTests()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Test.json", optional: false)
                .Build();

            _jwtHandler = new JwtHandler(config);
            adminPassword = config["AdminPassword"]!;
        }

        [Fact]
        public void GenerateToken_ShouldReturn_ValidJwt()
        {
            var user = new User
            {
                Id = ObjectId.GenerateNewId(),
                Username = "admin",
                Role = Role.Admin
            };

            var token = _jwtHandler.GenerateToken(user);
            token.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void ValidateToken_ShouldReturnPrincipal_WhenValid()
        {
            var user = new User
            {
                Id = ObjectId.GenerateNewId(),
                Username = "testuser",
                Role = Role.User
            };

            var token = _jwtHandler.GenerateToken(user);
            var principal = _jwtHandler.ValidateToken(token);

            principal.Should().NotBeNull();
            principal!.Identity!.IsAuthenticated.Should().BeTrue();

            var nameId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = principal.FindFirst(ClaimTypes.Role)?.Value;

            nameId.Should().Be(user.Id.ToString());
            role.Should().BeNull();
        }
        [Fact]
        public void ValidateToken_ShouldIncludeRole_WhenAdmin()
        {
            var user = new User
            {
                Id = ObjectId.GenerateNewId(),
                Username = "admin",
                Role = Role.Admin
            };

            var token = _jwtHandler.GenerateToken(user);
            var principal = _jwtHandler.ValidateToken(token);

            var role = principal?.FindFirst(ClaimTypes.Role)?.Value;
            role.Should().Be("Admin");
        }
    }
}