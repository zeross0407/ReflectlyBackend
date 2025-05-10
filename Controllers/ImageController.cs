using Microsoft.AspNetCore.Mvc;

namespace Reflectly.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Http;
    using System.IO;
    using System.Threading.Tasks;
    using System.Linq;

    namespace ImageUploadAPI.Controllers
    {
        [Route("api/[controller]")]
        [ApiController]
        public class ImagesController : ControllerBase
        {
            private readonly string _storagePath = Path.Combine(Directory.GetCurrentDirectory(), "UploadedImages");

            public ImagesController()
            {
                // Kiểm tra và tạo thư mục lưu trữ nếu chưa có
                if (!Directory.Exists(_storagePath))
                {
                    Directory.CreateDirectory(_storagePath);
                }
            }

            // API để upload file
            [HttpPost("upload")]
            public async Task<IActionResult> UploadImage(IFormFile file)
            {
                // Kiểm tra xem file có hợp lệ hay không
                if (file == null || file.Length == 0)
                    return BadRequest("File không hợp lệ.");

                // Kiểm tra định dạng file ảnh (chỉ chấp nhận .jpg, .jpeg, .png, .gif)
                var extension = Path.GetExtension(file.FileName).ToLower();
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };

                if (!allowedExtensions.Contains(extension))
                    return BadRequest("Chỉ được phép upload ảnh với định dạng .jpg, .jpeg, .png, hoặc .gif.");

                // Đặt tên file ngẫu nhiên để tránh trùng lặp
                var fileName = $"{Path.GetFileNameWithoutExtension(file.FileName)}_{System.Guid.NewGuid()}{extension}";

                // Đường dẫn lưu file trên server
                var filePath = Path.Combine(_storagePath, fileName);

                // Lưu file vào server
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Trả về đường dẫn file đã upload (hoặc có thể trả lại ID hoặc thông tin cần thiết)
                return Ok(new { FilePath = filePath });
            }
        }
    }

}
