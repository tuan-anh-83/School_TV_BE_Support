using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BOs.Models
{
    public class PasswordResetToken
    {
        public int PasswordResetTokenID { get; set; }
        public int AccountID { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public virtual Account Account { get; set; }
    }
}
