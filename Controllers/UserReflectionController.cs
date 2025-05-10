using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Reflectly.Entity;
using Reflectly.Service;
using System.Security.Claims;
using static Reflectly.Controllers.ChallengeController;

namespace Reflectly.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserReflectionController : Controller
    {
        private readonly Reflection_Service _reflectionService;
        private readonly UserReflection_Service _userReflection_Service;

        public UserReflectionController(Reflection_Service reflection_Service,UserReflection_Service userReflection_Service)
        {
            _reflectionService = reflection_Service;
            _userReflection_Service = userReflection_Service;
        }


        //[Authorize]
        [HttpGet("getreflection")]
        public async Task<IActionResult> Get_Reflection()
        {
            // Lấy toàn bộ các Challenge từ dịch vụ
            List<Reflection> reflections = await _reflectionService.Get_Weekly_Reflection_Async();
            return Ok(JsonConvert.SerializeObject(reflections));
        }


        [Authorize]
        [HttpPost("sharereflection")]
        public async Task<IActionResult> Share_Reflection(Share_ReflectionDTO req)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                Guid new_id = Guid.NewGuid();
                DateTime n = DateTime.Now;
                User_Reflection user_Reflection = new User_Reflection
                {
                    UserId = userId,
                    reflection_id = req.ReflectionId,
                    Photos = [],
                    SubmitTime = n,
                    UUID = new_id,
                    reflection = req.UserReflection
                };
                int index = 0;
                if(req.Files != null)
                foreach (IFormFile file in req.Files)
                {
                    if (file == null || file.Length == 0)
                        return BadRequest("File không hợp lệ.");
                    var extension = Path.GetExtension(file.FileName).ToLower();
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                    if (!allowedExtensions.Contains(extension))
                        return BadRequest("Sai dinh dang file");

                    var fileName = $"{new_id}{index}{extension}";
                    index++;
                    // Đường dẫn lưu file trên server
                    var filePath = Path.Combine(Path.Combine(Directory.GetCurrentDirectory(), $"UploadedImages/{userId}"), fileName);

                    // Lưu file vào server
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                        user_Reflection.Photos.Add(fileName);
                    }
                }

                await _userReflection_Service.CreateAsync(user_Reflection);
                return Ok(JsonConvert.SerializeObject(user_Reflection));
            }
            catch (Exception e)
            {
                return BadRequest();
            }
        }

        public class Share_ReflectionDTO
        {
            public required string ReflectionId { get; set; }
            public required string UserReflection { get; set; }
            public List<IFormFile>? Files { get; set; }
        }


        public class Edit_ReflectionDTO
        {
            public required Guid id { get; set; }
            public required string ReflectionId { get; set; }
            public required string UserReflection { get; set; }
            public List<IFormFile>? Files { get; set; }
        }


        [Authorize]
        [HttpPost("editreflection")]
        public async Task<IActionResult> Edit_Reflection(Edit_ReflectionDTO req)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                

                User_Reflection user_Reflection =await _userReflection_Service.GetByIdAsync(req.id.ToString());
                if (user_Reflection == null) return Ok();
                foreach (string s in user_Reflection.Photos)
                {
                    var filePath = Path.Combine(Path.Combine(Directory.GetCurrentDirectory(), $"UploadedImages/{userId}"), s);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
                user_Reflection.Photos.Clear();
                user_Reflection.reflection = req.UserReflection;

                int index = 0;
                if (req.Files != null)
                foreach (IFormFile file in req.Files)
                {
                    if (file == null || file.Length == 0)
                        return BadRequest("File không hợp lệ.");
                    var extension = Path.GetExtension(file.FileName).ToLower();
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif",".webp" };
                    if (!allowedExtensions.Contains(extension))
                        return BadRequest("Sai dinh dang file");

                    var fileName = $"{req.id}{index}{extension}";
                    index++;
                    // Đường dẫn lưu file trên server
                    var filePath = Path.Combine(Path.Combine(Directory.GetCurrentDirectory(), $"UploadedImages/{userId}"), fileName);

                    // Lưu file vào server
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                        user_Reflection.Photos.Add(fileName);
                    }
                }

                await _userReflection_Service.UpdateAsync(req.id.ToString(),user_Reflection);
                return Ok(JsonConvert.SerializeObject(user_Reflection));
            }
            catch (Exception e)
            {
                return BadRequest();
            }
        }

    }
}
