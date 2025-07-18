using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Products.Configuration;
using Products.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Products.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IConfiguration _configuration;
        private readonly JwtSettings _jwtSettings;
        private readonly AuthUser _authUser;

        public AuthController(
            ILogger<AuthController> logger, 
            IConfiguration configuration,
            IOptions<JwtSettings> jwtSettings,
            IOptions<AuthUser> authUser)
        {
            _logger = logger;
            _configuration = configuration;
            _jwtSettings = jwtSettings.Value;
            _authUser = authUser.Value;
        }

        /// <summary>
        /// Authenticates a user and returns a JWT token
        /// </summary>
        /// <param name="auth">User credentials</param>
        /// <returns>JWT token and expiration details</returns>
        /// <response code="200">Returns the JWT token</response>
        /// <response code="400">If the request is invalid</response>
        /// <response code="401">If authentication fails</response>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult Login([FromBody] Auth auth)
        {
            _logger.LogInformation("Login attempt for user: {Username}", auth?.UserName);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid login request: {Errors}", 
                    JsonSerializer.Serialize(ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)));
                return BadRequest(new { message = "Invalid request" });
            }

            if (auth.Password != _authUser.Password || auth.UserName != _authUser.UserName)
            {
                _logger.LogWarning("Authentication failed for user: {Username}", auth.UserName);
                return Unauthorized(new { message = "Invalid username or password" });
            }

            var token = GenerateJwtToken(auth.UserName);
            _logger.LogInformation("User {Username} authenticated successfully", auth.UserName);

            return Ok(new 
            { 
                token = token,
                expiresIn = _jwtSettings.ExpirationInMinutes * 60, // in seconds
                tokenType = "Bearer"
            });
        }

        private string GenerateJwtToken(string username)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, "User") 
            };

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
