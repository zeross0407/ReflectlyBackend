using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Reflectly.Models;

namespace Reflectly.Entity
{


    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class CollectionNameAttribute : Attribute
    {
        public string Name { get; }
        public CollectionNameAttribute(string name)
        {
            Name = name;
        }
    }


    public interface IDocument
    {
    }


    [CollectionName("Account")]
    public class Account
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("username")]
        public string? Username { get; set; }

        [BsonElement("email")]
        public required string Email { get; set; }

        [BsonElement("password")]
        public required string Password { get; set; }

        [BsonElement("avatar")]
        public string? Avatar { get; set; }

        [BsonElement("journey_token")]
        public string? journey_token { get; set; }

        [BsonElement("password_code")]
        public string? password_code { get; set; }

        [BsonElement("password_code_expire")]
        public DateTime? password_code_expire { get; set; }

        [BsonElement("active")]
        public required bool active { get; set; }

        [BsonElement("deletion_scheduled_at")]
        public DateTime? deletion_scheduled_at { get; set; }

    }


    [CollectionName("AccessToken")]
    public class AccessToken
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; } // Id của token, tự động sinh bởi MongoDB

        [BsonElement("userId")]
        [BsonRepresentation(BsonType.ObjectId)] // Liên kết với tài khoản người dùng
        public required string userId { get; set; }

        [BsonElement("token")]
        public required string token { get; set; } // Giá trị của refresh token

        [BsonElement("expires")]
        public required DateTime expires { get; set; } // Thời điểm hết hạn của token

    }


    public class Entry : IDocument
    {

        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid UUID { get; set; }

        [BsonElement("submittime")]
        public required DateTime SubmitTime { get; set; }

        [BsonElement("userId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string UserId { get; set; }

    }



    [CollectionName("MoodCheckin")]
    public class MoodCheckin : Entry
    {
        [BsonElement("mood")]
        public required double Mood { get; set; }

        [BsonElement("activities")]
        public required List<string> Activities { get; set; } = new List<string>();

        [BsonElement("feelings")]
        public required List<string> Feelings { get; set; } = new List<string>();

        [BsonElement("title")]
        public string? Title { get; set; }

        [BsonElement("description")]
        public string? Description { get; set; }
    }



    [CollectionName("Photo")]
    public class Photo : Entry
    {

    }


    [CollectionName("VoiceNote")]
    public class VoiceNote : Entry
    {
        [BsonElement("title")]
        public string? title { get; set; }

        [BsonElement("description")]
        public string? description { get; set; }
    }





    [CollectionName("Feeling")]
    public class Feeling
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid UUID { get; set; }

        [BsonElement("userId")]
        public string? userId { get; set; }

        [BsonElement("title")]
        public required string title { get; set; }

        [BsonElement("icon")]
        public required int icon { get; set; }

        [BsonElement("archive")]
        public required bool archive { get; set; }
    }

    [CollectionName("Activity")]
    public class Activity
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid UUID { get; set; }

        [BsonElement("userId")]
        public string? userId { get; set; }

        [BsonElement("title")]
        public required string title { get; set; }

        [BsonElement("icon")]
        public required int icon { get; set; }

        [BsonElement("archive")]
        public required bool archive { get; set; }
    }


    #region Quotes
    [CollectionName("Quotes")]
    public class Quote
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("title")]
        public required string title { get; set; }

        [BsonElement("author")]
        public required string author { get; set; }

        [BsonElement("categoryid")]
        //[BsonRepresentation(BsonType.ObjectId)]
        public string? categoryid { get; set; }


    }



    [CollectionName("UserHeart")]
    public class UserHeart
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("userId")]
        public string? userId { get; set; }

        [BsonElement("quote_id")]

        public required string quote_id { get; set; }
    }

    [CollectionName("QuoteCategory")]
    public class QuoteCategory
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("name")]
        public required string name { get; set; }

        [BsonElement("groupid")]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string groupid { get; set; }
    }




    [CollectionName("QuoteGroup")]
    public class QuoteGroup
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("name")]
        public required string name { get; set; }

    }

    #endregion




    #region Challenge
    [CollectionName("ChallengeCategory")]
    public class ChallengeCategory
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("name")]
        public required string name { get; set; }

    }

    [CollectionName("Challenge")]
    public class Challenge
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string id { get; set; }

        [BsonElement("description")]
        public required string description { get; set; }

        [BsonElement("category_id")]
        public string? category_id { get; set; }

    }


    [CollectionName("UserChallenge")]
    public class UserChallenge : Entry
    {

        [BsonElement("photos")]
        public required List<string> Photos { get; set; } = new List<string>();

        [BsonElement("challengeid")]
        public required string challenge_id { get; set; }

        [BsonElement("description")]
        public required string description { get; set; }
    }

    #endregion 




    #region Reflection
    [CollectionName("UserReflection")]
    public class User_Reflection : Entry
    {
        [BsonElement("photos")]
        public required List<string> Photos { get; set; } = new List<string>();

        [BsonElement("reflectionid")]
        public required string reflection_id { get; set; }

        [BsonElement("reflection")]
        public required string reflection { get; set; }
    }


    [CollectionName("Reflection")]
    public class Reflection
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string id { get; set; }

        [BsonElement("description")]
        public required string description { get; set; }

        [BsonElement("category")]
        public required int category { get; set; }
    }
    #endregion

}
