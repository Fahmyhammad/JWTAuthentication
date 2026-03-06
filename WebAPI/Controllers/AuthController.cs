using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Infrastructure;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly DataAccess dataAccess;
        public AuthController(DataAccess dataAccess)
        {
            this.dataAccess = dataAccess;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] DTO.RegisterRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password) || string.IsNullOrEmpty(request.Role))
            {
                return BadRequest("Email, Password, and Role are required.");
            }
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);
            var result = dataAccess.RegisterUser(request.Email, hashedPassword, request.Role);
            if (!result)
            {
                return Conflict("Email already exists.");
            }
            return Ok("User registered successfully.");
        }
    }
}
