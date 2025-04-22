using BOs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repos
{
    public interface IProgramFollowRepo
    {
        Task<List<ProgramFollow>> GetAllAsync();
        Task<ProgramFollow?> GetByIdAsync(int id);
        Task<List<ProgramFollow>> GetByAccountIdAsync(int accountId);
        Task<bool> AddAsync(ProgramFollow programFollow);
        Task<bool> UpdateAsync(ProgramFollow programFollow);
        Task<bool> DeleteAsync(int id);
        Task<int> CountByProgramAsync(int programId);

        Task<ProgramFollow> GetByAccountAndProgramAsync(int accountId, int programId);
        Task<ProgramFollow> CreateProgramFollowAsync(ProgramFollow follow);
        Task<ProgramFollow> UpdateProgramFollowAsync(ProgramFollow follow);
        Task<List<ProgramFollow>> GetFollowersByProgramIdAsync(int programId);
        Task<List<ProgramFollow>> GetByProgramIdAsync(int programId);
    }
}
