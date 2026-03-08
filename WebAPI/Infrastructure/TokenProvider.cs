using WebAPI.Models;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace WebAPI.Infrastructure
{
    public class TokenProvider
    {
        private readonly IConfiguration configuration;

        public TokenProvider(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public Token GenerateToken(UserAccount userAccount)
        {
            var accrssToken = GenerateAccessToken(userAccount);
            var refreshToken = GenerateRefreshToken();
            return new Token { AccessToken = accrssToken , RefreshToken = refreshToken };

        }

        private RefreshToken? GenerateRefreshToken()
        {
            var refreshToken = new RefreshToken
            {
                Token = Guid.NewGuid().ToString(),
                Expires = DateTime.UtcNow.AddDays(7),
                Enabled = true
            };
            return refreshToken;
        }
        private string GenerateAccessToken(UserAccount userAccount)
        {
            string secretKey = configuration["JWT:Key"];
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new JwtSecurityToken(
                issuer: configuration["JWT:Issuer"],
                audience: configuration["JWT:Audience"],
                claims: new[]
                {
                    new Claim(ClaimTypes.Name, userAccount.Email),
                    new Claim(ClaimTypes.Role, userAccount.Role)
                },
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials
            );
            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);

        }

        public class Token
        {
            public string AccessToken { get; set; }
             public RefreshToken RefreshToken { get; set; }
        }
    }
}
