using Microsoft.AspNetCore.Mvc;
using System.Text;
using TheArtOfDev.HtmlRenderer.PdfSharp;
using PdfSharp.Pdf;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using System.Drawing;
namespace Reflectly.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class PdfController : ControllerBase
    {
        [HttpPost("generate-pdf")]
        public IActionResult GeneratePdf()
        {
            // HTML mẫu có chứa hình WebP, icon SVG và font TTF
            string htmlContent = @"
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    @font-face {
                        font-family: 'CustomFont';
                        src: url('data:font/truetype;charset=utf-8;base64," + GetBase64Font() + @"') format('truetype');
                    }
                    body {
                        font-family: 'CustomFont', Arial, sans-serif;
                        text-align: center;
                    }
                    .image-container {
                        margin-top: 50px;
                    }
                    .icon-container {
                        margin-top: 20px;
                    }
                </style>
            </head>
            <body>
                <h1>PDF với hình ảnh WebP và icon SVG</h1>
                <div class='image-container'>
                    <img src='data:image/webp;base64," + GetBase64Image() + @"' alt='WebP Image' style='max-width: 100%;' />
                </div>
                <div class='icon-container'>
                    <img src='data:image/svg+xml;base64," + GetBase64SvgIcon() + @"' alt='SVG Icon' width='50' height='50' />
                </div>
            </body>
            </html>";

            // Tạo PDF từ HTML
            var pdfDocument = PdfGenerator.GeneratePdf(htmlContent, PdfSharp.PageSize.A4);

            // Chuyển đổi PDF thành một file stream
            var ms = new MemoryStream();
            pdfDocument.Save(ms);
            ms.Position = 0;

            // Trả về file PDF
            return File(ms, "application/pdf", "generated-file.pdf");
        }

        // Hàm để lấy base64 của font TTF
        private string GetBase64Font()
        {
            string fontPath = Path.Combine(Directory.GetCurrentDirectory(), "UploadedImages", "Quicksand-Bold.ttf");
            byte[] fontBytes = System.IO.File.ReadAllBytes(fontPath);
            return Convert.ToBase64String(fontBytes);
        }

        // Hàm để lấy base64 của hình WebP
        private string GetBase64Image()
        {
            string imagePath = Path.Combine(Directory.GetCurrentDirectory(), "UploadedImages", "image.webp");
            byte[] imageBytes = System.IO.File.ReadAllBytes(imagePath);
            return Convert.ToBase64String(imageBytes);
        }

        // Hàm để lấy base64 của icon SVG
        private string GetBase64SvgIcon()
        {
            string svgPath = Path.Combine(Directory.GetCurrentDirectory(), "UploadedImages", "icon.svg");
            string svgContent = System.IO.File.ReadAllText(svgPath);
            byte[] svgBytes = Encoding.UTF8.GetBytes(svgContent);
            return Convert.ToBase64String(svgBytes);
        }
    }

}

