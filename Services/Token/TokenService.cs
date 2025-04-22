using BOs.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Services.Token
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(Account account)
        {
            if (account == null)
                throw new ArgumentNullException(nameof(account), "Account information is missing.");

            if (account.Role == null || string.IsNullOrEmpty(account.Role.RoleName))
            {
                account.Role = new Role { RoleName = "User" };
            }

            var claims = new List<Claim>
        {
                new Claim(ClaimTypes.NameIdentifier, account.AccountID.ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, account.Username),
                new Claim(JwtRegisteredClaimNames.Email, account.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, account.Username),
                new Claim(ClaimTypes.Role, account.Role.RoleName)

            /*new Claim(JwtRegisteredClaimNames.Sub, account.Username),
            new Claim(JwtRegisteredClaimNames.Email, account.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, account.Username),
            new Claim(ClaimTypes.Role, account.Role.RoleName),
            new Claim("RoleID", account.Role.RoleID.ToString()),
            new Claim("AccountID", account.AccountID.ToString())*/
        };

            var keyString = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(keyString))
                throw new ArgumentNullException("Jwt:Key is missing in configuration.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"], // Đảm bảo đặt Audience
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1), // Dùng UTC để tránh lỗi múi giờ
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
