using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PhoneBookApi.Controllers;
using PhoneBookApi.Handlers;
using System.Security.Claims;

namespace PhoneBookApi.Tests.Helpers
{
    public static class JwtTestHelper
    {
        public static HttpContext CreateHttpContextFromToken(string token, JwtHandler jwtHandler)
        {
            var principal = jwtHandler.ValidateToken(token);

            var context = new DefaultHttpContext
            {
                User = principal ?? new ClaimsPrincipal()
            };

            return context;
        }

        public static PhoneBookController CreatePhoneBookControllerWithToken(string token, DataHandler handler, JwtHandler jwtHandler)
        {
            return new PhoneBookController(
                LoggerFactory.Create(x => x.AddConsole()).CreateLogger<PhoneBookController>(),
                handler)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = CreateHttpContextFromToken(token, jwtHandler)
                }
            };
        }
    }
}