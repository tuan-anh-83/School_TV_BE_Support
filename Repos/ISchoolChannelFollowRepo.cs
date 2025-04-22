using BOs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repos
{
    public interface ISchoolChannelFollowRepo
    {
        Task AddFollowAsync(SchoolChannelFollow follow);
        Task UpdateFollowStatusAsync(int accountId, int schoolChannelId, string status);
        Task<int> GetFollowCountAsync(int schoolChannelId);
        Task<List<SchoolChannelFollow>> GetAllFollowsAsync();
        Task<SchoolChannelFollow> GetFollowAsync(int accountId, int schoolChannelId);
        Task<bool> IsFollowingAsync(int accountId, int schoolChannelId);
        Task<IEnumerable<SchoolChannel>> GetFollowedSchoolChannelsAsync(int accountId);
        Task<List<object>> GetAllFollowedSchoolChannelsAsync();
        Task<List<SchoolChannelFollow>> GetFollowersByChannelIdAsync(int channelId);
        Task<List<SchoolChannelFollow>> GetFollowersBySchoolChannelIdAsync(int schoolChannelId);
    }
}
