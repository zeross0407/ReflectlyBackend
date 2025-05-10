using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Newtonsoft.Json;
using Reflectly.Entity;
using Reflectly.Service;
using Reflectly.Services;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Security.Claims;
using ThirdParty.Json.LitJson;

namespace Reflectly.Controllers
{
    public enum ActionEnum
    {
        Create,
        Update,
        Delete
    }

    public enum CollectionEnum
    {
        MoodCheckin,
        DailyChallenge,
        Photo,
        VoiceNote,
        ShareReflection,
        Activity,
        Feeling,
        User,
        Heart
    }


    public class DataController : Controller
    {

        private readonly IConfiguration _configuration;
        private readonly Account_Service _Account_Service;
        private readonly TokenService _Token_Service;
        private readonly CRUD_Service<MoodCheckin> _MoodCheckin_Service;
        private readonly CRUD_Service<UserChallenge> _UserChallenge_Service;
        private readonly CRUD_Service<Photo> _Photo_Service;
        private readonly CRUD_Service<VoiceNote> _VoiceNote_Service;
        private readonly CRUD_Service<Activity> _Activity_Service;
        private readonly CRUD_Service<Feeling> _Feeling_Service;
        private readonly UserReflection_Service _UserReflection_Service;
        private readonly Media_Service _media_Service;
        private readonly IEmailService _emailService;
        private readonly Quote_Service _quote_Service;


        public DataController(IConfiguration configuration,
            Account_Service _Service,
            TokenService Token_Service,
            IEmailService emailService,
            CRUD_Service<MoodCheckin> MoodCheckin_Service,
            CRUD_Service<Photo> Photo_Service,
            CRUD_Service<VoiceNote> VoiceNote_Service,
            UserReflection_Service userReflection_Service, 
            CRUD_Service<UserChallenge> userChallenge,
            Quote_Service quote_Service,


            Media_Service media_Service,
            CRUD_Service<Activity> Activity_Service,
            CRUD_Service<Feeling> Feeling_Service
            )
        {
            _Feeling_Service = Feeling_Service;
            _Activity_Service = Activity_Service;
            _media_Service = media_Service;
            _configuration = configuration;
            _Account_Service = _Service;
            _Token_Service = Token_Service;
            _emailService = emailService;
            _MoodCheckin_Service = MoodCheckin_Service;
            _Photo_Service = Photo_Service;
            _VoiceNote_Service = VoiceNote_Service;
            _UserReflection_Service = userReflection_Service;
            _UserChallenge_Service = userChallenge;
            _quote_Service = quote_Service;
        }




        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        [HttpPost("entrysync")]
        public async Task<IActionResult> SyncEntry([FromBody] List<RecordDTO> data_sync)
        {
            //List<RecordDTO> data_sync = JsonConvert.DeserializeObject<List<RecordDTO>>(rq);
            data_sync.Sort((x, y) => x.TimeStamp.CompareTo(y.TimeStamp));
            if (data_sync != null)
            {
                // Giải mã token để lấy thông tin người dùng
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null) return BadRequest();

                List<IDocument> l = new List<IDocument>();
                for (int i = 0; i < data_sync.Count; i++)
                {
                    //IDocument document = Factory_Document(data_sync[i].Name, data_sync[i].json_data);
                    switch (data_sync[i].Name)
                    {
                        case CollectionEnum.MoodCheckin:
                            {
                                await MoodCheckinSync(data_sync[i].Action, data_sync[i].json_data, userId);
                                break;
                            }
                        case CollectionEnum.DailyChallenge:
                            {
                                await UserChallengeSync(data_sync[i].Action, data_sync[i].json_data, userId);
                                break;
                            }
                        case CollectionEnum.VoiceNote:
                            {
                                await VoiceNoteSync(data_sync[i].Action, data_sync[i].json_data, userId);
                                break;
                            }
                        case CollectionEnum.Activity:
                            {
                                await ActivitySync(data_sync[i].Action, data_sync[i].json_data, userId);
                                break;
                            }
                        case CollectionEnum.Feeling:
                            {
                                await FeelingSync(data_sync[i].Action, data_sync[i].json_data, userId);
                                break;
                            }
                        case CollectionEnum.Photo:
                            {
                                await PhotoSync(data_sync[i].Action, data_sync[i].json_data, userId);
                                break;
                            }
                        case CollectionEnum.ShareReflection:
                            {
                                await User_Reflection_Sync(data_sync[i].Action, data_sync[i].json_data, userId);
                                break;
                            }
                        case CollectionEnum.User:
                            {
                                await UserSync(data_sync[i].Action, data_sync[i].json_data, userId);
                                break;
                            }
                        case CollectionEnum.Heart:
                            {
                                await HeartSync(data_sync[i].Action, data_sync[i].json_data, userId);
                                break;
                            }
                        default:
                            break;
                    }
                    //l.Add(document);
                }
                return Ok(JsonConvert.SerializeObject(l));
            }

