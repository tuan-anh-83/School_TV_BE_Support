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
    public class PackageRepo : IPackageRepo
    {
        public async Task<bool> AddPackageAsync(Package package)
        {
            return await PackageDAO.Instance.AddPackageAsync(package);
        }

        public async Task<bool> DeletePackageAsync(int packageId)
        {
            return await PackageDAO.Instance.DeletePackageAsync(packageId);
        }


        public async Task<List<Package>> GetAllActivePackagesAsync()
        {
            return await PackageDAO.Instance.GetAllActivePackagesAsync();
        }

        public Task<List<Package>> GetAllPackagesAsync()
        {
            return PackageDAO.Instance.GetAllPackagesAsync();
        }

        public async Task<(Package?, double?, DateTime?)?> GetCurrentPackageAndDurationByAccountIdAsync(int accountId)
        {
            return await PackageDAO.Instance.GetCurrentPackageAndDurationByAccountIdAsync(accountId);   
        }

        public async Task<AccountPackage?> GetCurrentPackageAndDurationByProgramIdAsync(int programId)
        {
            return await PackageDAO.Instance.GetCurrentPackageAndDurationByProgramIdAsync(programId);
        }

        public async Task<Package?> GetPackageByIdAsync(int packageId)
        {
            return await PackageDAO.Instance.GetPackageByIdAsync(packageId);
        }

        public async Task<List<object>> GetTopPurchasedPackagesAsync()
        {
            return await PackageDAO.Instance.GetTopPurchasedPackagesAsync();
        }

        public async Task<List<Package>> SearchPackagesByNameAsync(string name)
        {
            return await PackageDAO.Instance.SearchPackagesByNameAsync(name);
        }

        public async Task<bool> UpdatePackageAsync(Package package)
        {
            return await PackageDAO.Instance.UpdatePackageAsync(package);
        }
    }
}
