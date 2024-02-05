using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using PaySkyDemo.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System;
using Microsoft.EntityFrameworkCore;

namespace PaySkyDemo.Services
{
    public class AuthService
    {
        private readonly PaySkyDemoContext _paySkyDemoContext;
        private IConfiguration _config;
        private const int ExpirationMinutes = 30;
        private readonly ILogger<AuthService> _logger;
        public AuthService(PaySkyDemoContext paySkyDemoContext, IConfiguration config, ILogger<AuthService> logger) 
        { 
            _paySkyDemoContext = paySkyDemoContext;
            _config = config;
            _logger = logger;

        }

        public bool CheckUserPassword( string storedPassword,string password) 
        {     
            var checkPassword = BCrypt.Net.BCrypt.Verify(password, storedPassword);
            if(checkPassword) return true;
           
            return false;
        }

        public async Task<bool> StoreNewUser(User newUser)
        {
            try
            {
                 newUser.Password = BCrypt.Net.BCrypt.HashPassword(newUser.Password);
                _paySkyDemoContext.Users.Add(newUser);
                await _paySkyDemoContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while storing new user");
                return false;
            }
        }

        public async Task<User> GetUserByEmail(string email)
        {
            return await _paySkyDemoContext.Users.FirstOrDefaultAsync(u => u.Email == email);

        }



        public string CreateToken(User user)
        {
            try
            {
                var expiration = DateTime.UtcNow.AddMinutes(ExpirationMinutes);
                var token = CreateJwtToken(
                    CreateClaims(user),
                    CreateSigningCredentials(),
                    expiration
                );
                var tokenHandler = new JwtSecurityTokenHandler();

                _logger.LogInformation("JWT Token created");

                return tokenHandler.WriteToken(token);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error creating token");
                return "";
            }
        }

        private JwtSecurityToken CreateJwtToken(List<Claim> claims, SigningCredentials credentials,
            DateTime expiration) =>
            new(
                new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("JwtTokenSettings")["ValidIssuer"],
                new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("JwtTokenSettings")["ValidAudience"],
                claims,
                expires: expiration,
                signingCredentials: credentials
            );

        private List<Claim> CreateClaims(User user)
        {
            var jwtSub = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("JwtTokenSettings")["JwtRegisteredClaimNamesSub"];

            try
            {
                var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, jwtSub),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
                    new Claim("UserId", user.ID.ToString()),
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.UserType.ToString())
                };

                return claims;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error creating claims");
                throw;
            }
        }

        private SigningCredentials CreateSigningCredentials()
        {
            var symmetricSecurityKey = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("JwtTokenSettings")["SymmetricSecurityKey"];

            return new SigningCredentials(
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(symmetricSecurityKey)
                ),
                SecurityAlgorithms.HmacSha256
            );
        }
    }
}

