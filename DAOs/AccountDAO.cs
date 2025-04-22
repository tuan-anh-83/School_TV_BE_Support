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
                                 .Include(a => a.Role)
                                 .FirstOrDefaultAsync(a => a.Username == username);
        }

        public async Task<Account?> GetAccountByEmailAsync(string email)
        {
            return await _context.Accounts
                                 .Include(a => a.Role)
                                 .FirstOrDefaultAsync(a => a.Email == email);
        }

        public async Task<Role?> GetRoleByIdAsync(int roleId)
        {
            return await _context.Roles.FirstOrDefaultAsync(r => r.RoleID == roleId);
        }

        public async Task<List<Account>> GetAllAccountsAsync()
        {
            return await _context.Accounts.Include(a => a.Role).ToListAsync();
        }

        public async Task<Account?> GetAccountByIdAsync(int accountId)
        {
            return await _context.Accounts.Include(a => a.Role)
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
            var existingAccount = await _context.Accounts
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
            var existingAccount = await GetAccountByIdAsync(account.AccountID);
            if (existingAccount == null || existingAccount.RoleID == 0)
                return false;

            if (!string.IsNullOrEmpty(account.Password) && !account.Password.StartsWith("$2"))
            {
                account.Password = BCrypt.Net.BCrypt.HashPassword(account.Password);
            }
            else
            {
                account.Password = existingAccount.Password;
            }

            _context.Entry(existingAccount).CurrentValues.SetValues(account);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<Account?> Login(string email, string password)
        {
            var account = await _context.Accounts
                                        .Include(a => a.Role)
                                        .FirstOrDefaultAsync(a => a.Email == email && a.RoleID != 0);
            if (account == null || !BCrypt.Net.BCrypt.Verify(password, account.Password) || !account.Status.Equals("Active", StringComparison.OrdinalIgnoreCase))
                return null;
            return account;
        }

        public async Task<Account?> SearchAccountByIdAsync(int accountId)
        {
            return await _context.Accounts.Include(a => a.Role)
                                          .FirstOrDefaultAsync(a => a.AccountID == accountId);
        }

        public async Task<List<Account>> SearchAccountsByNameAsync(string searchTerm)
        {
            return await _context.Accounts
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

        public async Task<bool> UpdateAccountStatusAsync(int accountId, string status)
        {
            var allowedStatuses = new[] { "Active", "InActive", "Pending", "Reject" };
            if (!allowedStatuses.Contains(status, StringComparer.OrdinalIgnoreCase))
                return false;

            var account = await GetAccountByIdAsync(accountId);
            if (account == null)
                return false;

            account.Status = status;
            account.UpdatedAt = DateTime.UtcNow;
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
            var resetToken = await _context.PasswordResetTokens
                .FirstOrDefaultAsync(t => t.AccountID == accountId && t.Token == token);

            return resetToken != null && resetToken.Expiration >= DateTime.UtcNow;
        }

        public async Task InvalidatePasswordResetTokenAsync(int accountId, string token)
        {
            var resetToken = await _context.PasswordResetTokens
                .FirstOrDefaultAsync(t => t.AccountID == accountId && t.Token == token);

            if (resetToken != null)
            {
                _context.PasswordResetTokens.Remove(resetToken);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<List<Account>> GetAllPendingSchoolOwnerAsync()
        {
            return await _context.Accounts
                .Include(a => a.Role)
                .Where(a => a.RoleID == 2 && a.Status.ToLower() == "pending") 
                .ToListAsync();
        }

        public async Task<int> GetUserCountAsync()
        {
            return await _context.Accounts.CountAsync(a =>
                a.RoleID == 1 &&
                a.Status.ToLower() == "active");
        }
        public async Task<int> GetSchoolOwnerCountAsync()
        {
            return await _context.Accounts.CountAsync(a =>
                a.RoleID == 2 &&
                a.Status.ToLower() == "active");
        }
        public async Task<List<Account>> GetPendingAccountsOlderThanAsync(DateTime threshold)
        {
            return await _context.Accounts
                .Where(a => a.Status == "Pending" && a.CreatedAt < threshold)
                .ToListAsync();
        }

    }
}
