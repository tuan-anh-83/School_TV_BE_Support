using BOs.Models;
using Microsoft.Extensions.Caching.Distributed;
using Repos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class AccountPackageService : IAccountPackageService
    {
        private readonly IAccountPackageRepo _accountPackageRepo;
        private readonly IDistributedCache _cache;

        public AccountPackageService(IAccountPackageRepo accountPackageRepo, IDistributedCache cache)
        {
            _accountPackageRepo = accountPackageRepo;
            _cache = cache;
        }

        public async Task<bool> CreateAccountPackageAsync(AccountPackage accountPackage)
        {
            return await _accountPackageRepo.CreateAccountPackageAsync(accountPackage); 
        }

        public async Task<AccountPackage?> GetActiveAccountPackageAsync(int accountID)
        {
            return await _accountPackageRepo.GetActiveAccountPackageAsync(accountID);
        }

        public async Task<bool> UpdateAccountPackageAsync(AccountPackage accountPackage)
        {
            return await _accountPackageRepo.UpdateAccountPackageAsync(accountPackage);
        }
    }
}
