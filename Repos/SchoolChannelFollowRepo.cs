using BOs.Models;
using DAOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repos
{
    public class SchoolChannelFollowRepo : ISchoolChannelFollowRepo
    {
        public  Task AddFollowAsync(SchoolChannelFollow follow)
        {
           return  SchoolChannelFollowDAO.Instance.AddFollowAsync(follow);
        }

        public Task<List<object>> GetAllFollowedSchoolChannelsAsync()
        {
           return SchoolChannelFollowDAO.Instance.GetAllFollowedSchoolChannelsAsync();
        }

        public Task<List<SchoolChannelFollow>> GetAllFollowsAsync()
        {
            return SchoolChannelFollowDAO.Instance.GetAllFollowsAsync();    
        }

        public Task<SchoolChannelFollow> GetFollowAsync(int accountId, int schoolChannelId)
        {
            return SchoolChannelFollowDAO.Instance.GetFollowAsync(accountId, schoolChannelId);
        }

        public Task<int> GetFollowCountAsync(int schoolChannelId)
        {
            return SchoolChannelFollowDAO.Instance.GetFollowCountAsync (schoolChannelId);
        }

        public Task<IEnumerable<SchoolChannel>> GetFollowedSchoolChannelsAsync(int accountId)
        {
            return SchoolChannelFollowDAO.Instance.GetFollowedSchoolChannelsAsync (accountId);
        }

        public Task<List<SchoolChannelFollow>> GetFollowersByChannelIdAsync(int channelId)
        {
            return SchoolChannelFollowDAO.Instance.GetFollowersByChannelIdAsync (channelId);
        }

        public Task<List<SchoolChannelFollow>> GetFollowersBySchoolChannelIdAsync(int schoolChannelId)
        {
            return SchoolChannelFollowDAO.Instance.GetFollowersBySchoolChannelIdAsync (schoolChannelId);
        }

        public Task<bool> IsFollowingAsync(int accountId, int schoolChannelId)
        {
            return SchoolChannelFollowDAO.Instance.IsFollowingAsync (accountId, schoolChannelId);
        }

        public Task UpdateFollowStatusAsync(int accountId, int schoolChannelId, string status)
        {
            return SchoolChannelFollowDAO.Instance.UpdateFollowStatusAsync (accountId, schoolChannelId, status);
        }
    }
}
