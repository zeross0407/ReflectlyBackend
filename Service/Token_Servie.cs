using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Driver;
using Reflectly.Entity;
using Reflectly.Models;
using Reflectly.Services;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

namespace Reflectly.Service
{
    public class TokenService
    {
        private readonly IConfiguration _configuration;
        private readonly IMongoCollection<AccessToken> _RefreshTokenCollection;

        public TokenService(IConfiguration configuration, IOptions<DatabaseSettings> DatabaseSettings)
        {
            _configuration = configuration;

            var mongoClient = new MongoClient(DatabaseSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(DatabaseSettings.Value.DatabaseName);
            _RefreshTokenCollection = mongoDatabase.GetCollection<AccessToken>(GetCollectionName<AccessToken>());
        }

        private string GetCollectionName<T>()
        {
            var attribute = typeof(T).GetCustomAttribute<CollectionNameAttribute>();
            return attribute?.Name ?? typeof(T).Name;
        }

        // Tạo một refresh token ngẫu nhiên
        public async Task<string> GenerateRefreshToken(string user_id)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            JwtSecurityTokenHandler jwt_handler = new JwtSecurityTokenHandler();
            var token = new JwtSecurityToken
                (
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    claims: new[]
                    {
                    new Claim(JwtRegisteredClaimNames.Sub, user_id),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim("random", Guid.NewGuid().ToString())
                    },
                    expires: DateTime.Now.AddMonths(6),
                    signingCredentials: credentials
                );
            await SaveRefreshToken(user_id, jwt_handler.WriteToken(token), DateTime.Now.AddDays(30));

            return jwt_handler.WriteToken(token);
        }

        // Lưu refresh token vào database
        public async Task SaveRefreshToken(string userId, string refreshToken, DateTime expiration)
        {
            var refreshTokenModel = new AccessToken
            {
                userId = userId,
                token = refreshToken,
                expires = expiration
            };
            // Lưu vào MongoDB hoặc database của bạn
            AccessToken old = await _RefreshTokenCollection.Find(Builders<AccessToken>.Filter.Eq("userId", new ObjectId(userId) )).FirstOrDefaultAsync();
            if (old != null)
            {
                try
                {
                    refreshTokenModel.Id = old.Id;
                    await _RefreshTokenCollection.ReplaceOneAsync(
                Builders<AccessToken>.Filter.Eq("_id", new ObjectId(old.Id)),
                refreshTokenModel);

                    return;
                }
                catch(Exception e)
                {
                    int a = 0;
                }
                
                
            }

            await _RefreshTokenCollection.InsertOneAsync(refreshTokenModel);
        }



        // Hàm để đổi refresh token lấy access token
        public async Task<string> ExchangeRefreshTokenForAccessToken(string refreshToken)
        {
            // Tìm refresh token trong database
            var tokenFromDb = await _RefreshTokenCollection.Find(x => x.token == refreshToken).FirstOrDefaultAsync();

            if (tokenFromDb == null || tokenFromDb.expires < DateTime.UtcNow)
            {
                throw new SecurityTokenException("Invalid or expired refresh token");
            }

            // Nếu token hợp lệ, tạo access token mới
            var newAccessToken = await GenerateAccessToken(tokenFromDb.userId);

            // Có thể cập nhật lại thời gian hoặc thu hồi refresh token (tùy nhu cầu)
            // Update refresh token hoặc generate refresh token mới

            return newAccessToken;
        }

        // Hàm tạo access token
        public async Task<string> GenerateAccessToken(string userId)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                new Claim(ClaimTypes.NameIdentifier, userId)
                }),
                Expires = DateTime.UtcNow.AddMinutes(1000), // Access token sống 1000 phust
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }




        public async Task<AccessToken> Get_by_token_Async(string token) =>
        await _RefreshTokenCollection.Find(Builders<AccessToken>.Filter.Eq("token", token)).FirstOrDefaultAsync();

        public async Task DeleteByUserIdAsync(string userId)
        {
            var filter = Builders<AccessToken>.Filter.Eq("userId", userId);
            await _RefreshTokenCollection.DeleteManyAsync(filter);
        }


    }

}
