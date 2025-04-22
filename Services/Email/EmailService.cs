using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Services.Email
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendStreamKeyEmailAsync(string email, string rtmpUrl, DateTime startTime, DateTime endTime, string schoolName)
        {
            string smtpServer = _configuration["EmailSettings:SmtpServer"];
            int smtpPort = int.Parse(_configuration["EmailSettings:Port"]);
            string senderEmail = _configuration["EmailSettings:SenderEmail"];
            string senderPassword = _configuration["EmailSettings:SenderPassword"];

            var smtpClient = new SmtpClient(smtpServer)
            {
                Port = smtpPort,
                Credentials = new NetworkCredential(senderEmail, senderPassword),
                EnableSsl = true,
            };

            string fullUrl = rtmpUrl;
            string baseUrl = "rtmps://live.cloudflare.com:443/live";
            string streamKey = fullUrl.Replace(baseUrl, "").Trim('/');

            var mailMessage = new MailMessage
            {
                From = new MailAddress(senderEmail),
                Subject = $"🎬 Livestream từ {schoolName} sắp bắt đầu!",
                Body = $@"
        <div style='font-family:Segoe UI,Roboto,sans-serif;padding:20px;background-color:#f5f5f5;color:#333'>
            <div style='max-width:600px;margin:auto;background:#fff;padding:30px;border-radius:8px;box-shadow:0 2px 10px rgba(0,0,0,0.1)'>

                <h2 style='color:#007BFF'>📺 Thông báo từ {schoolName}</h2>
                <p>Xin chào <strong>Streamer</strong>,</p>
                <p>Buổi livestream của bạn đã được lên lịch và sắp bắt đầu. Dưới đây là thông tin chi tiết:</p>

                <table style='width:100%;margin-top:10px;margin-bottom:20px'>
                    <tr>
                        <td style='font-weight:bold'>📅 Thời gian phát:</td>
                        <td>{startTime:HH:mm} - {endTime:HH:mm} (UTC)</td>
                    </tr>
                    <tr>
                        <td style='font-weight:bold'>🔗 RTMP Server:</td>
                        <td style='color:#007BFF'>{baseUrl}</td>
                    </tr>
                    <tr>
                        <td style='font-weight:bold'>🔑 Stream Key:</td>
                        <td style='color:#007BFF'>{streamKey}</td>
                    </tr>
                </table>

                <p>Hãy sao chép <strong>RTMP Server</strong> và <strong>Stream Key</strong> vào phần mềm phát trực tiếp (như OBS).</p>
                <p>Nếu gặp bất kỳ vấn đề nào, vui lòng liên hệ quản trị viên của trường.</p>

                <hr style='margin:30px 0;border:none;border-top:1px solid #ddd'>
                <p style='font-size:12px;color:#777'>
                    Đây là email tự động từ hệ thống <strong>School TV Show</strong>.<br/>
                    Vui lòng không phản hồi lại email này.
                </p>
            </div>
        </div>",
                IsBodyHtml = true
            };

            mailMessage.To.Add(email);
            await smtpClient.SendMailAsync(mailMessage);
        }

        public async Task SendPasswordResetEmailAsync(string email, string token)
        {
            string smtpServer = _configuration["EmailSettings:SmtpServer"];
            int smtpPort = int.Parse(_configuration["EmailSettings:Port"]);
            string senderEmail = _configuration["EmailSettings:SenderEmail"];
            string senderPassword = _configuration["EmailSettings:SenderPassword"];

            var smtpClient = new SmtpClient(smtpServer)
            {
                Port = smtpPort,
                Credentials = new NetworkCredential(senderEmail, senderPassword),
                EnableSsl = true,
            };

            string baseUrl = _configuration["Frontend:ResetPasswordUrl"];
            string frontendLink = $"{baseUrl}?email={email}&token={token}";

            var mailMessage = new MailMessage
            {
                From = new MailAddress(senderEmail),
                Subject = "Reset Your Password",
                Body = $@"<p>Please reset your password by clicking this link:</p>
                 <a href='{frontendLink}'>Reset Password</a>",
                IsBodyHtml = true,
            };

            mailMessage.To.Add(email);
            await smtpClient.SendMailAsync(mailMessage);
        }

        public async Task SendOtpEmailAsync(string email, string otp)
        {
            string smtpServer = _configuration["EmailSettings:SmtpServer"];
            int smtpPort = int.Parse(_configuration["EmailSettings:Port"]);
            string senderEmail = _configuration["EmailSettings:SenderEmail"];
            string senderPassword = _configuration["EmailSettings:SenderPassword"];

            var smtpClient = new SmtpClient(smtpServer)
            {
                Port = smtpPort,
                Credentials = new NetworkCredential(senderEmail, senderPassword),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(senderEmail),
                Subject = "Your OTP Code",
                Body = $"Your OTP code is: {otp}. It is valid for 5 minutes.",
                IsBodyHtml = true,
            };
            mailMessage.To.Add(email);

            await smtpClient.SendMailAsync(mailMessage);
        }
        public async Task SendOtpReminderEmailAsync(string email)
        {

            Console.WriteLine($"Sending OTP reminder email to: {email}");
            await Task.CompletedTask;
        }
    }
}
