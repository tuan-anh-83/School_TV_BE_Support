using BOs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repos
{
    public interface IAdLiveStreamRepo
    {
        Task<IEnumerable<AdLiveStream>> GetExistsAdLiveStreams(int scheduleID);
        Task<int> AddRangeAdLiveStream(List<AdLiveStream> ads);
        Task<IEnumerable<AdLiveStream>> GetDueAds(DateTime now);
        Task<int> UpdateAdLiveStream(AdLiveStream adLiveStream);
        Task<int> UpdateRangeAdLiveStream(List<AdLiveStream> adLiveStreams);
        Task SaveChangeAsync();
        void UpdateStatus(int adLiveStreamId);
        void UpdateStatusAlternative(int adLiveStreamId);
    }
}
