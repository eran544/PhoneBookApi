using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using PhoneBookApi.Controllers;
using PhoneBookApi.DTOs.Requests;
using PhoneBookApi.DTOs.Responses;
using PhoneBookApi.Handlers;
using PhoneBookApi.Models;
using PhoneBookApi.Tests.Helpers;

namespace PhoneBookApi.Tests
{
    [Collection("MongoTest")]
    public class ContactIntegrationTests : MongoTestBase
    {
        private readonly JwtHandler _jwtHandler;

        public ContactIntegrationTests()
        {
            _jwtHandler = new JwtHandler(Config);
        }

        [Fact]
        public async Task User_ShouldSeeGlobalAndPrivateContacts()
        {
            // Arrange
            var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<PhoneBookController>();
            //var controller = new PhoneBookController(logger, Handler);

            // Admin token (uses seeded admin)
            var admin = TestUserConstants.AdminUser;
            var adminToken = _jwtHandler.GenerateToken(admin);
            var adminController = JwtTestHelper.CreatePhoneBookControllerWithToken(adminToken, Handler, _jwtHandler);

            // Regular user
            var user = new User
            {
                Id = ObjectId.GenerateNewId(),
                Role = Role.User,
                Username = "testuser"
            };
            var userToken = _jwtHandler.GenerateToken(user);
            var userController = JwtTestHelper.CreatePhoneBookControllerWithToken(userToken, Handler, _jwtHandler);

            await adminController.CreateContact(new CreateContactRequest
            {
                FirstName = "Global1",
                PhoneNumber = "1000",
                IsGlobal = true
            });

            await adminController.CreateContact(new CreateContactRequest
            {
                FirstName = "Global2",
                PhoneNumber = "1001",
                IsGlobal = true
            });

            await userController.CreateContact(new CreateContactRequest
            {
                FirstName = "Private1",
                PhoneNumber = "2000",
                IsGlobal = false
            });

            await userController.CreateContact(new CreateContactRequest
            {
                FirstName = "Private2",
                PhoneNumber = "2001",
                IsGlobal = false
            });

            // Act: User gets contacts
            var result = await userController.GetContacts(1);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            var response = okResult.Value as GetContactsResponse;
            response.Should().NotBeNull();

            var contacts = response?.Contacts;
            contacts.Should().NotBeNull();
            contacts!.Should().HaveCount(4);

            var names = contacts.Select(c => c.FirstName).ToList();
            names.Should().Contain(["Global1", "Global2", "Private1", "Private2"]);
        }

        [Fact]
        public async Task User_CannotEdit_GlobalContact()
        {
            // Admin creates global contact
            var admin = TestUserConstants.AdminUser;
            var adminToken = _jwtHandler.GenerateToken(admin);

            var adminController = JwtTestHelper.CreatePhoneBookControllerWithToken(adminToken, Handler, _jwtHandler);
            var result = await adminController.CreateContact(new CreateContactRequest
            {
                FirstName = "GlobalEditTest",
                PhoneNumber = "0001",
                IsGlobal = true
            });
            result.Should().NotBeNull();
            var CreateResult = result.Result as CreatedResult;
            CreateResult.Should().NotBeNull();
            var responseValue = CreateResult.Value as CreateContactResponse;
            responseValue.Should().NotBeNull();
            var contactId = responseValue.ContactId;
            contactId.Should().NotBeNull();

            // Regular user attempts edit
            var user = new User { Id = ObjectId.GenerateNewId(), Role = Role.User };
            var userToken = _jwtHandler.GenerateToken(user);
            var userController = JwtTestHelper.CreatePhoneBookControllerWithToken(userToken, Handler, _jwtHandler);

            var UpdateResult = await userController.UpdateContact(contactId.ToString()!, new UpdateContactRequest
            {
                FirstName = "BlockedEdit"
            });

            // Fix: Cast the ActionResult to ObjectResult to access StatusCode
            var objectResult = UpdateResult.Result as ObjectResult;
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be(403);
        }
    }
}

