using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Api.Services
{
    // Interface for JWT operations
    public interface IJwtService
    {
        string GenerateToken(string userId, string email,string role = "User"); // Generates a JWT token for a user
        ClaimsPrincipal? ValidateToken(string token); // Validates a JWT token and returns user claims if valid
    }

    // JWT service handles creating and validating JWT tokens
    public class JwtService : IJwtService 
    {
        private readonly IConfiguration _configuration;

        // Inject configuration to access JWT settings from appsettings.json
        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Creates a JWT token for a user after successful login
        public string GenerateToken(string userId, string email, string role = "User")
        {
            // Get the secret key from configuration and convert to bytes
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            
            // Create signing credentials using HMAC SHA256 algorithm
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Claims are pieces of information about the user stored in the token
            var claims = new[]
            {
                // User ID - used to identify which user this token belongs to
                new Claim(ClaimTypes.NameIdentifier, userId),
                
                // Email - convenient to have in token for display purposes
                new Claim(ClaimTypes.Email, email),

                new Claim(ClaimTypes.Role, role),
                
                // JWT ID - unique identifier for this specific token
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                
                // Issued At - when this token was created (Unix timestamp)
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            // Create the JWT token with issuer, audience, claims, expiration time, and signing credentials
            // Issuer is who created the token, audience is who this token is for
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],           // Who issued this token
                audience: _configuration["Jwt:Audience"],       // Who this token is for
                claims: claims,                                 // User information
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpireMinutes"])), // When it expires
                signingCredentials: credentials                 // How to verify it's authentic
            );

            // Convert token to string format that can be sent to client
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Validates a JWT token and extracts user information
        // Used by middleware to check if requests are authenticated
        public ClaimsPrincipal? ValidateToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);
                
                // Set up validation parameters - what to check when validating
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,           // Check if issuer matches our app
                    ValidateAudience = true,         // Check if audience matches our app
                    ValidateLifetime = true,         // Check if token hasn't expired
                    ValidateIssuerSigningKey = true, // Check if signature is valid
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidAudience = _configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero        // No tolerance for clock differences
                };

                // Validate the token and extract user claims
                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                return principal;
            }
            catch
            {
                // If anything goes wrong, token is invalid
                return null;
            }
        }
    }
}