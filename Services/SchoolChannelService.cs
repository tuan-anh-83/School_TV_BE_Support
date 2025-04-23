using BOs.Models;
using Repos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class SchoolChannelService : ISchoolChannelService
    {
        private readonly ISchoolChannelRepo _repository;

        public SchoolChannelService(ISchoolChannelRepo repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<SchoolChannel>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<IEnumerable<SchoolChannel>> GetAllActiveAsync()
        {
            return await _repository.GetAllActiveAsync();
        }

        public async Task CreateAsync(SchoolChannel schoolChannel)
        {
            await _repository.AddAsync(schoolChannel);
        }

        public async Task UpdateAsync(SchoolChannel schoolChannel)
        {
            await _repository.UpdateAsync(schoolChannel);
        }

        public async Task<bool> DeleteByNameAsync(string name)
        {
            return await _repository.DeleteByNameAsync(name);
        }

        public async Task<SchoolChannel> GetByIdAsync(int id)
        {
            var result = await _repository.GetByIdAsync(id);
            return result!;
        }

        public async Task<IEnumerable<SchoolChannel>> SearchAsync(string? keyword, string? address, int? accountId)
        {
            return await _repository.SearchAsync(keyword, address, accountId);
        }
        public async Task<bool> SchoolChannelExistsAsync(int schoolChannelId)
        {
            var schoolChannel = await _repository.GetByIdAsync(schoolChannelId);
            return schoolChannel != null;
        }

        public Task<IEnumerable<object>> GetAllChannelsAsync()
        {
            throw new NotImplementedException();
        }
        public async Task<bool> DoesAccountHaveSchoolChannelAsync(int accountId)
        {
            return await _repository.DoesAccountHaveSchoolChannelAsync(accountId);
        }

    }
}
