using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Reflectly.Entity;
using Reflectly.Models;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Reflectly.Service
{
    public class Reflection_Service
    {
        private readonly IMongoCollection<Reflection> _Reflection_collection;
        private readonly IMongoCollection<User_Reflection> _User_Reflection_collection;

        public Reflection_Service(IOptions<DatabaseSettings> DatabaseSettings)
        {
            var mongoClient = new MongoClient(DatabaseSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(DatabaseSettings.Value.DatabaseName);

            _Reflection_collection = mongoDatabase.GetCollection<Reflection>(GetCollectionName<Reflection>());
            _User_Reflection_collection = mongoDatabase.GetCollection<User_Reflection>(GetCollectionName<User_Reflection>());
        }

        private string GetCollectionName<T>()
        {
            var attribute = typeof(T).GetCustomAttribute<CollectionNameAttribute>();
            return attribute?.Name ?? typeof(T).Name;
        }

        // Lấy tất cả Reflection
        public async Task<List<Reflection>> GetAllAsync()
        {
            return await _Reflection_collection.Find(_ => true).ToListAsync();
        }


        public async Task<List<Reflection>> Get_Weekly_Reflection_Async()
        {
            // Tạo một danh sách để lưu trữ các category cần tìm
            var categories = new List<int> { 1, 2, 3, 4, 5, 6, 7 };

            // Tạo một truy vấn để tìm các bản ghi với category nằm trong danh sách categories
            var filter = Builders<Reflection>.Filter.In(r => r.category, categories);

            // Lấy danh sách các bản ghi từ cơ sở dữ liệu
            var reflections = await _Reflection_collection.Find(filter).ToListAsync();

            // Lấy 7 bản ghi khác nhau
            return reflections.GroupBy(r => r.category)
                              .SelectMany(g => g.Take(1)) // Lấy 1 bản ghi cho mỗi category
                              .Take(7) // Lấy tổng cộng 7 bản ghi
                              .ToList();
        }

        // Thêm Reflection mới
        public async Task<Reflection> CreateAsync(Reflection reflection)
        {
            await _Reflection_collection.InsertOneAsync(reflection);
            return reflection;
        }

        // Sửa Reflection
        public async Task<ReplaceOneResult> UpdateAsync(string id, Reflection reflection)
        {
            return await _Reflection_collection.ReplaceOneAsync(x => x.id == id, reflection);
        }

        // Xóa Reflection
        public async Task<DeleteResult> DeleteAsync(string id)
        {
            return await _Reflection_collection.DeleteOneAsync(x => x.id == id);
        }

        // Tìm kiếm Reflection theo id
        public async Task<Reflection?> GetByIdAsync(string id)
        {
            return await _Reflection_collection.Find(x => x.id == id).FirstOrDefaultAsync();
        }

        // Tìm kiếm Reflection theo description
        public async Task<List<Reflection>> GetByDescriptionAsync(string description)
        {
            var filter = Builders<Reflection>.Filter.Regex("description", new MongoDB.Bson.BsonRegularExpression(description, "i"));
            return await _Reflection_collection.Find(filter).ToListAsync();
        }

        // Tìm kiếm Reflection theo category_id
        public async Task<List<Reflection>> GetByCategoryIdAsync(int categoryId)
        {
            return await _Reflection_collection.Find(x => x.category == categoryId).ToListAsync();
        }
    }
}
