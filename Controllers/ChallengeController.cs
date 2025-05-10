using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Reflectly.Entity;
using Reflectly.Service;
using Reflectly.Services;
using static Reflectly.Controllers.DataController;
using System.Linq;
using System.Security.Claims;
using Newtonsoft.Json;
using iTextSharp.text;


namespace Reflectly.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChallengeController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly Account_Service _Account_Service;
        private readonly TokenService _Token_Service;
        private readonly IEmailService _emailService;
        private readonly CRUD_Service<Challenge> _Challenge_Service;
        private readonly CRUD_Service<ChallengeCategory> _ChallengeCategory_Service;
        private readonly CRUD_Service<UserChallenge> _UserChallenge_Service;



        public ChallengeController(IConfiguration configuration,
            Account_Service _Service,
            TokenService Token_Service,
            IEmailService emailService,
            CRUD_Service<Challenge> Challenge_Service,
            CRUD_Service<ChallengeCategory> ChallengeCategory_Service,
            CRUD_Service<UserChallenge> UserChallenge_Service
            )
        {
            _configuration = configuration;
            _Account_Service = _Service;
            _Token_Service = Token_Service;
            _emailService = emailService;
            _Challenge_Service = Challenge_Service;
            _ChallengeCategory_Service = ChallengeCategory_Service;
            _UserChallenge_Service = UserChallenge_Service;
        }



        [Authorize]
        [HttpGet("getchallenge")]
        public async Task<IActionResult> Get_Challenge(int number)
        {
            // Lấy toàn bộ các Challenge từ dịch vụ
            List<Challenge> challenges = await _Challenge_Service.GetAllAsync();

            // Nếu số lượng yêu cầu lớn hơn số lượng bản ghi hiện có, trả về toàn bộ danh sách
            if (challenges.Count < number)
            {
                number = challenges.Count; // Cập nhật số lượng cần lấy thành số lượng hiện có
            }

            // Lấy các bản ghi ngẫu nhiên từ danh sách
            var randomChallenges = challenges
                .OrderBy(x => Guid.NewGuid()) // Trộn ngẫu nhiên danh sách
                .Take(number)                 // Lấy đúng số lượng bản ghi
                .ToList();

            // Chuyển đổi sang DTO (loại bỏ các thuộc tính không cần thiết)
            var challengeDtos = randomChallenges.Select(challenge => new ChallengeDto
            {
                id = challenge.id,
                description = challenge.description
            }).ToList();

            // Trả về kết quả dưới dạng JSON
            return Ok(JsonConvert.SerializeObject(challengeDtos));
        }








        public class ChallengeDto
        {
            public string id { get; set; }
            public string description { get; set; }
        }


        public class CompleteChallengeDTO
        {
            public required string ChallengeId { get; set; }
            public required string description { get; set; }
            public List<IFormFile> Files { get; set; } // Danh sách các file nhận được
        }


        [Authorize]
        [HttpPost("completechallenge")]
        public async Task<IActionResult>  Complete_Challenge([FromForm] CompleteChallengeDTO req)
        {

            try 
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                Guid new_id = Guid.NewGuid();
                UserChallenge userChallenge = new UserChallenge
                {
                    UserId = userId,
                    challenge_id = req.ChallengeId,
                    Photos = [],
                    SubmitTime = DateTime.UtcNow,
                    UUID = new_id,
                    description = req.description
                };
                int index = 0;
                foreach (IFormFile file in req.Files)
                {
                    if (file == null || file.Length == 0)
                        return BadRequest("File không hợp lệ.");
                    var extension = Path.GetExtension(file.FileName).ToLower();
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                    if (!allowedExtensions.Contains(extension))
                        return BadRequest("Chỉ được phép upload ảnh với định dạng .jpg, .jpeg, .png, hoặc .gif.");

                    var fileName = $"{new_id}{index}{extension}";
                    index++;
                    // Đường dẫn lưu file trên server
                    var filePath = Path.Combine(Path.Combine(Directory.GetCurrentDirectory(), $"UploadedImages/{userId}"), fileName);

                    // Lưu file vào server
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                        userChallenge.Photos.Add(fileName);
                    }
                }

                await _UserChallenge_Service.AddAsync(userChallenge);

                return Ok(JsonConvert.SerializeObject(userChallenge));
            }
            catch(Exception e)
            {
                return BadRequest();
            }


            
        }
        public class UpdateChallengeDTO
        {
            public required string user_challenge_id { get; set; }
            public List<IFormFile> Files { get; set; } // Danh sách các file nhận được
        }

        [Authorize]
        [HttpPost("updatechallenge")]
        public async Task<IActionResult> Update_Challenge([FromForm] UpdateChallengeDTO req)
        {
            if (req.Files.Count == 0 || req.user_challenge_id == null) return BadRequest();

            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                UserChallenge userChallenge = await _UserChallenge_Service.GetBy_UUID_Async(req.user_challenge_id);
                foreach (string s in userChallenge.Photos)
                {
                    var filePath = Path.Combine(Path.Combine(Directory.GetCurrentDirectory(), $"UploadedImages/{userId}"), s);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
                userChallenge.Photos.Clear();


                int index = 0;
                foreach (IFormFile file in req.Files)
                {
                    if (file == null || file.Length == 0)
                        return BadRequest("File không hợp lệ.");
                    var extension = Path.GetExtension(file.FileName).ToLower();
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                    if (!allowedExtensions.Contains(extension))
                        return BadRequest("Chỉ được phép upload ảnh với định dạng .jpg, .jpeg, .png, hoặc .gif.");

                    var fileName = $"{userChallenge.UUID}{index}{extension}";
                    index++;
                    // Đường dẫn lưu file trên server
                    var filePath = Path.Combine(Path.Combine(Directory.GetCurrentDirectory(), $"UploadedImages/{userId}"), fileName);

                    // Lưu file vào server
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                        userChallenge.Photos.Add(fileName);
                    }
                }

                await _UserChallenge_Service.Update_by_UUID_Async(userChallenge.UUID.ToString(),  userChallenge);

                return Ok(JsonConvert.SerializeObject(userChallenge));
            }
            catch (Exception e)
            {
                return BadRequest();
            }



        }


    }
}
