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
    public class ProgramFollowDAO
    {
        private static ProgramFollowDAO instance = null;
        private readonly DataContext _context;

        private ProgramFollowDAO()
        {
            _context = new DataContext();
        }

        public static ProgramFollowDAO Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ProgramFollowDAO();
                }
                return instance;
            }
        }

        public async Task<List<ProgramFollow>> GetAllAsync()
        {
            return await _context.ProgramFollows.ToListAsync();
        }

        public async Task<ProgramFollow?> GetByIdAsync(int id)
        {
            return await _context.ProgramFollows.FindAsync(id);
        }

        public async Task<List<ProgramFollow>> GetByAccountIdAsync(int accountId)
        {
            return await _context.ProgramFollows
                .Where(f => f.AccountID == accountId)
                .ToListAsync();
        }

        public async Task<bool> AddAsync(ProgramFollow programFollow)
        {
            _context.ProgramFollows.Add(programFollow);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateAsync(ProgramFollow programFollow)
        {
            _context.ProgramFollows.Update(programFollow);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var follow = await _context.ProgramFollows.FindAsync(id);
            if (follow == null) return false;

            _context.ProgramFollows.Remove(follow);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<int> CountByProgramAsync(int programId)
        {
            return await _context.ProgramFollows.CountAsync(f => f.ProgramID == programId);
        }

        public async Task<ProgramFollow> GetByAccountAndProgramAsync(int accountId, int programId)
        {
            return await _context.ProgramFollows
                .FirstOrDefaultAsync(f => f.AccountID == accountId && f.ProgramID == programId);
        }

        public async Task<ProgramFollow> CreateProgramFollowAsync(ProgramFollow follow)
        {
            _context.ProgramFollows.Add(follow);
            await _context.SaveChangesAsync();
            return follow;
        }

        public async Task<ProgramFollow> UpdateProgramFollowAsync(ProgramFollow follow)
        {
            _context.ProgramFollows.Update(follow);
            await _context.SaveChangesAsync();
            return follow;
        }

        public async Task<List<ProgramFollow>> GetFollowersByProgramIdAsync(int programId)
        {
            return await _context.ProgramFollows
                .Where(f => f.ProgramID == programId)
                .ToListAsync();
        }
        public async Task<List<ProgramFollow>> GetByProgramIdAsync(int programId)
        {
            return await _context.ProgramFollows
                .Where(f => f.ProgramID == programId)
                .ToListAsync();
        }
    }
}
