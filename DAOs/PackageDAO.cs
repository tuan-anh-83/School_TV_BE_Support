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
    public class PackageDAO
    {
        private static PackageDAO instance = null;
        private readonly DataContext _context;
        TimeZoneInfo vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

        private PackageDAO()
        {
            _context = new DataContext();
        }

        public static PackageDAO Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new PackageDAO();
                }
                return instance;
            }
        }

        public async Task<List<Package>> GetAllPackagesAsync()
        {
            return await _context.Packages.AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<Package>> GetAllActivePackagesAsync()
        {
            return await _context.Packages.Where(p => p.Status == "Active").ToListAsync();
        }

        public async Task<Package?> GetPackageByIdAsync(int packageId)
        {
            return await _context.Packages.AsNoTracking()
         .FirstOrDefaultAsync(p => p.PackageID == packageId);
        }
        public async Task<List<Package>> SearchPackagesByNameAsync(string name)
        {
            return await _context.Packages.AsNoTracking()
        .Where(p => EF.Functions.Like(p.Name, $"%{name}%"))
                .ToListAsync();
        }

        public async Task<bool> AddPackageAsync(Package package)
        {
            package.Status = "Active";
            package.CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);
            package.UpdatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);
            await _context.Packages.AddAsync(package);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdatePackageAsync(Package package)
        {
            var tracked = _context.ChangeTracker.Entries<Package>()
                .FirstOrDefault(e => e.Entity.PackageID == package.PackageID);

            if (tracked != null)
            {
                tracked.State = EntityState.Detached;
            }

            _context.Packages.Attach(package);
            _context.Entry(package).State = EntityState.Modified;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeletePackageAsync(int packageId)
        {
            var package = await GetPackageByIdAsync(packageId);
            if (package == null) return false;

            // Đơn giản hơn: Làm sạch ChangeTracker trước khi attach
            _context.ChangeTracker.Clear();

            // Sau đó attach và thực hiện cập nhật
            package.Status = "Inactive";
            package.UpdatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);

            _context.Packages.Attach(package);
            _context.Entry(package).Property(p => p.Status).IsModified = true;
            _context.Entry(package).Property(p => p.UpdatedAt).IsModified = true;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<object>> GetTopPurchasedPackagesAsync()
        {
            var result = await _context.OrderDetails.AsNoTracking()
               .GroupBy(od => od.PackageID)
                .Select(g => new
                {
                    PackageID = g.Key,
                    PurchaseCount = g.Sum(od => od.Quantity)
                })
                .OrderByDescending(x => x.PurchaseCount)
                .ToListAsync();

            var packageDetails = await _context.Packages
                .Where(p => result.Select(r => r.PackageID).Contains(p.PackageID))
                .ToListAsync();

            var rankedPackages = result.Select(r => new
            {
                PackageID = r.PackageID,
                PackageName = packageDetails.FirstOrDefault(p => p.PackageID == r.PackageID)?.Name ?? "Unknown",
                PurchaseCount = r.PurchaseCount
            }).ToList();

            return rankedPackages.Cast<object>().ToList();
        }

        public async Task<(Package?, double?, DateTime?)?> GetCurrentPackageAndDurationByAccountIdAsync(int accountId)
        {
            Console.WriteLine($"[DEBUG] Start fetching current package for AccountID: {accountId}");

            var latestPaidOrder = await _context.Orders.AsNoTracking()
          .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Package)
                .Where(o => o.AccountID == accountId && o.Status == "Completed")
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();

            if (latestPaidOrder == null)
            {
                Console.WriteLine("[DEBUG] No completed order found for account.");
                return null;
            }

            Console.WriteLine($"[DEBUG] Found completed order: OrderID = {latestPaidOrder.OrderID}, CreatedAt = {latestPaidOrder.CreatedAt}");

            var packageDetail = latestPaidOrder.OrderDetails.FirstOrDefault();
            if (packageDetail == null)
            {
                Console.WriteLine("[DEBUG] Order found, but no order detail available.");
                return null;
            }

            Console.WriteLine($"[DEBUG] Found OrderDetail: PackageID = {packageDetail.PackageID}");

            if (packageDetail.Package == null)
            {
                Console.WriteLine("[DEBUG] Package in OrderDetail is null.");
                return null;
            }

            Console.WriteLine($"[DEBUG] Package loaded: Name = {packageDetail.Package.Name}, Duration = {packageDetail.Package.Duration}");

            var accountPackage = await _context.AccountPackages
.AsNoTracking()
    .FirstOrDefaultAsync(ap => ap.AccountID == accountId);

            if (accountPackage == null)
            {
                Console.WriteLine("[DEBUG] No account package found for account.");
                return null;
            }

            Console.WriteLine($"[DEBUG] AccountPackage found: RemainingMinutes = {accountPackage.RemainingMinutes}");

            return (packageDetail.Package, accountPackage.RemainingMinutes, accountPackage.ExpiredAt);
        }

        public async Task<AccountPackage?> GetCurrentPackageAndDurationByProgramIdAsync(int programId)
        {
            Console.WriteLine($"[DEBUG] Start fetching current package for ProgramID: {programId}");
            var schoolChannel = await _context.Programs
.AsNoTracking()
    .Include(p => p.SchoolChannel)
                .Where(p => p.ProgramID == programId)
                .Select(p => p.SchoolChannel)
                .FirstOrDefaultAsync();

            if (schoolChannel == null)
            {
                Console.WriteLine("[DEBUG] No school channel found for program.");
                return null;
            }
            var accountPackage = await _context.AccountPackages
.AsNoTracking()
    .Where(ap => ap.AccountID == schoolChannel.AccountID)
                .FirstOrDefaultAsync();

            return accountPackage;
        }
    }
}
