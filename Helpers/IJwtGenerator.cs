namespace AuthApi.Helpers
{
    public interface IJwtGenerator
    {
        string GenerateToken(string userId, string email);
        string GenerateRefreshToken();
        bool ValidateToken(string token);
        string GetUserIdFromToken(string token);
        string GetEmailFromToken(string token);
      
        string GetRoleFromToken(string token);
    }
}
