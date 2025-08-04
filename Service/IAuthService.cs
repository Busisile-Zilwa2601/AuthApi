using AuthApi.DTOs;

namespace AuthApi.Service
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(AuthRequest register);
        Task<AuthResponse> LoginAsync(AuthRequest login);
        Task LogoutAsync(string email);
        Task<AuthResponse> RefreshTokenAsync(string email, string token);
    }
}
