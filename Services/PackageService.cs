using BOs.Models;
using Repos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class PackageService : IPackageService
    {
        private readonly IPackageRepo _packageRepo;

        public PackageService(IPackageRepo packageRepo)
        {
            _packageRepo = packageRepo;
        }

        public async Task<List<Package>> GetAllPackagesAsync()
        {
            return await _packageRepo.GetAllPackagesAsync();
        }

        public async Task<List<Package>> GetAllActivePackagesAsync()
        {
            return await _packageRepo.GetAllActivePackagesAsync();
        }

        public async Task<Package?> GetPackageByIdAsync(int packageId)
        {
            return await _packageRepo.GetPackageByIdAsync(packageId);
        }
        public async Task<List<Package>> SearchPackagesByNameAsync(string name)
        {
            return await _packageRepo.SearchPackagesByNameAsync(name);
        }
        public async Task<bool> AddPackageAsync(Package package)
        {
            return await _packageRepo.AddPackageAsync(package);
        }

        public async Task<bool> UpdatePackageAsync(Package package)
        {
            return await _packageRepo.UpdatePackageAsync(package);
        }

        public async Task<bool> DeletePackageAsync(int packageId)
        {
            return await _packageRepo.DeletePackageAsync(packageId);
        }
        public async Task<List<object>> GetTopPurchasedPackagesAsync()
        {
            return await _packageRepo.GetTopPurchasedPackagesAsync();
        }

        public async Task<(Package?, double?)?> GetCurrentPackageAndDurationByAccountIdAsync(int accountId)
        {
            return await _packageRepo.GetCurrentPackageAndDurationByAccountIdAsync(accountId);
        }

        public async Task<AccountPackage?> GetCurrentPackageAndDurationByProgramIdAsync(int programId)
        {
            return await _packageRepo.GetCurrentPackageAndDurationByProgramIdAsync(programId);
        }
    }
}
