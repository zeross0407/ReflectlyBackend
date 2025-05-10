using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Reflectly.Entity;
using Reflectly.Service;
using Reflectly.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Net.NetworkInformation;




namespace Reflectly.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly Account_Service _Account_Service;
        private readonly TokenService _Token_Service;
        private readonly CRUD_Service<MoodCheckin> _MoodCheckin_Service;
        private readonly IEmailService _emailService;
        private readonly CRUD_Service<UserChallenge> _UserChallenge_Service;
        private readonly CRUD_Service<Photo> _Photo_Service;
        private readonly CRUD_Service<VoiceNote> _VoiceNote_Service;
        private readonly UserReflection_Service _UserReflection_Service;
        private readonly CRUD_Service<Activity> _Activity_Service;
        private readonly CRUD_Service<Feeling> _Feeling_Service;
        public AccountController(
            IConfiguration configuration,
            Account_Service _Service,
            TokenService Token_Service,
            IEmailService emailService,
            CRUD_Service<MoodCheckin> MoodCheckin_Service,

            CRUD_Service<Photo> Photo_Service,
            CRUD_Service<VoiceNote> VoiceNote_Service,
            UserReflection_Service userReflection_Service,
            CRUD_Service<UserChallenge> userChallenge,
            CRUD_Service<Activity> Activity_Service,
            CRUD_Service<Feeling> Feeling_Service

            )
        {
            _configuration = configuration;
            _Account_Service = _Service;
            _Token_Service = Token_Service;
            _emailService = emailService;
            _MoodCheckin_Service = MoodCheckin_Service;
            _Photo_Service = Photo_Service;
            _VoiceNote_Service = VoiceNote_Service;
            _UserReflection_Service = userReflection_Service;
            _UserChallenge_Service = userChallenge;
            _Feeling_Service = Feeling_Service;
            _Activity_Service = Activity_Service;
        }


        [HttpGet]
        public async Task<List<Account>> Get() => await _Account_Service.GetAsync();




        [HttpPost("login")]
        public async Task<IActionResult> login([FromBody] LoginModel login)
        {
            List<Account> ac = await _Account_Service.Get_by_Email_Async(login.Email);
            
            if (ac.Count == 1 && BCrypt.Net.BCrypt.Verify(login.Password, ac[0].Password))
            {
                if (ac[0].active == false)
                {
                    ac[0].active = true;
                    await _Account_Service.UpdateAsync(ac[0].Id, ac[0]);
                }


                var refresh_token = await _Token_Service.GenerateRefreshToken(ac[0].Id);
            
                List<MoodCheckin> mood_checkin_list = await _MoodCheckin_Service.GetBy_UserID_Async(ac[0].Id);
                List<Photo> photo_list = await _Photo_Service.GetBy_UserID_Async(ac[0].Id);
                List<UserChallenge> user_challenge_list = await _UserChallenge_Service.GetBy_UserID_Async(ac[0].Id);
                List<User_Reflection> user_reflection_list = await _UserReflection_Service.GetBy_UserID_Async(ac[0].Id);
                List<VoiceNote> voicenote_list = await _VoiceNote_Service.GetBy_UserID_Async(ac[0].Id);
                List<Activity> activity_list = await _Activity_Service.GetBy_UserID_Async(ac[0].Id);
                List<Feeling> feeling_list = await _Feeling_Service.GetBy_UserID_Async(ac[0].Id);
                return Ok(new
                {
                    refresh_token,
                    ac[0].Email,
                    ac[0].Avatar,
                    ac[0].Username,
                    mood_checkin_list,
                    photo_list,
                    user_challenge_list,
                    user_reflection_list,
                    voicenote_list,
                    activity_list,
                    feeling_list
                });
            }
            return Unauthorized();
        }




        [HttpPost("forgotpassword")]
        public async Task<IActionResult> forgot_password(string email)
        {
            List<Account> ac = await _Account_Service.Get_by_Email_Async(email);
            if (ac.Count == 1)
            {
                Random random = new Random();
                string code = random.Next(1000, 9999).ToString();
                ac[0].password_code = code;
                ac[0].password_code_expire = DateTime.Now.AddHours(1);
                await _Account_Service.UpdateAsync(ac[0].Id, ac[0]);
                _emailService.SendEmailAsync(ac[0].Email, "Reset Password", $"Your reset code is :{code}");
                return Ok();
            }
            return NotFound();
        }



        [HttpPost("changepassword")]
        public async Task<IActionResult> change_password(string email, string code, string new_password)
        {
            List<Account> ac = await _Account_Service.Get_by_Email_Async(email);
            if (ac.Count == 1)
            {
                if (ac[0].password_code == code && ac[0].password_code_expire < DateTime.Now)
                {
                    ac[0].Password = BCrypt.Net.BCrypt.HashPassword(new_password);
                    await _Account_Service.UpdateAsync(ac[0].Id, ac[0]);
                    return Ok();
                }
            }
            return BadRequest();
        }





        public class LoginModel
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }



        [Authorize]
        [HttpPost("updateavatar")]
        public async Task<IActionResult> UpdateAvatar(IFormFile file)
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

            // Lưu tệp vào thư mục
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), $"UploadedImages/{userId}");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            //var fileName = Guid.NewGuid() + extension; // Tạo tên file duy nhất
            var filePath = Path.Combine(uploadsFolder, "avatar" + extension);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Ok();
        }


        [Authorize]
        [HttpPost("deleteaccount")]
        public async Task<IActionResult> DeleteAccount()
        {

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Account ac = await _Account_Service.GetAsync(userId);
            if (ac != null)
            {
                ac.active = false;
                ac.deletion_scheduled_at = DateTime.UtcNow.AddDays(7);
                await _Account_Service.UpdateAsync(userId, ac);
                return Ok();
            }
            return BadRequest();
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            if (!Regex.IsMatch(registerDto.Email, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"))
                return BadRequest("Email Invalid");
            // Kiểm tra xem email đã tồn tại chưa
            var existingUser = await _Account_Service.Get_by_Email_Async(registerDto.Email);
            if (existingUser != null && existingUser.Count > 0)
            {
                return BadRequest("Email Existed.");
            }

            // Hash mật khẩu
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

            // Tạo đối tượng Account
            var newAccount = new Account
            {
                Username = registerDto.Username,
                Email = registerDto.Email,
                Password = hashedPassword,
                Avatar = null,
                active = true,
            };

            // Lưu vào MongoDB
            String id = await _Account_Service.CreateAsync(newAccount);

            // Tạo Access Token và Refresh Token
            var refreshToken = await _Token_Service.GenerateRefreshToken(id);


            // Lưu refresh token vào cơ sở dữ liệu
            var refreshTokenEntity = new AccessToken
            {
                token = refreshToken,
                userId = newAccount.Id,
                expires = DateTime.UtcNow.AddMonths(6)
            };



            var userFolder = Path.Combine(Directory.GetCurrentDirectory(), $"UploadedImages/{newAccount.Id}");
            if (!Directory.Exists(userFolder))
            {
                Directory.CreateDirectory(userFolder);
            }



            return Ok(new
            {
                RefreshToken = refreshToken,
                User = new
                {
                    Id = newAccount.Id,
                    Username = newAccount.Username,
                    Email = newAccount.Email,
                    Avatar = newAccount.Avatar
                }
            });
        }

        public class RegisterDto
        {
            public string Username { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
        }





        [Authorize]
        [HttpGet("media/{mediaId}")]
        public IActionResult GetMedia(string mediaId)
        {
            // Giải mã token để lấy thông tin người dùng
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Kiểm tra quyền truy cập của người dùng đối với tài nguyên này
            if (!HasAccessToMedia(userId, mediaId))
            {
                return Forbid(); // Trả về lỗi nếu không có quyền truy cập
            }

            // Nếu hợp lệ, trả về tài nguyên media
            var mediaFile = LoadMediaFromFileSystem(mediaId, userId);
            //return File(mediaFile, "image/jpeg");
            // Kiểm tra xem file có tồn tại không
            if (string.IsNullOrEmpty(mediaFile) || !System.IO.File.Exists(mediaFile))
            {
                return NotFound(); // Trả về 404 nếu không tìm thấy file
            }

            // Nếu file hợp lệ, trả về tài nguyên media
            return PhysicalFile(mediaFile, "image/jpeg"); // Thay đổi mime type nếu cần thiết

        }


        private bool HasAccessToMedia(string userId, string mediaId)
        {
            //// Giả sử mỗi tài nguyên media đều có một chủ sở hữu
            //var media = _mediaRepository.GetById(mediaId);
            //return media != null && media.OwnerId == userId;
            return true;
        }


        private string LoadMediaFromFileSystem(string mediaId, string userid)
        {
            var mediapath = Path.Combine(Directory.GetCurrentDirectory(), $"UploadedImages/{userid}/{mediaId}");
            return mediapath;
        }



        string pdf_day(DateTime time)
        {
            return $"<h2 style=\"text-align: left;\">{time.ToString()}</h2>";
        }




        [Authorize]
        [HttpPost("generate-html")]
        public async Task<IActionResult> GenerateHtml()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Đường dẫn đến thư mục 'upload' của ứng dụng
            string uploadDirectory = Path.Combine(Directory.GetCurrentDirectory(), "UploadedImages");

            // Đọc tệp ảnh và font từ thư mục upload và chuyển đổi sang Base64
            string avatarPath = Path.Combine(uploadDirectory, userId, "avatar.png");
            string imageBase64 = ConvertImageToBase64(avatarPath);

            string fontPath = Path.Combine(uploadDirectory, "Quicksand-Bold.ttf");
            string fontBase64 = ConvertFileToBase64(fontPath);

            // Tạo danh sách nhật ký
            List<Entry> list =
            [
                .. await _MoodCheckin_Service.GetBy_UserID_Async(userId),
                .. await _Photo_Service.GetBy_UserID_Async(userId),
                .. await _UserChallenge_Service.GetBy_UserID_Async(userId),
                .. await _UserReflection_Service.GetBy_UserID_Async(userId),
                .. await _VoiceNote_Service.GetBy_UserID_Async(userId),
            ];
            var sortedList = list.OrderBy(doc => doc.SubmitTime).ToList();

            string content = "";
            foreach (var entry in sortedList)
            {
                content += pdf_day(entry.SubmitTime);

                // Xử lý các đối tượng ảnh
                if (entry is Photo)
                {
                    content += $"<p class='title'>Photo</p>";
                    string imgPath = Path.Combine(uploadDirectory, userId, $"{entry.UUID}.png");
                    imgPath = imgPath.Replace("\\", "/");
                    content += $"<img src=\"file:///{imgPath}\" class='photo'/>\r\n";
                }
                else if (entry is VoiceNote voiceNote)
                {
                    content += $"<p class='title'>VoiceNote</p>";
                    content += $"<p class='title'>{voiceNote.title}</p>";
                    content += $"<p class='description'>{voiceNote.description}</p>";
                }
                // Xử lý UserChallenge với ảnh
                else if (entry is UserChallenge challenge)
                {
                    content += $"<p class='title'>Challenge</p>";
                    content += $"<p class='description'>Today I will {challenge.description}</p>";
                    foreach (var photo in challenge.Photos)
                    {
                        string imgPath = Path.Combine(uploadDirectory, userId, photo);
                        imgPath = imgPath.Replace("\\", "/");
                        content += $"<img src=\"file:///{imgPath}\" class='photo'/>\r\n";
                    }
                }

                // Xử lý User_Reflection với ảnh
                else if (entry is User_Reflection reflection)
                {
                    content += $"<p class='title'>Reflection</p>";
                    content += $"<p class='description'>{reflection.reflection}</p>";
                    foreach (var photo in reflection.Photos)
                    {
                        string imgPath = Path.Combine(uploadDirectory, userId, photo);
                        imgPath = imgPath.Replace("\\", "/");
                        content += $"<img src=\"file:///{imgPath}\" class='photo'/>\r\n";
                    }
                }
                // Xử lý MoodCheckin với SVG
                else if (entry is MoodCheckin moodCheckin)
                {
                    content += $"<p class='title'>MoodCheck-in</p>";
                    content += $"<p class='title'>{moodCheckin.Title}</p>";
                    content += $"<p class='description'>{moodCheckin.Description}</p>";



                    var activitiesMap = new Dictionary<string, Activity>
                    {
                        { "0", new Activity { UUID = Guid.NewGuid(), icon = 30, title = "weather", userId = "", archive = false } },
                        { "1", new Activity { UUID = Guid.NewGuid(), icon = 12, title = "work", userId = "", archive = false } },
                        { "2", new Activity { UUID = Guid.NewGuid(), icon = 13, title = "achievement", userId = "", archive = false } },
                        { "3", new Activity { UUID = Guid.NewGuid(), icon = 34, title = "candy", userId = "", archive = false } },
                        { "4", new Activity { UUID = Guid.NewGuid(), icon = 35, title = "gaming", userId = "", archive = false } },
                        { "5", new Activity { UUID = Guid.NewGuid(), icon = 16, title = "schedule", userId = "", archive = false } },
                        { "6", new Activity { UUID = Guid.NewGuid(), icon = 37, title = "pancake", userId = "", archive = false } },
                        { "7", new Activity { UUID = Guid.NewGuid(), icon = 38, title = "bread", userId = "", archive = false } }
                    };

                    var feelingsMap = new Dictionary<string, Feeling>
                    {
                        { "0", new Feeling { UUID = Guid.NewGuid(), icon = 2, title = "happy", userId = "", archive = false } },
                        { "1", new Feeling { UUID = Guid.NewGuid(), icon = 3, title = "confused", userId = "", archive = false } },
                        { "2", new Feeling { UUID = Guid.NewGuid(), icon = 6, title = "down", userId = "", archive = false } },
                        { "3", new Feeling { UUID = Guid.NewGuid(), icon = 67, title = "angry", userId = "", archive = false } },
                        { "4", new Feeling { UUID = Guid.NewGuid(), icon = 26, title = "awkward", userId = "", archive = false } }
                    };

                    List<Activity> l1 = await _Activity_Service.GetBy_UserID_Async(userId);
                    foreach (Activity activity in l1)
                    {
                        activitiesMap[activity.UUID.ToString()] = activity;
                    }
                    List<Feeling> l2 = await _Feeling_Service.GetBy_UserID_Async(userId);
                    foreach (Feeling feeling in l2)
                    {
                        feelingsMap[feeling.UUID.ToString()] = feeling;
                    }


                    content += $"<div style='display: flex; align-items: center;'>";
                    foreach (var activity in moodCheckin.Activities)
                    {
                        string svgPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "all", $"({activitiesMap[activity].icon}).svg");
                        string svgContent = System.IO.File.Exists(svgPath) ? System.IO.File.ReadAllText(svgPath) : string.Empty;

                        if (!string.IsNullOrEmpty(svgContent))
                        {
                            // Sử dụng flexbox để xếp icon và title trên cùng 1 dòng
                            content +=
                                        $"<div>{svgContent}</div>" + // Thêm icon SVG vào
                                        $"<div style='margin-left: 10px;margin-right: 20px; white-space: nowrap;'>{activitiesMap[activity].title}</div>";
                        }
                    }
                    content += $"</div><br>";
                    content += $"<div style='display: flex; align-items: center;'>";
                    foreach (var feeling in moodCheckin.Feelings)
                    {
                        string svgPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "all", $"({feelingsMap[feeling].icon}).svg");
                        string svgContent = System.IO.File.Exists(svgPath) ? System.IO.File.ReadAllText(svgPath) : string.Empty;

                        if (!string.IsNullOrEmpty(svgContent))
                        {
                            // Sử dụng flexbox để xếp icon và title trên cùng 1 dòng
                            content +=
                                        $"<div>{svgContent}</div>" + // Thêm icon SVG vào
                                        $"<div style='margin-left: 10px;margin-right: 20px; white-space: nowrap;'>{feelingsMap[feeling].title}</div>";
                        }
                    }
                    content += $"</div>";

                }
            }

            // Tạo nội dung HTML cho file HTML
            string htmlContent = $@"
            <html>
                <head>
                    <title>Journal</title>
                    <style>
                        @font-face {{
                            font-family: 'CustomFont';
                            src: url(data:font/ttf;base64,{fontBase64}) format('truetype');
                        }}
                        body {{
                            font-family: 'CustomFont', Arial, sans-serif;
                            margin: 0 auto;
                            max-width: 600px;
                        }}
                        h1 {{
                            color: #333;
                        }}
                        .title {{font - weight: bold; /* Chữ đậm */
                            font-size: 24px;   /* Kích thước lớn hơn */
                            color: #333;       /* Màu chữ đậm (tối hơn) */
                        }}

                        .description {{font - weight: 300;  /* Chữ nhạt (font-weight thấp hơn) */
                            font-size: 20px;   /* Kích thước nhỏ hơn */
                            color: #888;       /* Màu chữ nhạt (sáng hơn) */
                        }}
                        .photo {{
                            display: block;
                            margin: auto;
                            width: 500px;
                            height: auto;
                            object-fit: cover;
                            border-radius: 20px; /* Bo cong 20px */
                        }}
                        .centered-image {{
                            display: block;
                            margin: auto;
                            width: 300px;
                            height: 300px;
                            border-radius: 50%;
                            object-fit: cover;
                        }}

                    </style>
                </head>
                <body>
                    <img src=""file:///{avatarPath}"" class='centered-image'/>
                    <h1 style=""text-align: center;"">Duc's Journal</h1>
                    {content}
                </body>
            </html>";

            // Tạo tên file HTML và lưu vào thư mục
            string fileName = $"journey.html";
            string filePath = Path.Combine(uploadDirectory, userId, fileName);

            // Lưu HTML vào file
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.Write(htmlContent);
            }

            string token = await _Token_Service.GenerateAccessToken(userId);
            Account a = await _Account_Service.GetAsync(userId);
            if (a != null)
            {
                a.journey_token = token;
                await _Account_Service.UpdateAsync(a.Id, a);
                string downloadLink = $"http://localhost:5124/api/Account/journey/{token}";
                string emailContent = $@"
                <p>Hello {a.Username},</p>
                <p>Your journal is ready for download. Click the link below to download:</p>
                <p><a href='{downloadLink}'>Download your journal</a></p>
                <br>
                <p>Best regards,<br>Your App Team</p>";

                await _emailService.SendEmailAsync(a.Email, "Your Journal is Ready", emailContent);

            }

            return Ok(new { Message = "HTML file generated and saved successfully." });
        }







        // Hàm chuyển file sang Base64
        private string ConvertFileToBase64(string filePath)
        {
            if (System.IO.File.Exists(filePath))
            {
                byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
                return Convert.ToBase64String(fileBytes);
            }
            return string.Empty;
        }

        // Hàm chuyển ảnh sang Base64
        private string ConvertImageToBase64(string imagePath)
        {
            if (System.IO.File.Exists(imagePath))
            {
                byte[] imageBytes = System.IO.File.ReadAllBytes(imagePath);
                return Convert.ToBase64String(imageBytes);
            }
            return string.Empty;
        }





        [HttpGet("journey/{token}")]
        public IActionResult GetJourney(string token)
        {

            string userId = null;
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            try
            {
                // Xác minh và giải mã token
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true, // Kiểm tra thời gian sống của token
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidAudience = _configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                }, out SecurityToken validatedToken);

                // Lấy userId từ claims
                var jwtToken = validatedToken as JwtSecurityToken;
                if (jwtToken != null)
                {
                    var claim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                    claim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
                    claim = jwtToken.Claims.FirstOrDefault(c => c.Type == "nameid");

                    if (claim != null)
                    {
                        userId = claim.Value;
                        var mediaFile = LoadMediaFromFileSystem("journey.html", userId);

                        if (string.IsNullOrEmpty(mediaFile) || !System.IO.File.Exists(mediaFile))
                        {
                            return NotFound(); // Trả về 404 nếu không tìm thấy file
                        }

                        // Nếu file hợp lệ, trả về tài nguyên media
                        return PhysicalFile(mediaFile, "text/html", $"journal.html");
                    }
                }


            }
            catch (Exception ex)
            {
                return BadRequest("token expired");
            }

            return BadRequest();



        }



    }
}
