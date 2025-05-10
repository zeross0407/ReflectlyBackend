using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Reflectly.Entity;
using Reflectly.Models;
using SharpCompress.Common;
using System.Reflection;

namespace Reflectly.Service
{
    public class Account_Service
    {
        private readonly IMongoCollection<Account> _AccountCollection;
        private readonly IMongoCollection<AccessToken> _RefreshTokenCollection;

        public Account_Service(IOptions<DatabaseSettings> DatabaseSettings)
        {
            var mongoClient = new MongoClient(DatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(DatabaseSettings.Value.DatabaseName);

            _AccountCollection = mongoDatabase.GetCollection<Account>(GetCollectionName<Account>());
        }
        private string GetCollectionName<T>()
        {
            var attribute = typeof(T).GetCustomAttribute<CollectionNameAttribute>();
            return attribute?.Name ?? typeof(T).Name;
        }
        public async Task<List<Account>> GetAsync() =>
            await _AccountCollection.Find(_ => true).ToListAsync();

        public async Task<List<Account>> Get_by_Email_Async(string email) =>
            await _AccountCollection.Find(x => x.Email == email).ToListAsync();

        public async Task<Account?> GetAsync(string id) =>
            await _AccountCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task<String> CreateAsync(Account newAccount)
        {
            await _AccountCollection.InsertOneAsync(newAccount);
            var propertyInfo = newAccount.GetType().GetProperty("Id");
            if (propertyInfo != null)
            {
                return propertyInfo.GetValue(newAccount)?.ToString();
            }
            return null; // Trường hợp không tìm thấy Id
        }

        public async Task UpdateAsync(string id, Account updatedAccount) =>
            await _AccountCollection.ReplaceOneAsync(x => x.Id == id, updatedAccount);

        public async Task RemoveAsync(string id) =>
            await _AccountCollection.DeleteOneAsync(x => x.Id == id);
    }
}
