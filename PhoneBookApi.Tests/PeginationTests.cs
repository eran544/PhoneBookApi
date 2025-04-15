using FluentAssertions;
using MongoDB.Bson;
using PhoneBookApi.DTOs.Requests;
using PhoneBookApi.Tests.Helpers;

namespace PhoneBookApi.Tests
{
    [Collection("MongoTest")]
    public class PaginationTests : MongoTestBase
    {
        [Fact]
        public async Task GetContactsAsync_ShouldReturnPaginatedResults()
        {
            var userId = ObjectId.GenerateNewId();

            for (int i = 0; i < 25; i++)
            {
                await Handler.CreateContactAsync(new CreateContactRequest
                {
                    FirstName = $"Contact{i:D2}",
                    PhoneNumber = $"555{i:D4}",
                    IsGlobal = false
                }, userId);
            }

            var page1 = await Handler.GetContactsAsync(userId, 1);
            var page2 = await Handler.GetContactsAsync(userId, 2);
            var page3 = await Handler.GetContactsAsync(userId, 3);

            page1.Count.Should().Be(10);
            page2.Count.Should().Be(10);
            page3.Count.Should().Be(5);
        }
    }
}