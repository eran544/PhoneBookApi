using FluentAssertions;
using MongoDB.Bson;
using PhoneBookApi.DTOs.Requests;
using PhoneBookApi.Handlers;
using PhoneBookApi.Tests.Helpers;

namespace PhoneBookApi.Tests
{
    [Collection("MongoTest")]
    public class SearchContactsTests : MongoTestBase
    {
        private readonly JwtHandler _jwtHandler;

        public SearchContactsTests()
        {
            _jwtHandler = new JwtHandler(Config);
        }

        [Fact]
        public async Task Search_ShouldReturn_MatchingPrivateAndGlobalContacts()
        {
            var userId = ObjectId.GenerateNewId();

            // Add global contacts
            await Handler.CreateContactAsync(new CreateContactRequest
            {
                FirstName = "GlobalMatch",
                PhoneNumber = "111",
                IsGlobal = true
            }, null);

            // Add private user contact
            await Handler.CreateContactAsync(new CreateContactRequest
            {
                FirstName = "PrivateMatch",
                PhoneNumber = "222",
                IsGlobal = false
            }, userId);

            // Should match both global and private
            var results = await Handler.SearchContactsAsync("match", null, 1, userId);

            results.Should().HaveCount(2);
            results.Select(c => c.FirstName).Should().Contain(new[] { "GlobalMatch", "PrivateMatch" });
        }

        [Fact]
        public async Task Search_WithInvalidField_ShouldThrow()
        {
            var userId = ObjectId.GenerateNewId();

            Func<Task> act = async () => await Handler.SearchContactsAsync("test", "InvalidField", 1, userId);

            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*Invalid searchField*");
        }

        [Fact]
        public async Task Search_WithEmptyQuery_ShouldThrow()
        {
            var userId = ObjectId.GenerateNewId();

            Func<Task> act = async () => await Handler.SearchContactsAsync("", null, 1, userId);

            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*Search query cannot be empty*");
        }

        [Fact]
        public async Task Search_ShouldRespectPagination()
        {
            var userId = ObjectId.GenerateNewId();

            for (int i = 0; i < 15; i++)
            {
                await Handler.CreateContactAsync(new CreateContactRequest
                {
                    FirstName = $"Paged{i}",
                    PhoneNumber = $"555{i:D4}",
                    IsGlobal = false
                }, userId);
            }

            var page1 = await Handler.SearchContactsAsync("paged", null, 1, userId);
            var page2 = await Handler.SearchContactsAsync("paged", null, 2, userId);

            page1.Should().HaveCount(10);
            page2.Should().HaveCount(5);
        }
    }

}
