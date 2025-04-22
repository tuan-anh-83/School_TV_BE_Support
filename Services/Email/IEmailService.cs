using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Email
{
    public interface IEmailService
    {
        Task SendPasswordResetEmailAsync(string email, string resetLink);
        Task SendOtpEmailAsync(string email, string otp);
        Task SendOtpReminderEmailAsync(string email);
        Task SendStreamKeyEmailAsync(string email, string rtmpUrl, DateTime startTime, DateTime endTime, string schoolName);

    }
}
