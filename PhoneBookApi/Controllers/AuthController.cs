using Microsoft.AspNetCore.Mvc;
using PhoneBookApi.DTOs.Requests;
using PhoneBookApi.DTOs.Responses;
using PhoneBookApi.Handlers;
using PhoneBookApi.Models;
using System.Text.RegularExpressions;

namespace PhoneBookApi.Controllers
{
    [Route("auth")]
    [ApiController]
    public class AuthController(DataHandler dataHandler, JwtHandler jwtHandler) : ControllerBase
    {
        private readonly DataHandler _dataHandler = dataHandler;
        private readonly JwtHandler _jwtHandler = jwtHandler;

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest? request)
        {
            if (request == null)
            {
                return BadRequest();
            }
            var user = await _dataHandler.RegisterUser(request);
            if (user == null)
            {
                return BadRequest("User already exists");
            }
            return GenerateTokenResponse(user);

        }

        private ActionResult<AuthResponse> GenerateTokenResponse(User user)
        {
            var token = _jwtHandler.GenerateToken(user);
            var response = new AuthResponse()
            {
                Token = token,
                ExpiresIn = 3600,
                Username = user.Username,
            };
            return Ok(response);
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest? request)
        {
            if (request == null)
            {
                return BadRequest();
            }
            var user = await _dataHandler.GetUser(request);
            if (user == null)
            {
                return BadRequest("User not found");
            }
            return GenerateTokenResponse(user!);
        }
    }
}
