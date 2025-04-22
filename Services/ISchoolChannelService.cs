using BOs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface ISchoolChannelService
    {
        Task<IEnumerable<SchoolChannel>> GetAllAsync();
        Task<IEnumerable<SchoolChannel>> GetAllActiveAsync();
        Task<SchoolChannel> GetByIdAsync(int id);
        Task<IEnumerable<SchoolChannel>> SearchAsync(string? keyword, string? address, int? accountId);
        Task CreateAsync(SchoolChannel schoolChannel);
        Task UpdateAsync(SchoolChannel schoolChannel);
        Task<bool> DeleteByNameAsync(string name);
        Task<bool> SchoolChannelExistsAsync(int schoolChannelId);
        Task<IEnumerable<object>> GetAllChannelsAsync();
        Task<bool> DoesAccountHaveSchoolChannelAsync(int accountId);
    }
}
