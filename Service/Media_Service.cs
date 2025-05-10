using System.IO;
using System.Threading.Tasks;

namespace Reflectly.Service
{
    public class Media_Service
    {
        private readonly string _uploadFolderPath;

        public Media_Service()
        {
            // Đặt đường dẫn lưu trữ file
            _uploadFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "UploadedImages");
            if (!Directory.Exists(_uploadFolderPath))
            {
                Directory.CreateDirectory(_uploadFolderPath);
            }
        }

        // Thêm file vào thư mục lưu trữ
        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string User_ID)
        {
            var filePath = Path.Combine(_uploadFolderPath, User_ID, fileName);

            using (var fileStreamOutput = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(fileStreamOutput);
            }

            return filePath;
        }

        // Xóa file theo tên file
        public bool DeleteFile(string fileName, string userId)
        {
            // Kiểm tra nếu fileName không có đuôi .webp thì thêm vào
            if (!fileName.EndsWith(".webp", StringComparison.OrdinalIgnoreCase))
            {
                fileName += ".webp";
            }

            var filePath = Path.Combine(_uploadFolderPath, userId, fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return true;
            }
            return false;
        }

    }
}
