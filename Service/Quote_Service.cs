using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Reflectly.Controllers;
using Reflectly.Entity;
using Reflectly.Models;
using System;
using System.Reflection;

namespace Reflectly.Service
{
    public class Quote_Service
    {
        private readonly IMongoCollection<Quote> _Quote_collection;
        private readonly IMongoCollection<UserHeart> _UserHeart_collection;

        public Quote_Service(IOptions<DatabaseSettings> DatabaseSettings)
        {
            var mongoClient = new MongoClient(DatabaseSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(DatabaseSettings.Value.DatabaseName);

            _Quote_collection = mongoDatabase.GetCollection<Quote>(GetCollectionName<Quote>());
            _UserHeart_collection = mongoDatabase.GetCollection<UserHeart>(GetCollectionName<UserHeart>());
        }

        private string GetCollectionName<T>()
        {
            var attribute = typeof(T).GetCustomAttribute<CollectionNameAttribute>();
            return attribute?.Name ?? typeof(T).Name;
        }

        public async Task<List<Quote>> GetAllAsync()
        {
            return await _Quote_collection.Find(_ => true).ToListAsync();
        }

        public async Task<List<Quote>> Getby_CategoryID_Async(int id)
        {
            return await _Quote_collection.Find(Builders<Quote>.Filter.Eq("categoryid", id.ToString())).ToListAsync();
        }

        public async Task<List<Quote>> GetAll_Hearted_Quotes_Async(string userid)
        {
            List<UserHeart> list =   await _UserHeart_collection.Find(Builders<UserHeart>.Filter.Eq("user_id", userid)).ToListAsync();
            List<Quote> rs = [];

            foreach (var item in list)
            {
                Quote q = await _Quote_collection.Find(Builders<Quote>.Filter.Eq("_id", item.quote_id)).FirstOrDefaultAsync();
                rs.Add(q);
            }
            return rs;
        }
        public async Task<List<string>> GetAll_Hearted_Async(string userid)
        {
            List<UserHeart> list = await _UserHeart_collection.Find(Builders<UserHeart>.Filter.Eq("user_id", userid)).ToListAsync();
            List<string> rs = [];

            foreach (var item in list)
            {
                rs.Add(item.quote_id);
            }
            return rs;
        }

        public async Task ToggleHeart(string userId, ActionEnum action, string quoteId)
        {
            try
            {
                if (action == ActionEnum.Create)
                {
                    // Kiểm tra nếu đã tồn tại thì không thêm lại
                    var existingHeart = await _UserHeart_collection.Find(x => x.userId == userId && x.quote_id == quoteId).FirstOrDefaultAsync();
                    if (existingHeart == null)
                    {
                        var newHeart = new UserHeart
                        {
                            userId = userId,
                            quote_id = quoteId,
                        };
                        await _UserHeart_collection.InsertOneAsync(newHeart);
                    }
                }
                else if (action == ActionEnum.Delete)
                {
                    // Xóa bản ghi thả tim
                    await _UserHeart_collection.DeleteOneAsync(x => x.userId == userId && x.quote_id == quoteId);
                }
            }
            catch (Exception ex) 
            {
            
            }
            
        }


    }
}
