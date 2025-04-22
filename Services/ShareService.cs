using BOs.Models;
using Repos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class ShareService : IShareService
    {
        private readonly IShareRepo _shareRepo;

        public ShareService(IShareRepo shareRepo)
        {
            _shareRepo = shareRepo;
        }

        public async Task<List<Share>> GetAllSharesAsync()
        {
            return await _shareRepo.GetAllSharesAsync();
        }
        public async Task<List<Share>> GetAllActiveSharesAsync()
        {
            return await _shareRepo.GetAllActiveSharesAsync();
        }

        public async Task<Share?> GetShareByIdAsync(int shareId)
        {
            return await _shareRepo.GetShareByIdAsync(shareId);
        }

        public async Task<bool> AddShareAsync(Share share)
        {
            return await _shareRepo.AddShareAsync(share);
        }

        public async Task<bool> UpdateShareAsync(Share share)
        {
            return await _shareRepo.UpdateShareAsync(share);
        }

        public async Task<bool> DeleteShareAsync(int shareId)
        {
            return await _shareRepo.DeleteShareAsync(shareId);
        }
        public async Task<int> GetTotalSharesForVideoAsync(int videoHistoryId)
        {
            return await _shareRepo.GetTotalSharesForVideoAsync(videoHistoryId);
        }

        public async Task<int> GetTotalSharesAsync()
        {
            return await _shareRepo.GetTotalSharesAsync();
        }
        public async Task<Dictionary<int, int>> GetSharesPerVideoAsync()
        {
            return await _shareRepo.GetSharesPerVideoAsync();
        }

    }

}
