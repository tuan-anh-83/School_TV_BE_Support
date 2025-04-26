using BOs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repos
{
    public interface IAccountPackageRepo
    {
        Task<bool> UpdateAccountPackageAsync(AccountPackage accountPackage);
        Task<bool> CreateAccountPackageAsync(AccountPackage accountPackage);
        Task<AccountPackage?> GetActiveAccountPackageAsync(int accountId);
    }
}
