using AuthApi.DAL;
using AuthApi.DTOs;
using AuthApi.Models;
using AuthApi.Helpers;
using System.Text;
using AutoMapper;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace AuthApi.Service
{
    public class AuthService : IAuthService
    {
        private readonly IAuthContext _authContext;
        private readonly IRefreshTokenContext _refreshTokenContext;
        private readonly IMapper _mapper;
        private readonly IJwtGenerator _jwtGenerator;

        public AuthService(IAuthContext authContext, IRefreshTokenContext refreshTokenContext,IMapper mapper, IJwtGenerator jwtGenerator)
        {
            _authContext = authContext ?? throw new ArgumentNullException(nameof(authContext));
            _refreshTokenContext = refreshTokenContext ?? throw new ArgumentNullException(nameof(refreshTokenContext));
            _mapper = mapper;
            _jwtGenerator = jwtGenerator;
        }

        public async Task<AuthResponse> RegisterAsync(AuthRequest request)
        {
            var existingUser = _authContext.GetUserByEmailAsync(request.Email);
            if (existingUser.Result != null)
            {
                throw new InvalidOperationException("User with this email already exists.");
            }

            if (!IsValidPassword(request.Password))
            {
                throw new InvalidOperationException("Incorret password format and should be more than 9 Charectors ");
            }

            var salt = GenerateSalt();
            var hashedPassword = HashPassword(request.Password, salt);

            var user = new User
            {
                UserId = Guid.NewGuid(),
                Email = request.Email,
                Password = hashedPassword,
                Salt = salt
            };

            var mappedUser = _mapper.Map<UserDb>(user);


            await _authContext.AuthRegister(mappedUser);
            var id = user.UserId.ToString();

            var accessToken =  _jwtGenerator.GenerateToken(id, user.Email);

            var refreshToken = new RefreshToken
            {
                UserId = mappedUser.UserId,
                Token = _jwtGenerator.GenerateRefreshToken(),
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            };
            await _refreshTokenContext.SaveTokenAsync(refreshToken);
            
            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
            };
        }

        private string HashPassword(string password, string salt)
        {
            var sha = SHA256.Create();
            var salted = password + salt;
            var bytes = Encoding.UTF8.GetBytes(salted);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        private string GenerateSalt()
        {
            var bytes = new byte[16];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);

        }

        private bool VerifyPassword(string password, string hashedPassword, string salt)
        {
            return HashPassword(password, salt) == hashedPassword;
        }

        public async Task<AuthResponse> LoginAsync(AuthRequest login)
        {
            var user = await _authContext.GetUserByEmailAsync(login.Email);
            if (user == null || !VerifyPassword(login.Password, user.Password, user.Salt))
            { 
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            var accessToken = _jwtGenerator.GenerateToken(user.UserId.ToString(), user.Email);
            var refreshToken = _jwtGenerator.GenerateRefreshToken();

            var refreshTokenModel = new RefreshToken
            {
                UserId = user.UserId,
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            };
            await _refreshTokenContext.SaveTokenAsync(refreshTokenModel);
            
            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        public async Task<AuthResponse> RefreshTokenAsync(string email, string token)
        {
            var refreshToken = await _refreshTokenContext.GetRefreshTokenByUserEmail(email);
            if (refreshToken == null || refreshToken.isRevoked || refreshToken.ExpiresAt < DateTime.UtcNow)
            {
                throw new UnauthorizedAccessException("Invalid or expired refresh token.");
            }
            var user = await _authContext.GetUserByIdAsync(refreshToken.UserId);
            if (user == null)
            {
                throw new UnauthorizedAccessException("User not found.");
            }
            var newAccessToken = _jwtGenerator.GenerateToken(user.UserId.ToString(), user.Email);
            var newRefreshToken = _jwtGenerator.GenerateRefreshToken();
            refreshToken.Token = newRefreshToken;
            refreshToken.ExpiresAt = DateTime.UtcNow.AddDays(7);
            refreshToken.CreatedAt = DateTime.UtcNow;
            refreshToken.isRevoked = false;
            await _refreshTokenContext.UpdateAsync(refreshToken);
            
            return new AuthResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            };
        }

        public async Task LogoutAsync(string email)
        { 
            var user = await _authContext.GetUserByEmailAsync(email);
            if (user == null) return;

            var refreshToken = await _refreshTokenContext.GetTokenByUserIdAsync(user.UserId.ToString());
            refreshToken.Token = null;
            refreshToken.ExpiresAt = DateTime.MinValue;

            await _refreshTokenContext.UpdateAsync(refreshToken);
        }

        private bool IsValidPassword(string password)
        {
            var pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{6,}$";
            return Regex.IsMatch(password, pattern);
        }
    }
}
