using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

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

        public async Task SendStatusUserAsync(string email, string status)
        {
            string? smtpServer = _configuration["EmailSettings:SmtpServer"];
            int smtpPort = int.Parse(_configuration["EmailSettings:Port"] ?? "587");
            string? senderEmail = _configuration["EmailSettings:SenderEmail"];
            string? senderPassword = _configuration["EmailSettings:SenderPassword"];

            var smtpClient = new SmtpClient(smtpServer)
            {
                Port = smtpPort,
                Credentials = new NetworkCredential(senderEmail, senderPassword),
                EnableSsl = true,
            };

            var (subject, body) = GetEmailTemplate(status);

            var mailMessage = new MailMessage
            {
                From = new MailAddress(senderEmail ?? "tuananh83cbt@gmail.com"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(email);
            await smtpClient.SendMailAsync(mailMessage);
        }

        private (string Subject, string Body) GetEmailTemplate(string status)
        {
            return status.ToLower() switch
            {
                "inactive" => GetBannedTemplate(),
                "active" => GetUnbannedTemplate(),
                _ => throw new ArgumentException($"Unknown status: {status}")
            };
        }

        private (string Subject, string Body) GetBannedTemplate()
        {
            var subject = "Account Access Suspended - Action Required";

            var body = @"
    <!DOCTYPE html>
    <html>
    <head>
        <meta charset='utf-8'>
    </head>
    <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 0; background-color: #ffffff;'>
        <div style='background-color: #dc3545; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0;'>
            <h2 style='margin: 0; font-size: 24px; font-weight: bold;'>Account Access Suspended</h2>
        </div>
        
        <div style='padding: 30px 20px; background-color: #f8f9fa; border-left: 1px solid #dee2e6; border-right: 1px solid #dee2e6;'>
            <p style='margin: 0 0 20px 0; font-size: 16px;'>Dear User,</p>
            
            <div style='background-color: #fff3cd; border: 1px solid #ffeaa7; padding: 15px; border-radius: 5px; margin: 20px 0; border-left: 4px solid #f0ad4e;'>
                <strong style='color: #856404; font-size: 16px;'>⚠️ Your account has been temporarily suspended</strong>
            </div>
            
            <p style='margin: 20px 0; font-size: 15px; line-height: 1.8;'>We regret to inform you that your account access has been suspended due to a violation of our Terms of Service or Community Guidelines.</p>
            
            <h3 style='color: #dc3545; font-size: 18px; margin: 25px 0 15px 0; font-weight: bold;'>What this means:</h3>
            <ul style='margin: 15px 0; padding-left: 20px; font-size: 15px; line-height: 1.8;'>
                <li style='margin: 8px 0;'>You cannot access your account until further notice</li>
                <li style='margin: 8px 0;'>All services associated with your account are temporarily unavailable</li>
                <li style='margin: 8px 0;'>Your data remains secure and unchanged</li>
            </ul>
            
            <h3 style='color: #dc3545; font-size: 18px; margin: 25px 0 15px 0; font-weight: bold;'>Next Steps:</h3>
            <p style='margin: 15px 0; font-size: 15px; line-height: 1.8;'>If you believe this suspension was made in error or would like to appeal this decision, please contact our support team with your account details.</p>
            
            <p style='margin: 25px 0 15px 0; font-size: 15px; line-height: 1.8;'>We appreciate your understanding and look forward to resolving this matter promptly.</p>
            
            <p style='margin: 20px 0 0 0; font-size: 15px;'>Best regards,<br>
            <strong style='color: #dc3545;'>Support Team</strong></p>
        </div>
        
        <div style='padding: 20px; text-align: center; font-size: 12px; color: #666; background-color: #e9ecef; border-radius: 0 0 8px 8px; border: 1px solid #dee2e6; border-top: none;'>
            <p style='margin: 0 0 5px 0;'>This is an automated message. Please do not reply to this email.</p>
            <p style='margin: 5px 0 0 0;'>Contact us at: <a href='mailto:admin@example.com' style='color: #007bff; text-decoration: none;'>support@yourcompany.com</a></p>
        </div>
    </body>
    </html>";

            return (subject, body);
        }

        private (string Subject, string Body) GetUnbannedTemplate()
        {
            var subject = "Welcome Back - Your Account Has Been Restored";

            var body = @"
    <!DOCTYPE html>
    <html>
    <head>
        <meta charset='utf-8'>
    </head>
    <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 0; background-color: #ffffff;'>
        <div style='background-color: #28a745; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0;'>
            <h2 style='margin: 0; font-size: 24px; font-weight: bold;'>Welcome Back!</h2>
        </div>
        
        <div style='padding: 30px 20px; background-color: #f8f9fa; border-left: 1px solid #dee2e6; border-right: 1px solid #dee2e6;'>
            <p style='margin: 0 0 20px 0; font-size: 16px;'>Dear User,</p>
            
            <div style='background-color: #d4edda; border: 1px solid #c3e6cb; padding: 15px; border-radius: 5px; margin: 20px 0; border-left: 4px solid #28a745;'>
                <strong style='color: #155724; font-size: 16px;'>✅ Your account has been successfully restored</strong>
            </div>
            
            <p style='margin: 20px 0; font-size: 15px; line-height: 1.8;'>We're pleased to inform you that your account suspension has been lifted and you now have full access to all services.</p>
            
            <h3 style='color: #28a745; font-size: 18px; margin: 25px 0 15px 0; font-weight: bold;'>What's restored:</h3>
            <ul style='margin: 15px 0; padding-left: 20px; font-size: 15px; line-height: 1.8;'>
                <li style='margin: 8px 0;'>Full account access and functionality</li>
                <li style='margin: 8px 0;'>All previously available services</li>
                <li style='margin: 8px 0;'>Your account data and settings remain intact</li>
            </ul>
            
            <h3 style='color: #28a745; font-size: 18px; margin: 25px 0 15px 0; font-weight: bold;'>Moving Forward:</h3>
            <p style='margin: 15px 0; font-size: 15px; line-height: 1.8;'>To ensure continued access to your account, please:</p>
            <ul style='margin: 15px 0; padding-left: 20px; font-size: 15px; line-height: 1.8;'>
                <li style='margin: 8px 0;'>Review our Terms of Service and Community Guidelines</li>
                <li style='margin: 8px 0;'>Ensure all future activities comply with our policies</li>
                <li style='margin: 8px 0;'>Contact support if you have any questions</li>
            </ul>
            
            <div style='text-align: center; margin: 30px 0;'>
                <a href='#' style='display: inline-block; padding: 15px 30px; background-color: #007bff; color: white; text-decoration: none; border-radius: 5px; font-weight: bold; font-size: 16px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);'>Access Your Account</a>
            </div>
            
            <p style='margin: 25px 0 15px 0; font-size: 15px; line-height: 1.8;'>Thank you for your patience during the review process. We're glad to have you back!</p>
            
            <p style='margin: 20px 0 0 0; font-size: 15px;'>Best regards,<br>
            <strong style='color: #28a745;'>Support Team</strong></p>
        </div>
        
        <div style='padding: 20px; text-align: center; font-size: 12px; color: #666; background-color: #e9ecef; border-radius: 0 0 8px 8px; border: 1px solid #dee2e6; border-top: none;'>
            <p style='margin: 0 0 5px 0;'>This is an automated message. Please do not reply to this email.</p>
            <p style='margin: 5px 0 0 0;'>Contact us at: <a href='mailto:admin@example.com' style='color: #007bff; text-decoration: none;'>support@yourcompany.com</a> | Visit: <a href='https://www.yourcompany.com' style='color: #007bff; text-decoration: none;'>www.yourcompany.com</a></p>
        </div>
    </body>
    </html>";

            return (subject, body);
        }
    }
}
