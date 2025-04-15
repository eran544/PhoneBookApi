using Xunit;
using FluentAssertions;
using PhoneBookApi.DTOs.Requests;
using PhoneBookApi.Tests.Helpers;

namespace PhoneBookApi.Tests
{
    [Collection("MongoTest")]
    public class UserRegistrationTests : MongoTestBase
    {
        [Fact]
        public async Task RegisterUser_ShouldInsertUser()
        {
            var request = new RegisterRequest
            {
                Username = "integrationuser",
                Email = "integration@example.com",
                Password = "Secure123!",
                FirstName = "Jane",
                LastName = "Doe"
            };

            var result = await Handler.RegisterUser(request);

            result.Should().NotBeNull();
            result!.Username.Should().Be(request.Username);
        }

        [Fact]
        public async Task RegisterUser_ShouldReturnNull_WhenEmailExists()
        {
            await Handler.RegisterUser(new RegisterRequest
            {
                Username = "existing",
                Email = "dupe@example.com",
                Password = "Pass",
                FirstName = "Test",
                LastName = "One"
            });

            var duplicate = new RegisterRequest
            {
                Username = "newuser",
                Email = "dupe@example.com",
                Password = "Pass",
                FirstName = "Duped",
                LastName = "Email"
            };

            var result = await Handler.RegisterUser(duplicate);

            result.Should().BeNull();
        }
    }
}