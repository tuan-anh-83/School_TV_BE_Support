using BOs.Models;
using DAOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repos
{
    public class SchoolChannelRepo : ISchoolChannelRepo
    {
        public async Task AddAsync(SchoolChannel schoolChannel)
        {
            await SchoolChannelDAO.Instance.AddAsync(schoolChannel);
        }

        public async Task<bool> DeleteByNameAsync(string name)
        {
            return await SchoolChannelDAO.Instance.DeleteByNameAsync(name);
        }

        public async Task<bool> DoesAccountHaveSchoolChannelAsync(int accountId)
        {
            return await SchoolChannelDAO.Instance.DoesAccountHaveSchoolChannelAsync(accountId);
        }

        public async Task<IEnumerable<SchoolChannel>> GetAllActiveAsync()
        {
            return await SchoolChannelDAO.Instance.GetAllActiveAsync();
        }

        public async Task<IEnumerable<SchoolChannel>> GetAllAsync()
        {
            return await SchoolChannelDAO.Instance.GetAllAsync();
        }

        public async Task<SchoolChannel?> GetByIdAsync(int id)
        {
            return await SchoolChannelDAO.Instance.GetByIdAsync(id);
        }

        /*public async Task<SchoolChannel?> GetByNameAsync(string name)
        {
            return await SchoolChannelDAO.Instance.GetByNameAsync(name);
        }*/

        public async Task<IEnumerable<SchoolChannel>> SearchAsync(string? keyword, string? address, int? accountId)
        {
            return await SchoolChannelDAO.Instance.SearchAsync(keyword, address, accountId);
        }

        public async Task UpdateAsync(SchoolChannel schoolChannel)
        {
            await SchoolChannelDAO.Instance.UpdateAsync(schoolChannel);
        }
    }
}
