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
    public class AccountPackageDAO
    {
        private static AccountPackageDAO instance = null;
        private readonly DataContext _context;

        private AccountPackageDAO()
        {
            _context = new DataContext();
        }

        public static AccountPackageDAO Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AccountPackageDAO();
                }
                return instance;
            }
        }

        public async Task<AccountPackage?> GetAccountPackageByIdAsync(int accountPackageId)
        {
            return await _context.AccountPackages.FirstOrDefaultAsync(a => a.AccountPackageID == accountPackageId);
        }

        public async Task<bool> UpdateAccountPackageAsync(AccountPackage accountPackage)
        {
            var existingAccountPackage = await GetAccountPackageByIdAsync(accountPackage.AccountPackageID);
            if (existingAccountPackage == null)
                return false;

            _context.Entry(existingAccountPackage).CurrentValues.SetValues(accountPackage);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> CreateAccountPackageAsync(AccountPackage accountPackage)
        {
            _context.Add(accountPackage);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
