using BOs.Models;
using DAOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repos
{
    public class ProgramFollowRepo : IProgramFollowRepo
    {
        public async Task<bool> AddAsync(ProgramFollow programFollow)
        {
            return await ProgramFollowDAO.Instance.AddAsync(programFollow); 
        }

        public async Task<int> CountByProgramAsync(int programId)
        {
            return await ProgramFollowDAO.Instance.CountByProgramAsync(programId);
        }

        public async Task<ProgramFollow> CreateProgramFollowAsync(ProgramFollow follow)
        {
            return await ProgramFollowDAO.Instance.CreateProgramFollowAsync(follow);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await ProgramFollowDAO.Instance.DeleteAsync(id);
        }

        public async Task<List<ProgramFollow>> GetAllAsync()
        {
            return await ProgramFollowDAO.Instance.GetAllAsync();
        }

        public async Task<ProgramFollow> GetByAccountAndProgramAsync(int accountId, int programId)
        {
            return await ProgramFollowDAO.Instance.GetByAccountAndProgramAsync(accountId, programId);   
        }

        public async Task<List<ProgramFollow>> GetByAccountIdAsync(int accountId)
        {
            return await ProgramFollowDAO.Instance.GetByAccountIdAsync(accountId);
        }

        public async Task<ProgramFollow?> GetByIdAsync(int id)
        {
            return await ProgramFollowDAO.Instance.GetByIdAsync(id);
        }

        public async Task<List<ProgramFollow>> GetByProgramIdAsync(int programId)
        {
            return await ProgramFollowDAO.Instance.GetByProgramIdAsync(programId);
        }

        public async Task<List<ProgramFollow>> GetFollowersByProgramIdAsync(int programId)
        {
            return await ProgramFollowDAO.Instance.GetFollowersByProgramIdAsync(programId);
        }

        public async Task<bool> UpdateAsync(ProgramFollow programFollow)
        {
            return await ProgramFollowDAO.Instance.UpdateAsync(programFollow);
        }

        public async Task<ProgramFollow> UpdateProgramFollowAsync(ProgramFollow follow)
        {
            return await ProgramFollowDAO.Instance.UpdateProgramFollowAsync(follow);    
        }
    }
}
