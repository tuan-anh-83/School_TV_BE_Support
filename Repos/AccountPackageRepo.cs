using BOs.Models;
using DAOs;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repos
{
    public class AccountPackageRepo : IAccountPackageRepo
    {
        public async Task<bool> UpdateAccountPackageAsync(AccountPackage accountPackage)
        {
            return await AccountPackageDAO.Instance.UpdateAccountPackageAsync(accountPackage);
        }

        public async Task<bool> CreateAccountPackageAsync(AccountPackage accountPackage)
        {
            return await AccountPackageDAO.Instance.CreateAccountPackageAsync(accountPackage);
        }

        public async Task<AccountPackage?> GetActiveAccountPackageAsync(int accountId)
        {
            return await AccountPackageDAO.Instance.GetAccountPackageByAccountIdAsync(accountId);
        }
    }
}
