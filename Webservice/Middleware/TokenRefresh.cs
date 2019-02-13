using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;
using Webservice.Database;
using Webservice.Database.Models;
using Webservice.Services;

namespace Webservice.Middleware
{
    public class TokenRefresh
    {
        private readonly RequestDelegate _next;

        public TokenRefresh(RequestDelegate next )
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext, DatabaseContext _databaseContext, IConfiguration _configuration)
        {
            var userIdClaim = httpContext.User?.Claims?.FirstOrDefault(x => x.Type == "CustomUserIdentifier")?.Value;


            if (!string.IsNullOrEmpty(userIdClaim))
            {
                var userId = int.Parse(userIdClaim);
                var existingToken = await _databaseContext.RefreshTokens.FirstOrDefaultAsync(x => x.UserId == userId && !x.User.Deactivated);

                if (existingToken != null)
                {
                    _databaseContext.RefreshTokens.RemoveRange(_databaseContext.RefreshTokens.Where(x => x.UserId == userId)); //clear used tokens

                    var newToken = new RefreshToken
                    {
                        UserId = userId,
                        ExpirationTime = DateTime.Now.AddHours(_configuration.GetSection("jwt").GetValue<int>("refreshExpirationInHours")),
                        Token = UsersService.GenerateRefreshToken()
                    };

                    _databaseContext.RefreshTokens.Add(newToken);
                    await _databaseContext.SaveChangesAsync();

                    httpContext.Response.Headers.Add("X-Integrity", newToken.Token);
                    httpContext.Response.Headers.Add("X-Life", ((int)(newToken.ExpirationTime.Subtract(new DateTime(1970, 1, 1)).TotalSeconds * 1000)).ToString()); // Unix style dates for the javascript new Date()
                }
            }

            await _next(httpContext);
        }
    }
}
