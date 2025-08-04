using AuthApi.Models;
using MongoDB.Driver;

namespace AuthApi.DAL
{
    public class RefreshTokenContext: IRefreshTokenContext
    {
        private readonly IMongoCollection<RefreshToken> refreshTokenCollection;

        public RefreshTokenContext(IConfiguration config)
        {
            var client = new MongoClient(config["MongoDb:ConnectionString"]);
            var database = client.GetDatabase(config["MongoDb:DatabaseName"]);
            refreshTokenCollection = database.GetCollection<RefreshToken>(config["MongoDb:RefreshTokenCollection"]);
        }
        public async Task SaveTokenAsync(RefreshToken refreshToken)
        {
            await refreshTokenCollection.InsertOneAsync(refreshToken);
        }
        public async Task<RefreshToken> GetTokenByUserIdAsync(string userId)
        {
            return await refreshTokenCollection
                .Find(rt => rt.UserId == userId && !rt.isRevoked)
                .FirstOrDefaultAsync() ?? throw new Exception("Token not found");
        }
        public async Task<RefreshToken> GetRefreshTokenByUserEmail(string email)
        {
            return await refreshTokenCollection
                .Find(rt => rt.UserId == email && !rt.isRevoked)
                .FirstOrDefaultAsync() ?? throw new Exception("Token not found");
        }
        public async Task RevokeTokenAsync(string token)
        {
           var update = Builders<RefreshToken>.Update.Set(rt => rt.isRevoked, true);
           await refreshTokenCollection
                .FindOneAndUpdateAsync(rt => rt.Token == token && !rt.isRevoked, update);
        }

        public async Task UpdateAsync(RefreshToken refreshToken)
        {
            await refreshTokenCollection
                .ReplaceOneAsync(rt => rt.UserId == refreshToken.UserId, refreshToken);
        }
    }
}
