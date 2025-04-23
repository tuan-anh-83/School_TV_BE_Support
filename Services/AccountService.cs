using BOs.Models;
using Microsoft.Extensions.Caching.Distributed;
using Repos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Services
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepo _accountRepo;
        private readonly IDistributedCache _cache;

        public AccountService(IAccountRepo accountRepo, IDistributedCache cache)
        {
            _accountRepo = accountRepo;
            _cache = cache;
        }

        public async Task<bool> AssignRoleAsync(int accountId, int roleId)
        {
            return await _accountRepo.AssignRoleAsync(accountId, roleId);
        }

        public async Task<bool> DeleteAccountAsync(int accountId)
        {
            return await _accountRepo.DeleteAccountAsync(accountId);
        }

        public async Task<Account?> GetAccountByEmailAsync(string email)
        {
            return await _accountRepo.GetAccountByEmailAsync(email);
        }

        public async Task<Account?> GetAccountByIdAsync(int accountId)
        {
            return await _accountRepo.GetAccountByIdAsync(accountId);
        }

        public async Task<Account?> GetAccountByUsernameAsync(string username)
        {
            return await _accountRepo.GetAccountByUsernameAsync(username);
        }

        public async Task<List<Account>> GetAllAccountsAsync()
        {
            return await _accountRepo.GetAllAccountsAsync();
        }

        public async Task<List<Account>> GetAllPendingSchoolOwnerAsync()
        {
            return await _accountRepo.GetAllPendingSchoolOwnerAsync();
        }

        public async Task<OtpInfo> GetCurrentOtpAsync(string email)
        {
            var otpJson = await _cache.GetStringAsync(GetOtpCacheKey(email));
            if (string.IsNullOrEmpty(otpJson))
                return null;
            return JsonSerializer.Deserialize<OtpInfo>(otpJson);
        }

        private string GetOtpCacheKey(string email)
        {
            return $"OTP_{email}";
        }

        public async Task<List<Account>> GetPendingAccountsAsync(DateTime threshold)
        {
            return await _accountRepo.GetPendingAccountsOlderThanAsync(threshold);
        }

        public async Task<Role?> GetRoleByIdAsync(int roleId)
        {
            return await _accountRepo.GetRoleByIdAsync(roleId);
        }

        public async Task<int> GetSchoolOwnerCountAsync()
        {
            return await _accountRepo.GetSchoolOwnerCountAsync();
        }

        public async Task<int> GetUserCountAsync()
        {
            return await _accountRepo.GetUserCountAsync();
        }

        public async Task InvalidateOtpAsync(string email)
        {
            await _cache.RemoveAsync(GetOtpCacheKey(email));

        }

        public Task InvalidatePasswordResetTokenAsync(int accountId, string token)
        {
            return _accountRepo.InvalidatePasswordResetTokenAsync(accountId, token);
        }

        public async Task<Account> Login(string email, string password)
        {
            return await _accountRepo.Login(email, password);
        }

        public async Task<bool> SaveOtpAsync(string email, string otp, DateTime expiration)
        {
            var otpInfo = new OtpInfo
            {
                Code = otp,
                Expiration = expiration
            };

            var otpJson = JsonSerializer.Serialize(otpInfo);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = expiration
            };

            try
            {
                await _cache.SetStringAsync(GetOtpCacheKey(email), otpJson, options);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public Task SavePasswordResetTokenAsync(int accountId, string token, DateTime expiration)
        {
            return _accountRepo.SavePasswordResetTokenAsync(accountId, token, expiration);
        }

        public async Task<Account?> SearchAccountByIdAsync(int accountId)
        {
            return await _accountRepo.SearchAccountByIdAsync(accountId);
        }

        public async Task<List<Account>> SearchAccountsByNameAsync(string searchTerm)
        {
            return await _accountRepo.SearchAccountsByNameAsync(searchTerm);
        }

        public async Task<bool> SignUpAsync(Account account)
        {
            return await _accountRepo.SignUpAsync(account);
        }

        public Task<bool> UpdateAccountAsync(Account account)
        {
            return _accountRepo.UpdateAccountAsync(account);
        }

        public async Task<bool> UpdateAccountStatusAsync(int accountId, string status)
        {
            return await _accountRepo.UpdateAccountStatusAsync(accountId, status);
        }

        public async Task<bool> VerifyOtpAsync(string email, string otp)
        {
            var otpJson = await _cache.GetStringAsync(GetOtpCacheKey(email));
            if (string.IsNullOrEmpty(otpJson))
                return false;
            var otpInfo = JsonSerializer.Deserialize<OtpInfo>(otpJson);
            return otpInfo != null && otpInfo.Code == otp;
        }

        public async Task<bool> VerifyPasswordResetTokenAsync(int accountId, string token)
        {
            return await _accountRepo.VerifyPasswordResetTokenAsync(accountId, token);
        }
    }
}
