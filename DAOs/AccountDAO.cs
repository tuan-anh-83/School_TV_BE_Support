using BCrypt.Net;
using BOs.Data;
using BOs.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAOs
{
    public class AccountDAO
    {
        private static AccountDAO instance = null;
        private readonly DataContext _context;

        private AccountDAO()
        {
            _context = new DataContext();
        }

        public static AccountDAO Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AccountDAO();
                }
                return instance;
            }
        }

        
        public async Task<Account?> GetAccountByUsernameAsync(string username)
        {
            return await _context.Accounts
                                 .AsNoTracking()
                                 .Include(a => a.Role)
                                 .FirstOrDefaultAsync(a => a.Username == username);
        }

        public async Task<Account?> GetAccountByEmailAsync(string email)
        {
            return await _context.Accounts
                                 .AsNoTracking()
                                 .Include(a => a.Role)
                                 .FirstOrDefaultAsync(a => a.Email == email);
        }

        public async Task<Role?> GetRoleByIdAsync(int roleId)
        {
            return await _context.Roles.AsNoTracking().FirstOrDefaultAsync(r => r.RoleID == roleId);
        }

        public async Task<List<Account>> GetAllAccountsAsync()
        {
            return await _context.Accounts.AsNoTracking().Include(a => a.Role).ToListAsync();
        }

        public async Task<Account?> GetAccountByIdAsync(int accountId)
        {
            return await _context.Accounts.Include(a => a.Role).AsNoTracking()
                                         .FirstOrDefaultAsync(a => a.AccountID == accountId);
        }

        public async Task<bool> DeleteAccountAsync(int accountId)
        {
            var account = await GetAccountByIdAsync(accountId);
            if (account == null)
                return false;
            account.Status = "InActive";
            _context.Accounts.Update(account);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> SignUpAsync(Account account)
        {
            var existingAccount = await _context.Accounts.AsNoTracking()
                .FirstOrDefaultAsync(a => a.Username == account.Username || a.Email == account.Email);

            if (existingAccount != null)
            {
                if (existingAccount.Status.Equals("Reject", StringComparison.OrdinalIgnoreCase))
                {
                    existingAccount.Fullname = account.Fullname;
                    existingAccount.Address = account.Address;
                    existingAccount.PhoneNumber = account.PhoneNumber;
                    existingAccount.Password = BCrypt.Net.BCrypt.HashPassword(account.Password);
                    existingAccount.Status = "Pending";
                    existingAccount.UpdatedAt = DateTime.UtcNow;

                    _context.Accounts.Update(existingAccount);
                    return await _context.SaveChangesAsync() > 0;
                }
                return false;
            }

            account.RoleID = account.RoleID == 0 ? 1 : account.RoleID;
            if (string.IsNullOrWhiteSpace(account.Status))
            {
                account.Status = (account.RoleID == 1) ? "Active" : "Pending";
            }
            account.Password = BCrypt.Net.BCrypt.HashPassword(account.Password);

            await _context.Accounts.AddAsync(account);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateAccountAsync(Account account)
        {
            var existingAccount = await _context.Accounts.FirstOrDefaultAsync(a => a.AccountID == account.AccountID);

            if (existingAccount == null || existingAccount.RoleID == 0)
                return false;

            // Cập nhật từng trường cần thiết
            if (!string.IsNullOrEmpty(account.Password) && !account.Password.StartsWith("$2"))
            {
                existingAccount.Password = BCrypt.Net.BCrypt.HashPassword(account.Password);
            }

            existingAccount.Status = account.Status;
            existingAccount.UpdatedAt = account.UpdatedAt;

            // Thêm log debug nếu cần
            // Console.WriteLine($"Cập nhật status từ {existingAccount.Status} → {account.Status}");

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<Account?> Login(string email, string password)
        {
            var account = await _context.Accounts
                                        .AsNoTracking()
                                        .Include(a => a.Role)
                                        .FirstOrDefaultAsync(a => a.Email == email && a.RoleID != 0);
            if (account == null || !BCrypt.Net.BCrypt.Verify(password, account.Password) || !account.Status.Equals("Active", StringComparison.OrdinalIgnoreCase))
                return null;
            return account;
        }

        public async Task<Account?> SearchAccountByIdAsync(int accountId)
        {
            return await _context.Accounts.Include(a => a.Role).AsNoTracking()
                                   .FirstOrDefaultAsync(a => a.AccountID == accountId);
        }

        public async Task<List<Account>> SearchAccountsByNameAsync(string searchTerm)
        {
            return await _context.Accounts.AsNoTracking()
           .Include(a => a.Role)
                .Where(a => EF.Functions.Like(a.Fullname, $"%{searchTerm}%"))
                .ToListAsync();
        }

        public async Task<bool> AssignRoleAsync(int accountId, int roleId)
        {
            var account = await GetAccountByIdAsync(accountId);
            if (account == null || account.RoleID == 0)
                return false;

            account.RoleID = roleId;
            return await UpdateAccountAsync(account);
        }

        public async Task<bool> UpdateAccountStatusAsync(Account account, string status)
        {
            // Danh sách các trạng thái được phép
            var allowedStatuses = new[] { "Active", "InActive", "Pending", "Reject" };

            // Kiểm tra trạng thái hợp lệ
            if (!allowedStatuses.Contains(status, StringComparer.OrdinalIgnoreCase))
                return false;

            // Kiểm tra nếu trạng thái mới giống trạng thái hiện tại
            if (account.Status.Equals(status, StringComparison.OrdinalIgnoreCase))
                return true; // Không cần update, xem như thành công

            // Cập nhật trạng thái và thời gian cập nhật
            account.Status = status;
            account.UpdatedAt = DateTime.UtcNow;

            // Gọi hàm cập nhật thực tế
            return await UpdateAccountAsync(account);
        }

        public async Task SavePasswordResetTokenAsync(int accountId, string token, DateTime expiration)
        {
            var resetToken = new PasswordResetToken
            {
                AccountID = accountId,
                Token = token,
                Expiration = expiration,
                CreatedAt = DateTime.UtcNow
            };

            _context.PasswordResetTokens.Add(resetToken);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> VerifyPasswordResetTokenAsync(int accountId, string token)
        {
            var resetToken = await _context.PasswordResetTokens.AsNoTracking()
        .FirstOrDefaultAsync(t => t.AccountID == accountId && t.Token == token);

            return resetToken != null && resetToken.Expiration >= DateTime.UtcNow;
        }

        public async Task InvalidatePasswordResetTokenAsync(int accountId, string token)
        {
            var resetToken = await _context.PasswordResetTokens.AsNoTracking()
            .FirstOrDefaultAsync(t => t.AccountID == accountId && t.Token == token);

            if (resetToken != null)
            {
                _context.PasswordResetTokens.Remove(resetToken);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<List<Account>> GetAllPendingSchoolOwnerAsync()
        {
            return await _context.Accounts.AsNoTracking()
            .Include(a => a.Role)
                .Where(a => a.RoleID == 2 && a.Status.ToLower() == "pending") 
                .ToListAsync();
        }

        public async Task<List<Account>> GetAllPendingAdvertiserAsync()
        {
            return await _context.Accounts.AsNoTracking()
            .Include(a => a.Role)
                .Where(a => a.RoleID == 4 && a.Status.ToLower() == "pending")
                .ToListAsync();
        }

        public async Task<int> GetUserCountAsync()
        {
            return await _context.Accounts.AsNoTracking().CountAsync(a =>
                a.RoleID == 1 &&
                a.Status.ToLower() == "active");
        }
        public async Task<int> GetSchoolOwnerCountAsync()
        {
            return await _context.Accounts.AsNoTracking().CountAsync(a =>
                a.RoleID == 2 &&
                a.Status.ToLower() == "active");
        }
        public async Task<List<Account>> GetPendingAccountsOlderThanAsync(DateTime threshold)
        {
            return await _context.Accounts.AsNoTracking()
                .Where(a => a.Status == "Pending" && a.CreatedAt < threshold)
                .ToListAsync();
        }

    }
}
