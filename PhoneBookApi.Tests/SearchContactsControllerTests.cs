using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using PhoneBookApi.DTOs.Requests;
using PhoneBookApi.DTOs.Responses;
using PhoneBookApi.Handlers;
using PhoneBookApi.Models;
using PhoneBookApi.Tests.Helpers;

namespace PhoneBookApi.Tests
{
    [Collection("MongoTest")]
    public class SearchContactsControllerTests : MongoTestBase
    {
        private readonly JwtHandler _jwtHandler;

        public SearchContactsControllerTests()
        {
            _jwtHandler = new JwtHandler(Config);
        }

        [Fact]
        public async Task Search_IsCaseInsensitive()
        {
            var user = new User { Id = ObjectId.GenerateNewId(), Role = Role.User };
            var token = _jwtHandler.GenerateToken(user);
            var controller = JwtTestHelper.CreatePhoneBookControllerWithToken(token, Handler, _jwtHandler);

            await controller.CreateContact(new CreateContactRequest
            {
                FirstName = "Eran",
                PhoneNumber = "1234"
            });

            var request = new SearchContactsRequest
            {
                Query = "eRaN",
                Page = 1,
                SearchField = "FirstName"
            };

            var result = await controller.SearchContacts(request);
            var ok = result.Result as OkObjectResult;
            var response = ok!.Value as GetContactsResponse;

            response!.Contacts.Should().ContainSingle(c => c.FirstName == "Eran");
        }

        [Fact]
        public async Task Search_WithAllField_MatchesMultipleFields()
        {
            var user = new User { Id = ObjectId.GenerateNewId(), Role = Role.User };
            var token = _jwtHandler.GenerateToken(user);
            var controller = JwtTestHelper.CreatePhoneBookControllerWithToken(token, Handler, _jwtHandler);

            await controller.CreateContact(new CreateContactRequest
            {
                FirstName = "MatchMe",
                PhoneNumber = "55555"
            });

            await controller.CreateContact(new CreateContactRequest
            {
                FirstName = "Ignore",
                PhoneNumber = "match-here"
            });

            var request = new SearchContactsRequest
            {
                Query = "match",
                Page = 1,
                SearchField = "all" // should behave like null
            };

            var result = await controller.SearchContacts(request);
            var ok = result.Result as OkObjectResult;
            var response = ok!.Value as GetContactsResponse;

            response!.Contacts.Should().HaveCount(2);
        }

        [Fact]
        public async Task Search_ShouldReturnOnlyAccessibleContacts()
        {
            var user = new User { Id = ObjectId.GenerateNewId(), Role = Role.User };
            var token = _jwtHandler.GenerateToken(user);
            var userController = JwtTestHelper.CreatePhoneBookControllerWithToken(token, Handler, _jwtHandler);

            // Global contact
            await Handler.CreateContactAsync(new CreateContactRequest
            {
                FirstName = "VisibleToAll",
                PhoneNumber = "000",
                IsGlobal = true
            }, null);

            // Private to this user
            await userController.CreateContact(new CreateContactRequest
            {
                FirstName = "PrivateUserContact",
                PhoneNumber = "123"
            });

            // Private to other user
            await Handler.CreateContactAsync(new CreateContactRequest
            {
                FirstName = "OtherUserSecret",
                PhoneNumber = "999"
            }, ObjectId.GenerateNewId());

            var result = await userController.SearchContacts(new SearchContactsRequest
            {
                Query = "user",
                Page = 1,
                SearchField = null
            });

            var ok = result.Result as OkObjectResult;
            var response = ok!.Value as GetContactsResponse;

            response!.Contacts.Should().ContainSingle(c => c.FirstName == "PrivateUserContact");
            response.Contacts.Should().NotContain(c => c.FirstName == "OtherUserSecret");
        }

        [Theory]
        [InlineData("FirstName")]
        [InlineData("LastName")]
        [InlineData("PhoneNumber")]
        [InlineData("Email")]
        [InlineData("")]
        public async Task Search_ShouldReturn_Match_WhenFieldMatches(string field)
        {
            var user = new User { Id = ObjectId.GenerateNewId(), Role = Role.User };
            var token = _jwtHandler.GenerateToken(user);
            var controller = JwtTestHelper.CreatePhoneBookControllerWithToken(token, Handler, _jwtHandler);

            // All fields will contain "Searchable" value
            await controller.CreateContact(new CreateContactRequest
            {
                FirstName = "Searchable",
                LastName = "Searchable",
                PhoneNumber = "Searchable",
                Email = "searchable@example.com"
            });

            var request = new SearchContactsRequest
            {
                Query = "search",
                Page = 1,
                SearchField = field
            };

            var result = await controller.SearchContacts(request);
            var ok = result.Result as OkObjectResult;
            var response = ok!.Value as GetContactsResponse;

            response.Should().NotBeNull();
            response!.Contacts.Should().NotBeEmpty();
        }

        [Theory]
        [InlineData("FirstName")]
        [InlineData("LastName")]
        [InlineData("PhoneNumber")]
        [InlineData("Email")]
        [InlineData("")]
        public async Task Search_ShouldReturnEmpty_WhenNoMatch(string field)
        {
            var user = new User { Id = ObjectId.GenerateNewId(), Role = Role.User };
            var token = _jwtHandler.GenerateToken(user);
            var controller = JwtTestHelper.CreatePhoneBookControllerWithToken(token, Handler, _jwtHandler);

            // Add a contact that shouldn't match
            await controller.CreateContact(new CreateContactRequest
            {
                FirstName = "DoesNotMatch",
                LastName = "StillWrong",
                PhoneNumber = "0000",
                Email = "nope@example.com"
            });

            var request = new SearchContactsRequest
            {
                Query = "searchthis",
                Page = 1,
                SearchField = field
            };

            var result = await controller.SearchContacts(request);
            var ok = result.Result as OkObjectResult;
            var response = ok!.Value as GetContactsResponse;

            response.Should().NotBeNull();
            response!.Contacts.Should().BeEmpty();
        }
    }
}