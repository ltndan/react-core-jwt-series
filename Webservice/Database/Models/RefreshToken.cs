using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Webservice.Database.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }

        public string Token { get; set; }

        public DateTime ExpirationTime { get; set; }
    }
}
