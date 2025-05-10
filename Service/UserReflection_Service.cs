using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Reflectly.Entity;
using Reflectly.Models;
using System.Reflection;
using System.Threading.Tasks;

namespace Reflectly.Service
{
    public class UserReflection_Service
    {
        private readonly IMongoCollection<Reflection> _reflectionCollection;
        private readonly IMongoCollection<User_Reflection> _userReflectionCollection;

        public UserReflection_Service(IOptions<DatabaseSettings> databaseSettings)
        {
            var mongoClient = new MongoClient(databaseSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(databaseSettings.Value.DatabaseName);

            _reflectionCollection = mongoDatabase.GetCollection<Reflection>(GetCollectionName<Reflection>());
            _userReflectionCollection = mongoDatabase.GetCollection<User_Reflection>(GetCollectionName<User_Reflection>());
        }

        private string GetCollectionName<T>()
        {
            var attribute = typeof(T).GetCustomAttribute<CollectionNameAttribute>();
            return attribute?.Name ?? typeof(T).Name;
        }

        // Thêm User_Reflection mới
        public async Task CreateAsync(User_Reflection userReflection)
        {
            await _userReflectionCollection.InsertOneAsync(userReflection);
        }

        // Lấy tất cả User_Reflection
        public async Task<List<User_Reflection>> GetAllAsync()
        {
            return await _userReflectionCollection.Find(_ => true).ToListAsync();
        }

        // Lấy User_Reflection theo id
        public async Task<User_Reflection?> GetByIdAsync(string id)
        {
            return await _userReflectionCollection.Find(ur => ur.UUID.ToString() == id).FirstOrDefaultAsync();
        }

        // Cập nhật User_Reflection
        public async Task<ReplaceOneResult> UpdateAsync(string id, User_Reflection userReflection)
        {
            return await _userReflectionCollection.ReplaceOneAsync(ur => ur.UUID.ToString() == id, userReflection);
        }

        // Xóa User_Reflection
        public async Task<DeleteResult> DeleteAsync(string id)
        {
            return await _userReflectionCollection.DeleteOneAsync(ur => ur.UUID.ToString() == id);
        }

        // Tìm kiếm User_Reflection theo reflection_id
        public async Task<List<User_Reflection>> GetByReflectionIdAsync(string reflectionId)
        {
            return await _userReflectionCollection.Find(ur => ur.reflection_id == reflectionId).ToListAsync();
        }



        public async Task<List<User_Reflection>> GetBy_UserID_Async(string id)
        {
            // Trả về tài liệu tìm thấy dựa trên UUID
            return await _userReflectionCollection.Find(Builders<User_Reflection>.Filter.Eq("userId", new ObjectId(id))).ToListAsync();
        }

    }
}
