using BOs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface IPackageService
    {
        Task<List<Package>> GetAllPackagesAsync();
        Task<Package?> GetPackageByIdAsync(int packageId);
        Task<bool> AddPackageAsync(Package package);
        Task<bool> UpdatePackageAsync(Package package);
        Task<bool> DeletePackageAsync(int packageId);
        Task<List<Package>> GetAllActivePackagesAsync();
        Task<List<Package>> SearchPackagesByNameAsync(string name);
        Task<List<object>> GetTopPurchasedPackagesAsync();
        Task<(Package?, double?)?> GetCurrentPackageAndDurationByAccountIdAsync(int accountId);
        Task<AccountPackage?> GetCurrentPackageAndDurationByProgramIdAsync(int programId);
    }
}
