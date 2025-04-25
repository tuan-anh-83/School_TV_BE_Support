using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BOs.Models
{
    public class Account
    {
        public int AccountID { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Fullname { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public int RoleID { get; set; }
        public Role Role { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? ExternalProvider { get; set; }
        public string? ExternalProviderKey { get; set; }
        public ICollection<PasswordResetToken> PasswordResetTokens { get; set; } = new List<PasswordResetToken>();
        public ICollection<SchoolChannelFollow> Follows { get; set; }
        public ICollection<ProgramFollow> ProgramFollows { get; set; }
        public ICollection<AccountPackage> AccountPackages { get; set; }
    }
}
