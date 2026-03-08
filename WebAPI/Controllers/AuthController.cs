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

            response.RefreshToken = token.RefreshToken.Token;

            dataAccess.InsertRefreshToken(token.RefreshToken, user.Email);
            dataAccess.DisableUserTokenByEmail(user.Email);


            return response;

        }
        [HttpPost("refresh")]
        public ActionResult<AuthResponse> Refresh()
        {
            AuthResponse response = new AuthResponse();
            var refreshToken = Request.Cookies["refreshtoken"];
            if (refreshToken == null)
            {
                return Unauthorized("Invalid or expired refresh token.");
            }
            var user = dataAccess.FindUserByEmail(refreshToken);
            if (user == null)
            {
                return Unauthorized("User not found.");
            }
            var currentUser = dataAccess.FindUserByToken(refreshToken);
            if (currentUser == null)
            {
                return Unauthorized("Invalid or expired refresh token.");
            }
            var token = tokenProvider.GenerateToken(currentUser);
            response.AccessToken = token.AccessToken;
            response.RefreshToken = token.RefreshToken.Token;

            dataAccess.InsertRefreshToken(token.RefreshToken, currentUser.Email);
            dataAccess.DisableUserTokenByEmail(currentUser.Email);

            return response;

        }
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            var refreshToken = Request.Cookies["refreshtoken"];
            if (refreshToken == null)
            {
                return BadRequest("Refresh token is required.");
            }
            var user = dataAccess.FindUserByEmail(refreshToken);
            if (user == null)
            {
                return Unauthorized("User not found.");
            }
            dataAccess.DisableUserTokenByEmail(user.Email);
            return Ok("Logged out successfully.");
        }
    }
}
