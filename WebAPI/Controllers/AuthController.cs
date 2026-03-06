using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAPI.DTO;
using WebAPI.Infrastructure;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly DataAccess dataAccess;
        private readonly TokenProvider tokenProvider;
        public AuthController(DataAccess dataAccess, TokenProvider tokenProvider)
        {
            this.dataAccess = dataAccess;
            this.tokenProvider = tokenProvider;
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
        [HttpPost("login")]
        public ActionResult<AuthResponse> Login([FromBody] DTO.AuthRequest request)
        {
            AuthResponse response = new AuthResponse();
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest("Email and Password are required.");
            }
            var user = dataAccess.FindUserByEmail(request.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            {
                return Unauthorized("Invalid email or password.");
            }
            // Here you would typically generate a JWT token and return it to the client
            var token = tokenProvider.GenerateToken(user);
            response.AccessToken = token.AccessToken;
            return response;

        }
    }
}
