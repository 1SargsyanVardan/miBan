using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.HelperClasses;
using WebApplication1.Models;
using WebApplication1.Models.MyModels.Request;

namespace WebApplication1.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [ApiController]
    [Route("api/[controller]")]
    public class AdminSetupController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        public AdminSetupController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpPost("create-admin")]
        public IActionResult CreateAdmin([FromBody] UserRegisterRequest userModel)
        {
            var user = _mapper.Map<User>(userModel);
            user.PasswordHash = PasswordHelper.HashPassword(userModel.PasswordHash); 
            if (userModel.GroupId == 0)
            {
                user.GroupId = null;
            }
            _context.Users.Add(user);
            _context.SaveChanges();

            return Ok("Օգտվողը հաջողությամբ գրանցվեց:");
        }
    }

}
