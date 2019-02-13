using System.Threading.Tasks;
using Webservice.Database.Models;
using Webservice.ViewModels;

namespace Webservice.Interfaces
{
    public interface IUserService
    {
        Task<User> AddUser(User data, string password);

        Task<UserLogin> SignInUser(string emailAddress, string password);

        Task<UserLogin> RefreshLogin(string oldJwtToken, string refreshToken);
    }
}
