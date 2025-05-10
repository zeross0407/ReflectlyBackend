using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp.Formats.Tiff.Compression.Decompressors;
using SkiaSharp;
using System;
using System.Text.RegularExpressions;

namespace Reflectly.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MediaController : Controller
    {
        [HttpGet("random")]
        public IActionResult GetRandomWallpaper()
        {
            string rootDirectory = @"D:\"; // Thư mục gốc
            var random = new Random();

            // Hàm lấy file ngẫu nhiên đệ quy
            string? GetRandomFile(string directory)
            {
                try
                {
                    var directories = Directory.GetDirectories(directory);
                    var files = Directory.GetFiles(directory, "*.*")
                        .Where(file => file.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                                       file.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                                       file.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                                       file.EndsWith(".webp", StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    if (files.Count > 0)
                    {
                        return files[random.Next(files.Count)];
                    }

                    if (directories.Length > 0)
                    {
                        foreach (var dir in directories.OrderBy(x => random.Next())) // Ngẫu nhiên thứ tự duyệt thư mục con
                        {
                            var file = GetRandomFile(dir);
                            if (file != null)
                            {
                                return file; // Trả về file nếu tìm thấy
                            }
                        }
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    // Bỏ qua thư mục không có quyền truy cập
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error accessing directory {directory}: {ex.Message}");
                }

                return null; // Không tìm thấy file nào
            }


            string? randomFile = GetRandomFile(rootDirectory);

            if (randomFile == null || !System.IO.File.Exists(randomFile))
            {
                return NotFound("No image files found.");
            }

            // Nén ảnh bằng SkiaSharp
            byte[] CompressImage(string filePath)
            {
                using var inputStream = System.IO.File.OpenRead(filePath);
                using var originalBitmap = SKBitmap.Decode(inputStream);

                // Xác định kích thước mới với tỉ lệ giữ nguyên
                const int maxWidth = 3000;  // Chiều rộng tối đa
                const int maxHeight = 3000; // Chiều cao tối đa
                int newWidth = originalBitmap.Width;
                int newHeight = originalBitmap.Height;

                if (originalBitmap.Width > maxWidth || originalBitmap.Height > maxHeight)
                {
                    float widthRatio = (float)maxWidth / originalBitmap.Width;
                    float heightRatio = (float)maxHeight / originalBitmap.Height;
                    float scale = Math.Min(widthRatio, heightRatio);

                    newWidth = (int)(originalBitmap.Width * scale);
                    newHeight = (int)(originalBitmap.Height * scale);
                }

                using var resizedBitmap = originalBitmap.Resize(new SKImageInfo(newWidth, newHeight), SKFilterQuality.Medium);
                using var image = SKImage.FromBitmap(resizedBitmap ?? originalBitmap);
                using var data = image.Encode(SKEncodedImageFormat.Jpeg, 75); // Chất lượng 75%
                return data.ToArray();
            }


            byte[] compressedImage;
            try
            {
                compressedImage = CompressImage(randomFile);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error compressing image: {ex.Message}");
            }

            // Trả về ảnh đã nén
            return File(compressedImage, "image/jpeg");
        }


        [HttpGet("thumbnail")]
        public IActionResult GetThumbnail(string path)
        {
            Console.WriteLine($"Raw path: {path}");

            // Giải mã URL nếu cần
            path = Uri.UnescapeDataString(path);

            path = path.Replace("\\", @"\").Replace(@"\\", @"\");
            //path = Regex.Unescape(path);

            if (string.IsNullOrEmpty(path) || !System.IO.File.Exists(path))
            {
                return NotFound("File not found or path is invalid.");
            }

            // Chỉ chấp nhận các định dạng ảnh hợp lệ
            var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var fileExtension = Path.GetExtension(path).ToLowerInvariant();

            if (!validExtensions.Contains(fileExtension))
            {
                return BadRequest("Invalid file format. Only JPG, JPEG, PNG, and WEBP are supported.");
            }

            // Hàm nén ảnh bằng SkiaSharp
            byte[] CompressImage(string filePath)
            {
                using var inputStream = System.IO.File.OpenRead(filePath);
                using var originalBitmap = SKBitmap.Decode(inputStream);

                // Xác định kích thước mới với tỉ lệ giữ nguyên
                const int maxWidth = 600;  // Chiều rộng tối đa cho thumbnail
                const int maxHeight = 1000; // Chiều cao tối đa cho thumbnail
                int newWidth = originalBitmap.Width;
                int newHeight = originalBitmap.Height;

                if (originalBitmap.Width > maxWidth || originalBitmap.Height > maxHeight)
                {
                    float widthRatio = (float)maxWidth / originalBitmap.Width;
                    float heightRatio = (float)maxHeight / originalBitmap.Height;
                    float scale = Math.Min(widthRatio, heightRatio);

                    newWidth = (int)(originalBitmap.Width * scale);
                    newHeight = (int)(originalBitmap.Height * scale);
                }

                using var resizedBitmap = originalBitmap.Resize(new SKImageInfo(newWidth, newHeight), SKFilterQuality.Medium);
                using var image = SKImage.FromBitmap(resizedBitmap ?? originalBitmap);
                using var data = image.Encode(SKEncodedImageFormat.Jpeg, 75); // Chất lượng 75%
                return data.ToArray();
            }

            byte[] compressedImage;
            try
            {
                compressedImage = CompressImage(path);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error compressing image: {ex.Message}");
            }

            // Trả về ảnh thumbnail đã nén
            return File(compressedImage, "image/jpeg");
        }

        [HttpGet("original")]
        public IActionResult Getoriginal(string path)
        {
            Console.WriteLine($"Raw path: {path}");

            // Giải mã URL nếu cần
            path = Uri.UnescapeDataString(path);

            path = path.Replace("\\", @"\").Replace(@"\\", @"\");
            //path = Regex.Unescape(path);

            if (string.IsNullOrEmpty(path) || !System.IO.File.Exists(path))
            {
                return NotFound("File not found or path is invalid.");
            }

            // Chỉ chấp nhận các định dạng ảnh hợp lệ
            var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var fileExtension = Path.GetExtension(path).ToLowerInvariant();

            if (!validExtensions.Contains(fileExtension))
            {
                return BadRequest("Invalid file format. Only JPG, JPEG, PNG, and WEBP are supported.");
            }

            // Hàm nén ảnh bằng SkiaSharp
            

            // Trả về ảnh thumbnail đã nén
            return PhysicalFile(path, "image/jpeg");
        }



        [HttpGet("stream")]
        public async Task<IActionResult> StreamRandomVideo()
        {
            string rootDirectory = @"D:\"; // Thư mục gốc chứa video
            string? randomVideoPath = GetRandomVideo(rootDirectory);

            if (randomVideoPath == null || !System.IO.File.Exists(randomVideoPath))
            {
                return NotFound("No video files found.");
            }

            var fileInfo = new FileInfo(randomVideoPath);
            var fileExtension = Path.GetExtension(randomVideoPath).ToLowerInvariant();

            // Kiểm tra định dạng video hợp lệ (có thể thay đổi tùy theo yêu cầu)
            if (fileExtension != ".mp4" && fileExtension != ".avi" && fileExtension != ".mov")
            {
                return BadRequest("Invalid file format.");
            }

            var videoStream = new FileStream(randomVideoPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var videoContentType = "video/mp4"; // Content type cho video (có thể thay đổi tùy theo loại video)

            // Trả về video dưới dạng stream
            return File(videoStream, videoContentType, Path.GetFileName(randomVideoPath));
        }

        // Hàm để tìm video ngẫu nhiên từ thư mục
        private string? GetRandomVideo(string directory)
        {
            try
            {
                var random = new Random();
                // Bỏ qua thư mục "$RECYCLE.BIN"
                if (directory.Contains("$RECYCLE.BIN"))
                {
                    return null;
                }

                // Lấy tất cả các thư mục con
                var directories = Directory.GetDirectories(directory);
                // Lấy tất cả các file video có định dạng hợp lệ
                var videoFiles = Directory.GetFiles(directory, "*.*")
                    .Where(file => file.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) ||
                                   file.EndsWith(".avi", StringComparison.OrdinalIgnoreCase) ||
                                   file.EndsWith(".mov", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (videoFiles.Count > 0)
                {
                    return videoFiles[random.Next(videoFiles.Count)];
                }

                // Nếu không có video trong thư mục, tìm trong thư mục con
                foreach (var subDir in directories)
                {
                    var subDirVideo = GetRandomVideo(subDir);
                    if (subDirVideo != null)
                    {
                        return subDirVideo;
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Bỏ qua thư mục không có quyền truy cập
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error accessing directory {directory}: {ex.Message}");
            }

            return null;
        }

        // Endpoint quét thư mục theo yêu cầu
        [HttpGet("scan")]
        public IActionResult ScanDirectory([FromQuery] string? path)
        {
            string rootDirectory = @"D:\"; // Đường dẫn mặc định là ổ D

            // Nếu không có tham số `path`, sử dụng đường dẫn mặc định
            string targetPath = string.IsNullOrEmpty(path) ? rootDirectory : path;

            if (!Directory.Exists(targetPath))
            {
                return NotFound($"Path '{targetPath}' does not exist.");
            }

            var directoryContent = GetDirectoryContent(targetPath);

            return Ok(directoryContent);
        }

        // Hàm trả về danh sách thư mục và file trong thư mục hiện tại (không đệ quy)
        private DirectoryContent GetDirectoryContent(string path)
        {
            var directoryContent = new DirectoryContent
            {
                CurrentPath = path,
                Folders = new List<string>(),
                Files = new List<string>()
            };

            try
            {
                // Lấy tất cả các thư mục con
                var directories = Directory.GetDirectories(path);
                foreach (var directory in directories)
                {
                    directoryContent.Folders.Add(Path.GetFileName(directory));
                }

                // Lấy tất cả các file trong thư mục
                var files = Directory.GetFiles(path);
                foreach (var file in files)
                {
                    directoryContent.Files.Add(Path.GetFileName(file));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while accessing directory {path}: {ex.Message}");
            }

            return directoryContent;
        }
    }

    // Lớp đại diện cho danh sách thư mục và file
    public class DirectoryContent
    {
        public string CurrentPath { get; set; } // Đường dẫn hiện tại
        public List<string> Folders { get; set; } // Danh sách thư mục con
        public List<string> Files { get; set; } // Danh sách file
    }

}

