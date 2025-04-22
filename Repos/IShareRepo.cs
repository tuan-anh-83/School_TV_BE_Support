using BOs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repos
{
    public interface IShareRepo
    {
        Task<List<Share>> GetAllSharesAsync();
        Task<Share?> GetShareByIdAsync(int shareId);
        Task<List<Share>> GetAllActiveSharesAsync();
        Task<bool> AddShareAsync(Share share);
        Task<bool> UpdateShareAsync(Share share);
        Task<bool> DeleteShareAsync(int shareId);
        Task<int> GetTotalSharesForVideoAsync(int videoHistoryId);

        Task<int> GetTotalSharesAsync();
        Task<Dictionary<int, int>> GetSharesPerVideoAsync();
    }
}
