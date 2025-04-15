using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using PhoneBookApi.DTOs.Responses;
using PhoneBookApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PhoneBookApi.Handlers
{
    public class JwtHandler
    {
        private readonly string secret;
        private readonly string issuer;
        private readonly string audience;
        private readonly byte[] key;
        private static TokenValidationParameters? validationParameters;

        public JwtHandler(IConfiguration configuration)
        {
            secret = configuration["Jwt:SecretKey"]!;
            issuer = configuration["Jwt:Issuer"]!;
            audience = configuration["Jwt:Audience"]!;
            key = Encoding.ASCII.GetBytes(secret);
        }

        public string GenerateToken(User user, int durationMinutes = 60)
        {
            // Create JWT token that expires after 60 minutes
            var tokenHandler = new JwtSecurityTokenHandler();
            var claims = new List<Claim> { new(ClaimTypes.Name, user.Id.ToString()) };
            if (user.Role != null && user.Role != Role.User)
            {
                claims.Add(new(ClaimTypes.Role, user.Role!.ToString()!));
            }
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = issuer,
                Audience = audience,
                Expires = DateTime.UtcNow.AddMinutes(durationMinutes),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            validationParameters ??= new TokenValidationParameters
            {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ClockSkew = TimeSpan.Zero
            };

            try
            {
                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                if (validatedToken is JwtSecurityToken jwt &&
            jwt.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return principal;
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public static (ObjectId?, Role) GetUserIdAndRole(ClaimsPrincipal principal)
        {
            var userIdString = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            _ = ObjectId.TryParse(userIdString, out var userId);
            var roleString = principal.FindFirst(ClaimTypes.Role)?.Value;
            Role role = Role.User;
            if (!string.IsNullOrEmpty(roleString) &&
                Enum.TryParse<Role>(roleString, ignoreCase: true, out var parsedRole))
            {
                role = parsedRole;
            }
            return (userId, role);
        }
    }
}
