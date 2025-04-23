using BOs.Models;
using DAOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repos
{
    public class AccountRepo : IAccountRepo
    {
        public async Task<bool> AssignRoleAsync(int accountId, int roleId)
        {
            return await AccountDAO.Instance.AssignRoleAsync(accountId, roleId);    
        }

        public async Task<bool> DeleteAccountAsync(int accountId)
        {
            return await AccountDAO.Instance.DeleteAccountAsync(accountId);
        }

        public async Task<Account?> GetAccountByEmailAsync(string email)
        {
            return await AccountDAO.Instance.GetAccountByEmailAsync(email);
        }

        public async Task<Account?> GetAccountByIdAsync(int accountId)
        {
            return await AccountDAO.Instance.GetAccountByIdAsync(accountId);
        }

        public async Task<Account?> GetAccountByUsernameAsync(string username)
        {
            return await AccountDAO.Instance.GetAccountByUsernameAsync(username);
        }

        public async Task<List<Account>> GetAllAccountsAsync()
        {
            return await AccountDAO.Instance.GetAllAccountsAsync();
        }

        public async Task<List<Account>> GetAllPendingSchoolOwnerAsync()
        {
            return await AccountDAO.Instance.GetAllPendingSchoolOwnerAsync();
        }

        public async Task<List<Account>> GetPendingAccountsOlderThanAsync(DateTime threshold)
        {
            return await AccountDAO.Instance.GetPendingAccountsOlderThanAsync(threshold);
        }

        public async Task<Role?> GetRoleByIdAsync(int roleId)
        {
            return await AccountDAO.Instance.GetRoleByIdAsync(roleId);
        }

        public async Task<int> GetSchoolOwnerCountAsync()
        {
            return await AccountDAO.Instance.GetSchoolOwnerCountAsync();
        }

        public async Task<int> GetUserCountAsync()
        {
            return await AccountDAO.Instance.GetUserCountAsync();
        }

        public Task InvalidatePasswordResetTokenAsync(int accountId, string token)
        {
            return AccountDAO.Instance.InvalidatePasswordResetTokenAsync(accountId, token);
        }

        public async Task<Account> Login(string email, string password)
        {
            return await AccountDAO.Instance.Login(email, password);
        }

        public Task SavePasswordResetTokenAsync(int accountId, string token, DateTime expiration)
        {
            return AccountDAO.Instance.SavePasswordResetTokenAsync(accountId, token, expiration);
        }

        public async Task<Account?> SearchAccountByIdAsync(int accountId)
        {
            return await AccountDAO.Instance.SearchAccountByIdAsync(accountId);
        }

        public async Task<List<Account>> SearchAccountsByNameAsync(string searchTerm)
        {
            return await AccountDAO.Instance.SearchAccountsByNameAsync(searchTerm);
        }

        public async Task<bool> SignUpAsync(Account account)
        {
            return await AccountDAO.Instance.SignUpAsync(account);
        }

        public async Task<bool> UpdateAccountAsync(Account account)
        {
            return await AccountDAO.Instance.UpdateAccountAsync(account);
        }

        public async Task<bool> UpdateAccountStatusAsync(int accountId, string status)
        {
            return await AccountDAO.Instance.UpdateAccountStatusAsync(accountId, status);
        }

        public async Task<bool> VerifyPasswordResetTokenAsync(int accountId, string token)
        {
            return await AccountDAO.Instance.VerifyPasswordResetTokenAsync(accountId,token);
        }
    }
}
