using AuthApi.Models;

namespace AuthApi.DAL
{
    public interface IAuthContext
    {
        Task AuthRegister(UserDb user);
        Task<UserDb?> GetUserByEmailAsync(string email);
        Task<UserDb?> GetUserByIdAsync(string userId);
    }
}
