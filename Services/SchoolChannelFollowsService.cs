using BOs.Models;
using Repos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class SchoolChannelFollowsService : ISchoolChannelFollowService
    {
        private readonly ISchoolChannelFollowRepo _followRepository;

        public SchoolChannelFollowsService(ISchoolChannelFollowRepo followRepository)
        {
            _followRepository = followRepository;
        }
        public async Task<List<SchoolChannelFollow>> GetFollowersBySchoolChannelIdAsync(int schoolChannelId)
        {
            return await _followRepository.GetFollowersBySchoolChannelIdAsync(schoolChannelId);
        }

        public async Task AddFollowAsync(SchoolChannelFollow follow)
        {
            await _followRepository.AddFollowAsync(follow);
        }

        public async Task UpdateFollowStatusAsync(int accountId, int schoolChannelId, string status)
        {
            await _followRepository.UpdateFollowStatusAsync(accountId, schoolChannelId, status);
        }

        public async Task<int> GetFollowCountAsync(int schoolChannelId)
        {
            return await _followRepository.GetFollowCountAsync(schoolChannelId);
        }

        public async Task<List<SchoolChannelFollow>> GetAllFollowsAsync()
        {
            return await _followRepository.GetAllFollowsAsync();
        }

        public async Task<SchoolChannelFollow> GetFollowAsync(int accountId, int schoolChannelId)
        {
            return await _followRepository.GetFollowAsync(accountId, schoolChannelId);
        }

        public async Task<bool> IsFollowingAsync(int accountId, int schoolChannelId)
        {
            return await _followRepository.IsFollowingAsync(accountId, schoolChannelId);
        }
        public async Task<IEnumerable<SchoolChannel>> GetFollowedSchoolChannelsAsync(int accountId)
        {
            return await _followRepository.GetFollowedSchoolChannelsAsync(accountId);
        }
        public async Task<List<object>> GetAllFollowedSchoolChannelsAsync()
        {
            return await _followRepository.GetAllFollowedSchoolChannelsAsync();
        }


    }
}
