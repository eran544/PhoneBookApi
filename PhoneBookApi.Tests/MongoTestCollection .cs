using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoneBookApi.Tests
{
    [CollectionDefinition("MongoTest")]
    public class MongoTestCollection : ICollectionFixture<MongoTestFixture> { }

    public class MongoTestFixture
    {
    }
}
