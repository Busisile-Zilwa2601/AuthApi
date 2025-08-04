using AuthApi.Models;
using MongoDB.Driver;

namespace AuthApi.DAL
{
    public class AuthContext: IAuthContext
    {
        private readonly IMongoCollection<UserDb> userCollection;
        

        public AuthContext(IConfiguration config)
        {
            var client = new MongoClient(config["MongoDb:ConnectionString"]);
            var database = client.GetDatabase(config["MongoDb:DatabaseName"]);
            userCollection = database.GetCollection<UserDb>(config["MongoDb:UserCollection"]);
        }
        public async Task AuthRegister(UserDb user)
        {
            await userCollection.InsertOneAsync(user);
        }

        public async Task<UserDb?> GetUserByEmailAsync(string email)
        {
            return await userCollection
                .Find(u => u.Email == email)
                .FirstOrDefaultAsync();
        }

        public async Task<UserDb?> GetUserByIdAsync(string userId)
        {
            return await userCollection
                .Find(u => u.UserId == userId)
                .FirstOrDefaultAsync();
        }
    }
}
