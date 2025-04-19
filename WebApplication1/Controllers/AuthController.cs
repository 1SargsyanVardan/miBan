using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApplication1.Models;
using WebApplication1.HelperClasses;
using WebApplication1.Models.MyModels.Request;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly IConfiguration _config;

        public AuthController(AppDbContext dbContext, IConfiguration config)
        {
            _dbContext = dbContext;
            _config = config;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDtoModel loginDto)
        {
            var user = AuthenticateUser(loginDto);
            if (user == null)
                return Unauthorized("Invalid credentials");

            var token = GenerateToken(user);
            return Ok(new { token });
        }

        private User AuthenticateUser(LoginDtoModel loginDto)
        {
            var user = _dbContext.Users.FirstOrDefault(u => u.Email == loginDto.Email);
            if (user == null) return null;

            return PasswordHelper.VerifyPassword(user.PasswordHash, loginDto.Password) ? user : null;
        }

        private string GenerateToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
