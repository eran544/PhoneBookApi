using Microsoft.AspNetCore.Mvc;
using PhoneBookApi.DTOs;
using PhoneBookApi.DTOs.Responses;
using PhoneBookApi.Handlers;
using PhoneBookApi.Models;
using System.Text.RegularExpressions;

namespace PhoneBookApi.Controllers
{
    [Route("auth")]
    public class AuthController(DataHandler dataHandler, JwtHandler jwtHandler) : Controller
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
            if (!DavaValidatorHandler.IsValidEmail(request.Email))
            {
                return BadRequest("Email format is invalid");
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
                token = token,
                expiresIn = 3600,
                username = user.Username,
                Role = user.Role,
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
            if (!_dataHandler.TryGetUser(request, out User? user))
            {
                return BadRequest("User not found");
            }
            return GenerateTokenResponse(user!);
        }
    }
}
