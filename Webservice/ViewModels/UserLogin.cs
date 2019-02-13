using System;
using Webservice.Database.Models;

namespace Webservice.ViewModels
{
    public class UserLogin : User
    {
        public string JwtToken { get; set; }

        public string RefreshToken { get; set; }

        public DateTime ExpirationDate { get; set; }
    }
}
