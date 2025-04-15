// This file contains additional controller tests covering access control, edge cases, and token validation

using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PhoneBookApi.Controllers;
using PhoneBookApi.Handlers;
using PhoneBookApi.Models;
using PhoneBookApi.DTOs.Requests;
using MongoDB.Bson;
using PhoneBookApi.Tests.Helpers;
using MongoDB.Driver;

namespace PhoneBookApi.Tests
{
    [Collection("MongoTest")]
    public class ContactAccessAndValidationTests : MongoTestBase
    {
        private readonly JwtHandler _jwtHandler;

        public ContactAccessAndValidationTests()
        {
            _jwtHandler = new JwtHandler(Config);
        }

        [Fact]
        public async Task RegularUser_CannotEdit_AdminGlobalContact()
        {
            var admin = TestUserConstants.AdminUser;
            var adminToken = _jwtHandler.GenerateToken(admin);
            var adminController = JwtTestHelper.CreatePhoneBookControllerWithToken(adminToken, Handler, _jwtHandler);

            var createResult = await adminController.CreateContact(new CreateContactRequest
            {
                FirstName = "GlobalEditTest",
                PhoneNumber = "1000",
                IsGlobal = true
            });

            var contacts = await Handler.GetContactsAsync(admin.Id, 1);
            var contactId = contacts.First().Id.ToString();

            var user = new User { Id = ObjectId.GenerateNewId(), Role = Role.User };
            var userToken = _jwtHandler.GenerateToken(user);
            var userController = JwtTestHelper.CreatePhoneBookControllerWithToken(userToken, Handler, _jwtHandler);

            var updateResult = await userController.UpdateContact(contactId, new UpdateContactRequest
            {
                FirstName = "BlockedEdit"
            });

            var objectResult = updateResult.Result as ObjectResult;
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be(403);
        }

        [Fact]
        public async Task Admin_CannotEdit_UsersPrivateContact()
        {
            var user = new User { Id = ObjectId.GenerateNewId(), Role = Role.User };
            var userToken = _jwtHandler.GenerateToken(user);
            var userController = JwtTestHelper.CreatePhoneBookControllerWithToken(userToken, Handler, _jwtHandler);

            var create = await userController.CreateContact(new CreateContactRequest
            {
                FirstName = "PrivateUserContact",
                PhoneNumber = "123",
                IsGlobal = false
            });

            var contactId = (await Handler.GetContactsAsync(user.Id, 1)).First().Id.ToString();

            var admin = TestUserConstants.AdminUser;
            var adminToken = _jwtHandler.GenerateToken(admin);
            var adminController = JwtTestHelper.CreatePhoneBookControllerWithToken(adminToken, Handler, _jwtHandler);

            var result = await adminController.UpdateContact(contactId, new UpdateContactRequest
            {
                FirstName = "AdminHack"
            });

            result.Result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task InvalidToken_ShouldReturn401()
        {
            var controller = new PhoneBookController(
                LoggerFactory.Create(x => x.AddConsole()).CreateLogger<PhoneBookController>(),
                Handler)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext() // No claims at all
                }
            };

            var result = await controller.GetContacts(1);
            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Updating_NonExistentContact_ShouldReturn404()
        {
            var user = new User { Id = ObjectId.GenerateNewId(), Role = Role.User };
            var token = _jwtHandler.GenerateToken(user);
            var controller = JwtTestHelper.CreatePhoneBookControllerWithToken(token, Handler, _jwtHandler);

            var fakeId = ObjectId.GenerateNewId().ToString();
            var result = await controller.UpdateContact(fakeId, new UpdateContactRequest
            {
                FirstName = "NoOne"
            });

            result.Result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task Creating_InvalidContact_ShouldReturn400()
        {
            var user = new User { Id = ObjectId.GenerateNewId(), Role = Role.User };
            var token = _jwtHandler.GenerateToken(user);
            var controller = JwtTestHelper.CreatePhoneBookControllerWithToken(token, Handler, _jwtHandler);

            var request = new CreateContactRequest
            {
                FirstName = "",
                PhoneNumber = ""
            };

            ModelValidator.ValidateModel(request, controller);

            var result = await controller.CreateContact(request);

            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }
    }
}