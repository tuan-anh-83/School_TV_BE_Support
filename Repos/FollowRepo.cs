using BOs.Models;
using DAOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repos
{
    public class FollowRepo : IFollowRepo
    {
        public Task AddFollowAsync(SchoolChannelFollow follow)
        {
            return FollowDAO.Instance.AddFollowAsync(follow);
        }

        public async Task<List<object>> GetAllFollowedSchoolChannelsAsync()
        {
            return await FollowDAO.Instance.GetAllFollowedSchoolChannelsAsync();
        }

        public async Task<List<SchoolChannelFollow>> GetAllFollowsAsync()
        {
            return await FollowDAO.Instance.GetAllFollowsAsync();   
        }

        public async Task<SchoolChannelFollow> GetFollowAsync(int accountId, int schoolChannelId)
        {
            return await FollowDAO.Instance.GetFollowAsync(accountId, schoolChannelId);
        }

        public async Task<int> GetFollowCountAsync(int schoolChannelId)
        {
            return await FollowDAO.Instance.GetFollowCountAsync(schoolChannelId);
        }

        public async Task<IEnumerable<SchoolChannel>> GetFollowedSchoolChannelsAsync(int accountId)
        {
            return await FollowDAO.Instance.GetFollowedSchoolChannelsAsync(accountId); 
        }

        public async Task<List<SchoolChannelFollow>> GetFollowersByChannelIdAsync(int channelId)
        {
            return await FollowDAO.Instance.GetFollowersByChannelIdAsync(channelId);
        }

        public async Task<List<SchoolChannelFollow>> GetFollowersBySchoolChannelIdAsync(int schoolChannelId)
        {
            return await FollowDAO.Instance.GetFollowersBySchoolChannelIdAsync(schoolChannelId);
        }

        public async Task<bool> IsFollowingAsync(int accountId, int schoolChannelId)
        {
            return await FollowDAO.Instance.IsFollowingAsync(accountId, schoolChannelId);   
        }

        public Task UpdateFollowStatusAsync(int accountId, int schoolChannelId, string status)
        {
            return FollowDAO.Instance.UpdateFollowStatusAsync(accountId, schoolChannelId, status);    
        }
    }
}
