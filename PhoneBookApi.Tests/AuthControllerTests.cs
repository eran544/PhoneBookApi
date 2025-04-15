using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PhoneBookApi.Controllers;
using PhoneBookApi.DTOs.Requests;
using PhoneBookApi.DTOs.Responses;
using PhoneBookApi.Handlers;
using PhoneBookApi.Tests.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoneBookApi.Tests
{
    [Collection("MongoTest")]
    public class AuthControllerTests : MongoTestBase
    {
        private readonly JwtHandler _jwtHandler;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _jwtHandler = new JwtHandler(Config);
            var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<AuthController>();
            _controller = new AuthController(Handler, _jwtHandler)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
        }

        [Fact]
        public async Task Register_ShouldReturnToken()
        {
            var request = new RegisterRequest
            {
                Username = "newuser",
                Email = "newuser@example.com",
                Password = "Secret123!",
                FirstName = "New",
                LastName = "User"
            };

            var result = await _controller.Register(request);

            result.Should().NotBeNull();
            var OkResult = result.Result as OkObjectResult;
            OkResult.Should().NotBeNull();
            var token = (OkResult!.Value as AuthResponse)?.Token;
            token.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task Login_ShouldReturnToken_WhenValid()
        {
            var request = new RegisterRequest
            {
                Username = "logintest",
                Email = "login@example.com",
                Password = "Test123!",
                FirstName = "Log",
                LastName = "In"
            };

            await _controller.Register(request);

            var login = new LoginRequest
            {
                Username = "logintest",
                Password = "Test123!"
            };

            var result = await _controller.Login(login);

            result.Should().NotBeNull();
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            var response = okResult!.Value as AuthResponse;
            response.Should().NotBeNull();
            var token = response.Token;
            token.Should().NotBeNullOrWhiteSpace();
        }
    }
}
