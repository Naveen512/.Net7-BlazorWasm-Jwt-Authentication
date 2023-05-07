using Jwt.Auth.API.Dtos;
using Jwt.Auth.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Jwt.Auth.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> UserRegistration(UserRegistrationDto userRegistration)
        {
            var result = await _userService.RegisterNewUserAsync(userRegistration);
            if(result.IsUserRegistered)
            {
                return Ok(result.Message);
            }

            ModelState.AddModelError("Email", result.Message);
            return BadRequest(ModelState);
        }
        [HttpGet("unique-user-email")]
        public IActionResult CheckUniqueUserEmail(string email)
        {
            var result = _userService.CheckUserUniqueEmail(email);
            return Ok(result);
        }
        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync(LoginDto payload)
        {
            var result = await _userService.LoginAsync(payload);
            if(result.IsLoginSuccess)
            {
                return Ok(result.TokeResponse);
            }
            ModelState.AddModelError("LoginError", "Invalid Credentials");
            return BadRequest(ModelState);
        }
    }
}
