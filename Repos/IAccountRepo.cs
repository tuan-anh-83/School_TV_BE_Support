using BOs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repos
{
    public interface IAccountRepo
    {
        Task<Account?> GetAccountByUsernameAsync(string username);
        Task<Account?> GetAccountByEmailAsync(string email);
        Task<Role?> GetRoleByIdAsync(int roleId);
        Task<List<Account>> GetAllAccountsAsync();
        Task<Account?> GetAccountByIdAsync(int accountId);
        Task<bool> DeleteAccountAsync(int accountId);
        Task<bool> SignUpAsync(Account account);
        Task<bool> UpdateAccountAsync(Account account);
        Task<Account?> Login(string email, string password);
        Task<Account?> SearchAccountByIdAsync(int accountId);
        Task<List<Account>> SearchAccountsByNameAsync(string searchTerm);
        Task<bool> AssignRoleAsync(int accountId, int roleId);
        Task<bool> UpdateAccountStatusAsync(int accountId, string status);
        Task SavePasswordResetTokenAsync(int accountId, string token, DateTime expiration);
        Task<bool> VerifyPasswordResetTokenAsync(int accountId, string token);
        Task InvalidatePasswordResetTokenAsync(int accountId, string token);
        Task<List<Account>> GetAllPendingSchoolOwnerAsync();
        Task<int> GetUserCountAsync();
        Task<int> GetSchoolOwnerCountAsync();
        Task<List<Account>> GetPendingAccountsOlderThanAsync(DateTime threshold);
    }
}
