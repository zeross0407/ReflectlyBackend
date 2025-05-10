using MimeKit;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
namespace Reflectly.Service
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string message);
    }


    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }




        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            var emailMessage = new MimeMessage();

            // Thông tin người gửi
            emailMessage.From.Add(new MailboxAddress(
                _configuration["SmtpSettings:SenderName"],
                _configuration["SmtpSettings:SenderEmail"]));

            // Thông tin người nhận
            emailMessage.To.Add(new MailboxAddress("", toEmail));

            emailMessage.Subject = subject;

            // Nội dung email
            emailMessage.Body = new TextPart("html")
            {
                Text = message
            };

            // Sử dụng MailKit để gửi email
            using (var client = new SmtpClient())
            {
                // Kết nối tới server SMTP
                await client.ConnectAsync(_configuration["SmtpSettings:Server"],
                                          int.Parse(_configuration["SmtpSettings:Port"]),
                                          MailKit.Security.SecureSocketOptions.StartTls);

                // Xác thực người gửi
                await client.AuthenticateAsync(_configuration["SmtpSettings:Username"],
                                               _configuration["SmtpSettings:Password"]);

                // Gửi email
                await client.SendAsync(emailMessage);

                // Ngắt kết nối
                await client.DisconnectAsync(true);
            }
        }
    }

}
