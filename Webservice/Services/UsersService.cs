using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Webservice.Database;
using Webservice.Database.Models;
using Webservice.Interfaces;
using Webservice.ViewModels;

namespace Webservice.Services
{
    public class UsersService : IUserService
    {
        private readonly ILogger<UsersService> _logger;
        private readonly DatabaseContext _databaseContext;
        private readonly IPasswordService _passwordService;
        private readonly IConfiguration _configuration;

        public UsersService(ILogger<UsersService> logger, DatabaseContext databaseContext,
            IPasswordService passwordService, IConfiguration configuration)
        {
            _logger = logger;
            _databaseContext = databaseContext;
            _passwordService = passwordService;
            _configuration = configuration;
        }

        public async Task<User> AddUser(User data, string password)
        {
            data.Credentials = _passwordService.HashUserPassword(password); // or maybe a combination of the email and the password
            var newUser = _databaseContext.Users.Add(data);

            await _databaseContext.SaveChangesAsync();
            _logger.LogInformation($"Added new user to the database {data.EmailAddress}"); // don't log usernames of email addresses GDPR ehm ehm
            return newUser.Entity;
        }

        public async Task<UserLogin> SignInUser(string emailAddress, string password)
        {
            _logger.LogInformation("Signing user in");
            var user = await GetUserByCredentials(emailAddress, password);

            if (user == null) { return null; }

            var expirationTime = DateTime.Now.AddMinutes(_configuration.GetSection("jwt").GetValue<int>("expirationInMinutes"));

            var jwtToken = GenerateJwtToken("some user role", user, expirationTime);
            var refreshToken = GenerateRefreshToken();

            _databaseContext.RefreshTokens.RemoveRange(_databaseContext.RefreshTokens.Where(x => x.UserId == user.Id)); // always clear the used tokens
            _databaseContext.RefreshTokens.Add(new RefreshToken
            {
                ExpirationTime = DateTime.Now.AddHours(_configuration.GetSection("jwt").GetValue<int>("refreshExpirationInHours")),
                Token = refreshToken,
                UserId = user.Id,
            });

            await _databaseContext.SaveChangesAsync();

            return new UserLogin
            {
                Id = user.Id,
                EmailAddress = user.EmailAddress,
                ExpirationDate = expirationTime,
                JwtToken = jwtToken,
                RefreshToken = refreshToken,
            };
        }

        public async Task<UserLogin> RefreshLogin(string oldJwtToken, string refreshToken)
        {
            var principal = GetPrincipalFromExpiredToken(oldJwtToken);
            var validRefreshToken = await _databaseContext.RefreshTokens.Include(x => x.User).FirstOrDefaultAsync(x => x.Token == refreshToken && !x.User.Deactivated && x.ExpirationTime > DateTime.Now);

            if(validRefreshToken == null)
            {
                _logger.LogWarning("Invalid refresh token");
                _logger.LogDebug($"Invalid refresh token for User {principal.Identity.Name}"); // only write user sensitive data in debug

                _databaseContext.RefreshTokens.RemoveRange(_databaseContext.RefreshTokens.Where(x => x.User.EmailAddress == principal.Identity.Name)); // clear the users tokens, to prevent brute force
                await _databaseContext.SaveChangesAsync();

                throw new SecurityTokenException("Invalid refresh token");
            }

            var expirationTime = DateTime.Now.AddMinutes(_configuration.GetSection("jwt").GetValue<int>("expirationInMinutes"));

            var jwtToken = GenerateJwtToken("some user role", validRefreshToken.User, expirationTime);
            var newRefreshToken = GenerateRefreshToken();

            _databaseContext.RefreshTokens.RemoveRange(_databaseContext.RefreshTokens.Where( x=> x.UserId == validRefreshToken.UserId)); // always clear the used tokens, you can also match on the token itself if you have multiple tokens per user

            _databaseContext.RefreshTokens.Add(new RefreshToken
            {
                ExpirationTime = DateTime.Now.AddHours(_configuration.GetSection("jwt").GetValue<int>("refreshExpirationInHours")),
                Token = newRefreshToken,
                UserId = validRefreshToken.UserId
            });

            await _databaseContext.SaveChangesAsync();

            return new UserLogin
            {
                Id = validRefreshToken.UserId,
                EmailAddress = validRefreshToken.User.EmailAddress,
                ExpirationDate = expirationTime,
                JwtToken = jwtToken,
                RefreshToken = newRefreshToken,
            };
        }

        private async Task<User> GetUserByCredentials(string emailAddress, string password)
        {
            var user = await _databaseContext.Users.FirstOrDefaultAsync(x => x.EmailAddress == emailAddress);
            if (user == null) return null;

            var passwordResult = _passwordService.VerifyPassword(user.Credentials, password);
            if (passwordResult != PasswordVerificationResult.Success) return null;

            _logger.LogInformation("User signed in");
            return user;
        }

        private string GenerateJwtToken(string userRole, User user, DateTime expirationTime)
        {
            var claims = new[]
                {
                    new Claim(ClaimTypes.Role, userRole),
                    new Claim(ClaimTypes.Name, user.EmailAddress),
                    new Claim("CustomUserIdentifier", user.Id.ToString())
                };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("jwt").GetValue<string>("key")));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var token = new JwtSecurityToken(
                _configuration.GetSection("jwt").GetValue<string>("issuer"), // get value with a specified types makes me happy
                _configuration.GetSection("jwt").GetValue<string>("audience"),
                claims,
                expires: expirationTime,
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _configuration.GetSection("jwt").GetValue<string>("issuer"),
                ValidAudience = _configuration.GetSection("jwt").GetValue<string>("audience"),
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("jwt").GetValue<string>("key"))),
                ValidateLifetime = false //here we are saying that we don't care about the token's expiration date
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha512, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid JWT token");

            return principal;
        }
    }
}
