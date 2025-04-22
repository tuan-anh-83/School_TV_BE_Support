using BOs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repos
{
    public interface ISchoolChannelRepo
    {
        Task<IEnumerable<SchoolChannel>> GetAllAsync();
        Task<IEnumerable<SchoolChannel>> GetAllActiveAsync();
        Task<SchoolChannel?> GetByIdAsync(int id);
        Task<IEnumerable<SchoolChannel>> SearchAsync(string? keyword, string? address, int? accountId);
        Task AddAsync(SchoolChannel schoolChannel);
        Task UpdateAsync(SchoolChannel schoolChannel);
        Task<bool> DeleteByNameAsync(string name);
        Task<bool> DoesAccountHaveSchoolChannelAsync(int accountId);
    }
}
