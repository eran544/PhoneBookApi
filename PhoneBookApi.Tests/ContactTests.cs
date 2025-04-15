using FluentAssertions;
using MongoDB.Bson;
using PhoneBookApi.DTOs.Requests;
using PhoneBookApi.Tests.Helpers;

namespace PhoneBookApi.Tests
{
    [Collection("MongoTest")]
    public class ContactTests : MongoTestBase
    {
        [Fact]
        public async Task CreateContact_ShouldInsertPrivateContact()
        {
            var userId = ObjectId.GenerateNewId();

            var contactId = await Handler.CreateContactAsync(new CreateContactRequest
            {
                FirstName = "PrivateUser",
                PhoneNumber = "5551234",
                IsGlobal = false
            }, userId);

            contactId.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateGlobalContact_ShouldInsertWithoutUserId()
        {
            var contactId = await Handler.CreateContactAsync(new CreateContactRequest
            {
                FirstName = "AdminOnly",
                PhoneNumber = "5559999",
                IsGlobal = true
            }, null);

            contactId.Should().NotBeNull();
        }
    }
}