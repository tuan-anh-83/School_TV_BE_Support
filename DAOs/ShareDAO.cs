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
    public class ShareDAO
    {
        private static ShareDAO instance = null;
        private readonly DataContext _context;

        private ShareDAO()
        {
            _context = new DataContext();
        }

        public static ShareDAO Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ShareDAO();
                }
                return instance;
            }
        }

        public async Task<List<Share>> GetAllSharesAsync()
        {
            return await _context.Shares
                .Include(s => s.VideoHistory)
                .ToListAsync();
        }

        public async Task<List<Share>> GetAllActiveSharesAsync()
        {
            return await _context.Shares
                .Include(s => s.VideoHistory)
                .Where(s => s.Quantity > 0)
                .ToListAsync();
        }

        public async Task<Share?> GetShareByIdAsync(int shareId)
        {
            return await _context.Shares
                .Include(s => s.VideoHistory)
                .FirstOrDefaultAsync(s => s.ShareID == shareId);
        }

        public async Task<bool> AddShareAsync(Share share)
        {

            bool vhExists = await _context.VideoHistories.AnyAsync(v => v.VideoHistoryID == share.VideoHistoryID);
            if (!vhExists) return false;

            bool accountExists = await _context.Accounts.AnyAsync(a => a.AccountID == share.AccountID);
            if (!accountExists) return false;

            share.Quantity = 1;
            await _context.Shares.AddAsync(share);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateShareAsync(Share share)
        {
            var existingShare = await GetShareByIdAsync(share.ShareID);
            if (existingShare == null) return false;

            _context.Entry(existingShare).CurrentValues.SetValues(share);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteShareAsync(int shareId)
        {
            var share = await GetShareByIdAsync(shareId);
            if (share == null) return false;

            share.Quantity = 0;
            _context.Shares.Update(share);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetTotalSharesForVideoAsync(int videoHistoryId)
        {
            return await _context.Shares
                .Where(s => s.VideoHistoryID == videoHistoryId && s.Quantity > 0)
                .SumAsync(s => s.Quantity);
        }

        public async Task<int> GetTotalSharesAsync()
        {
            return await _context.Shares
                 .Where(s => s.Quantity > 0)
                 .SumAsync(s => s.Quantity);
        }

        public async Task<Dictionary<int, int>> GetSharesPerVideoAsync()
        {
            return await _context.Shares
                .Where(s => s.Quantity > 0)
                .GroupBy(s => s.VideoHistoryID)
                .Select(g => new
                {
                    VideoHistoryID = g.Key,
                    TotalShares = g.Sum(s => s.Quantity)
                })
                .ToDictionaryAsync(x => x.VideoHistoryID, x => x.TotalShares);
        }
    }
}
