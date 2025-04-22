using BOs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface IProgramFollowService
    {
        Task<List<ProgramFollow>> GetAllAsync();
        Task<ProgramFollow?> GetByIdAsync(int id);
        Task<List<ProgramFollow>> GetByAccountIdAsync(int accountId);
        Task<bool> AddAsync(ProgramFollow programFollow);
        Task<bool> UpdateAsync(ProgramFollow programFollow);
        Task<bool> DeleteAsync(int id);
        Task<int> CountByProgramAsync(int programId);
        Task<ProgramFollow> CreateOrRefollowAsync(int accountId, int programId);

        Task<ProgramFollow> UpdateFollowStatusAsync(int programFollowId, string status);
        Task<List<ProgramFollow>> GetByProgramIdAsync(int programId);
    }
}
