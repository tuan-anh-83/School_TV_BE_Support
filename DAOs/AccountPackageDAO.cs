﻿using BOs.Data;
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
            return await _context.AccountPackages.AsNoTracking().FirstOrDefaultAsync(a => a.AccountPackageID == accountPackageId);
        }

        public async Task<AccountPackage?> GetAccountPackageByAccountIdAsync(int accountId)
        {
            return await _context.AccountPackages.AsNoTracking().FirstOrDefaultAsync(a => a.AccountID == accountId);
        }

        public async Task<bool> UpdateAccountPackageAsync(AccountPackage accountPackage)
        {
            var tracked = _context.ChangeTracker.Entries<AccountPackage>()
              .FirstOrDefault(e => e.Entity.AccountPackageID == accountPackage.AccountPackageID);

            if (tracked != null)
            {
                _context.Entry(tracked.Entity).State = EntityState.Detached;
            }

            _context.AccountPackages.Update(accountPackage);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> CreateAccountPackageAsync(AccountPackage accountPackage)
        {
            _context.Add(accountPackage);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
