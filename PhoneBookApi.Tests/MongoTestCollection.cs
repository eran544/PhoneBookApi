namespace PhoneBookApi.Tests
{
    [CollectionDefinition("MongoTest")]
    public class MongoTestCollection : ICollectionFixture<MongoTestFixture> { }

    public class MongoTestFixture
    {
    }
}
