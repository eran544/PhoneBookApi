using MongoDB.Bson;
using PhoneBookApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoneBookApi.Tests.Helpers
{
    public static class TestUserConstants
    {
        public static User AdminUser => new()
        {
            Id = ObjectId.Parse("67fd395b5d6182d5f93f1060"),
            Username = "Admin",
            Role = Role.Admin
        };
    }
}