            return BadRequest();

        }


        private async Task HeartSync(ActionEnum action, string json_data, string user_id)
        {
            try
            {
                await _quote_Service.ToggleHeart(user_id, action, json_data);
            }
            catch (Exception ex)
            {
                int a;
                if (true)
                {
                    int b = 1;
                }
            }

        }


        private async Task PhotoSync(ActionEnum action, string json_data, string user_id)
        {
            switch (action)
            {
                case ActionEnum.Delete:
                    {
                        _media_Service.DeleteFile(json_data + ".webp", user_id);
                        await _Photo_Service.DeleteAsync(json_data);
                        break;
                    }
            }
        }
        private async Task UserChallengeSync(ActionEnum action, string json_data, string user_id)
        {
            switch (action)
            {
                case ActionEnum.Delete:
                    {
                        try
                        {
                            for (int i = 0 ; i < 3; i++){
                                _media_Service.DeleteFile(json_data+$"{i}", user_id);
                            }
                            await _UserChallenge_Service.DeleteAsync(json_data);
                            break;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            break;
                        }

                    }
            }
        }

        private async Task User_Reflection_Sync(ActionEnum action, string json_data, string user_id)
        {
            switch (action)
            {
                case ActionEnum.Delete:
                    {
                        try
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                _media_Service.DeleteFile(json_data + $"{i}", user_id);
                            }
                            await _UserReflection_Service.DeleteAsync(json_data);
                            break;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            break;
                        }
                    }
            }
        }


        private async Task MoodCheckinSync(ActionEnum action, string json_data, string user_id)
        {
            switch (action)
            {
                case ActionEnum.Create:
                    {
                        MoodCheckin moodCheckin = JsonConvert.DeserializeObject<MoodCheckin>(json_data);
                        if (await _MoodCheckin_Service.ExistsAsync(moodCheckin.UUID.ToString())) return;
                        moodCheckin.UserId = user_id;
                        await _MoodCheckin_Service.AddAsync(moodCheckin);
                        break;
                    }
                case ActionEnum.Update:
                    {
                        MoodCheckin moodCheckin = JsonConvert.DeserializeObject<MoodCheckin>(json_data);
                        moodCheckin.UserId = user_id;
                        await _MoodCheckin_Service.Update_by_UUID_Async(moodCheckin.UUID.ToString(), moodCheckin);
                        break;
                    }
                case ActionEnum.Delete:
                    {
                        await _MoodCheckin_Service.DeleteAsync(json_data);
                        break;
                    }
            }
        }


        private async Task VoiceNoteSync(ActionEnum action, string json_data, string user_id)
        {
            try
            {

                switch (action)
                {
                    case ActionEnum.Create:
                        {
                            VoiceNote voiceNote = JsonConvert.DeserializeObject<VoiceNote>(json_data);
                            if (await _VoiceNote_Service.ExistsAsync(voiceNote.UUID.ToString())) return;
                            voiceNote.UserId = user_id;
                            await _VoiceNote_Service.AddAsync(voiceNote);
                            break;
                        }
                    case ActionEnum.Update:
                        {
                            VoiceNote voiceNote = JsonConvert.DeserializeObject<VoiceNote>(json_data);
                            voiceNote.UserId = user_id;
                            await _VoiceNote_Service.Update_by_UUID_Async(voiceNote.UUID.ToString(), voiceNote);
                            break;
                        }
                    case ActionEnum.Delete:
                        {
                            await _VoiceNote_Service.DeleteAsync(json_data);
                            break;
                        }
                }

            }
            catch (Exception e)
            {
                int a;
            }

        
        }


        private async Task ActivitySync(ActionEnum action, string json_data, string user_id)
        {
            switch (action)
            {
                case ActionEnum.Create:
                    {
                        Entity.Activity activity = JsonConvert.DeserializeObject<Activity>(json_data);
                        activity.userId = user_id;
                        await _Activity_Service.AddAsync(activity);
                        break;
                    }
                case ActionEnum.Update:
                    {
                        Entity.Activity activity = await _Activity_Service.GetBy_UUID_Async(json_data);
                        activity.archive = !activity.archive;
                        await _Activity_Service.Update_by_UUID_Async(activity.UUID.ToString(), activity);
                        break;
                    }
            }
        }


        private async Task FeelingSync(ActionEnum action, string json_data, string user_id)
        {
            switch (action)
            {
                case ActionEnum.Create:
                    {
                        Feeling feeling = JsonConvert.DeserializeObject<Feeling>(json_data);
                        feeling.userId = user_id;
                        await _Feeling_Service.AddAsync(feeling);
                        break;
                    }
                case ActionEnum.Update:
                    {
                        Feeling feeling = await _Feeling_Service.GetBy_UUID_Async(json_data);
                        feeling.archive = !feeling.archive;
                        await _Feeling_Service.Update_by_UUID_Async(feeling.UUID.ToString(), feeling);
                        break;
                    }
            }
        }




        private async Task Data_sync<T>(ActionEnum action, T document, CRUD_Service<T> service, Guid UUID)
        {
            switch (action)
            {
                case ActionEnum.Create:
                    await service.AddAsync(document);
                    break;

                case ActionEnum.Update:
                    await service.Update_by_UUID_Async(UUID.ToString(), document);
                    break;

                case ActionEnum.Delete:
                    await service.DeleteAsync(UUID.ToString());
                    break;

                default:

                    break;
            }
        }



        private async Task UserSync(ActionEnum action, string json_data, string user_id)
        {
            switch (action)
            {
                case ActionEnum.Update:
                    {
                        Account ac = await _Account_Service.GetAsync(user_id);
                        ac.Username = json_data;
                        await _Account_Service.UpdateAsync(user_id,ac);
                        break;
                    }
            }
        }




        public class RecordDTO
        {
            public required string id { get; set; }
            public required CollectionEnum Name { get; set; }
            public required ActionEnum Action { get; set; }
            public required string json_data { get; set; }
            public required DateTime TimeStamp { get; set; }
        }



        public class Data_Sync_DTO
        {
            public List<RecordDTO> data { get; set; }
        }






        //public static IDocument Factory_Document(CollectionEnum type, string json_data)
        //{
        //    return type switch
        //    {
        //        CollectionEnum.MoodCheckin => JsonConvert.DeserializeObject<MoodCheckin>(json_data)
        //                                        ?? throw new InvalidOperationException("Deserialization failed for MoodCheckin"),

        //        CollectionEnum.DailyChallenge => JsonConvert.DeserializeObject<DailyChallenge_Complete>(json_data)
        //                                        ?? throw new InvalidOperationException("Deserialization failed for DailyChallenge"),

        //        CollectionEnum.Photo => JsonConvert.DeserializeObject<Photo>(json_data)
        //                                        ?? throw new InvalidOperationException("Deserialization failed for Photo"),

        //        CollectionEnum.VoiceNote => JsonConvert.DeserializeObject<VoiceNote>(json_data)
        //                                        ?? throw new InvalidOperationException("Deserialization failed for ShareReflection"),

        //        _ => throw new ArgumentException("Invalid type", nameof(type)),
        //    };
        //}















        public class AddPhotoDTO
        {
            public string photo { get; set; }
            public IFormFile file { get; set; }
        }




        [Authorize]

        [HttpPost("addphoto")]
        public async Task<IActionResult> AddPhoto([FromForm] IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("No file uploaded.");
                }

                // Kiểm tra định dạng file
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(extension))
                {
                    return BadRequest("Invalid file type.");
                }


                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                Guid newGuid = Guid.NewGuid();
                Photo pt = new Photo
                {
                    UUID = newGuid, // Thiết lập UUID
                    SubmitTime = DateTime.UtcNow,
                    UserId = userId // Thay thế bằng UserId thực tế (ObjectId dưới dạng chuỗi)
                };



                // Lưu tệp vào thư mục
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), $"UploadedImages/{userId}");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                var filePath = Path.Combine(uploadsFolder, pt.UUID + extension);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                await _Photo_Service.AddAsync(pt);

                return Ok(newGuid);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return BadRequest();

        }








    }


}

