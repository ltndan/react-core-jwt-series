using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Webservice.Database.Models;
using Webservice.Interfaces;
using Webservice.ViewModels;

namespace Webservice.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        public async Task<User> AddUser(UserCreate data)
        {
            var newUser = await _userService.AddUser(new User { EmailAddress = data.EmailAddress }, data.Password);

            return newUser;
        }

        [HttpPost]
        public async Task<ActionResult<UserLogin>> SignInUser(UserSignin data)
        {

            var login = await _userService.SignInUser(data.EmailAddress, data.Password);

            if (login != null)
            {
                return login;
            }

            return Unauthorized();
        }

        [HttpPost]
        public async Task<ActionResult<UserLogin>> RefreshLogin(RefreshTokenRequest data)
        {
            var login = await _userService.RefreshLogin(data.OldJwtToken, data.RefreshToken);

            if (login != null)
            {
                return login;
            }

            return Unauthorized();
        }
    }
}
