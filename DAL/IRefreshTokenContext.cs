using AuthApi.Models;

namespace AuthApi.DAL
{
    public interface IRefreshTokenContext
    {
        Task SaveTokenAsync(RefreshToken refreshToken);
        Task<RefreshToken> GetTokenByUserIdAsync(string userId);

        Task<RefreshToken> GetRefreshTokenByUserEmail(string email);
        Task RevokeTokenAsync(string token);

        Task UpdateAsync(RefreshToken refreshToken);
    }
}
