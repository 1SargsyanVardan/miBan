using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApplication1.Models;
using WebApplication1.HelperClasses;
using WebApplication1.Models.MyModels.Request;
using System.Net;
using WebApplication1.Models.MyModels.Response;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly IConfiguration _config;
        private readonly AppDbContext _context;

        public AuthController(AppDbContext dbContext, IConfiguration config,AppDbContext appDbContext)
        {
            _dbContext = dbContext;
            _config = config;
            _context = appDbContext;
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        public IActionResult Login([FromBody] LoginDtoRequest loginDto)
        {
            var user = AuthenticateUser(loginDto);
            if (user == null)
                return Unauthorized("Invalid credentials");

            var token = GenerateToken(user);
            return Ok(new { token });
        }

        [HttpPost("VerifyEmail")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult VerifyEmail([FromBody] EmailVerificationRequest model)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);
            if (user == null)
                return BadRequest("Օգտվողը գտնված չէ։");

            if (user.VerificationCode != model.Code)
                return BadRequest("Սխալ հաստատման կոդ։");

            user.IsEmailConfirmed = true;
            user.VerificationCode = null;
            _context.SaveChanges();

            return Ok("Էլ․ հասցեն հաստատվեց հաջողությամբ։");
        }

        private User AuthenticateUser(LoginDtoRequest loginDto)
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
