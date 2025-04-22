using BOs.Models;
using Repos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class ProgramFollowService : IProgramFollowService
    {
        private readonly IProgramFollowRepo _programFollowRepository;

        public ProgramFollowService(IProgramFollowRepo programFollowRepository)
        {
            _programFollowRepository = programFollowRepository;
        }

        public async Task<List<ProgramFollow>> GetByProgramIdAsync(int programId)
        {
            return await _programFollowRepository.GetByProgramIdAsync(programId);
        }

        public async Task<List<ProgramFollow>> GetAllAsync()
        {
            var follows = await _programFollowRepository.GetAllAsync();
            if (!follows.Any())
                throw new Exception("No program follows found.");

            return follows;
        }

        public async Task<ProgramFollow?> GetByIdAsync(int id)
        {
            if (id <= 0)
                throw new Exception("Invalid ProgramFollow ID.");

            var follow = await _programFollowRepository.GetByIdAsync(id);
            if (follow == null)
                throw new Exception("ProgramFollow not found.");

            return follow;
        }

        public async Task<List<ProgramFollow>> GetByAccountIdAsync(int accountId)
        {
            if (accountId <= 0)
                throw new Exception("Invalid Account ID.");

            var follows = await _programFollowRepository.GetByAccountIdAsync(accountId);
            if (!follows.Any())
                throw new Exception("No follows found for this account.");

            return follows;
        }

        public async Task<bool> AddAsync(ProgramFollow programFollow)
        {
            if (programFollow == null)
                throw new Exception("Invalid ProgramFollow data.");

            return await _programFollowRepository.AddAsync(programFollow);
        }

        public async Task<bool> UpdateAsync(ProgramFollow programFollow)
        {
            if (programFollow == null || programFollow.ProgramFollowID <= 0)
                throw new Exception("Invalid ProgramFollow data.");

            return await _programFollowRepository.UpdateAsync(programFollow);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            if (id <= 0)
                throw new Exception("Invalid ProgramFollow ID.");

            return await _programFollowRepository.DeleteAsync(id);
        }

        public async Task<int> CountByProgramAsync(int programId)
        {
            if (programId <= 0)
                throw new Exception("Invalid Program ID.");

            return await _programFollowRepository.CountByProgramAsync(programId);
        }

        public async Task<ProgramFollow> CreateOrRefollowAsync(int accountId, int programId)
        {
            var existing = await _programFollowRepository.GetByAccountAndProgramAsync(accountId, programId);

            if (existing != null)
            {
                existing.Status = "Followed";
                existing.FollowedAt = DateTime.UtcNow;
                await _programFollowRepository.UpdateProgramFollowAsync(existing);
                return existing;
            }

            var newFollow = new ProgramFollow
            {
                AccountID = accountId,
                ProgramID = programId,
                Status = "Followed",
                FollowedAt = DateTime.UtcNow
            };

            return await _programFollowRepository.CreateProgramFollowAsync(newFollow);
        }

        public async Task<ProgramFollow> UpdateFollowStatusAsync(int programFollowId, string status)
        {
            var follow = await _programFollowRepository.GetByIdAsync(programFollowId);
            if (follow == null)
                throw new Exception("Follow record not found.");

            follow.Status = status;

            if (status == "Followed")
                follow.FollowedAt = DateTime.UtcNow;

            await _programFollowRepository.UpdateProgramFollowAsync(follow);
            return follow;
        }
    }
}
