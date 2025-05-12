using BOs.Models;
using DAOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repos
{
    public class AdLiveStreamRepo : IAdLiveStreamRepo
    {
        public async Task<int> AddRangeAdLiveStream(List<AdLiveStream> ads)
        {
            return await AdLiveStreamDAO.Instance.AddRangeAdLiveStreamAsync(ads);
        }

        public async Task<IEnumerable<AdLiveStream>> GetDueAds(DateTime now)
        {
            return await AdLiveStreamDAO.Instance.GetDueAds(now);
        }

        public async Task<IEnumerable<AdLiveStream>> GetExistsAdLiveStreams(int scheduleID)
        {
            return await AdLiveStreamDAO.Instance.GetExistsAdLiveStreams(scheduleID);
        }

        public async Task SaveChangeAsync()
        {
            await AdLiveStreamDAO.Instance.SaveChangeAsync();
        }

        public async Task<int> UpdateAdLiveStream(AdLiveStream adLiveStream)
        {
            return await AdLiveStreamDAO.Instance.UpdateAdLiveStream(adLiveStream);
        }

        public async Task<int> UpdateRangeAdLiveStream(List<AdLiveStream> adLiveStreams)
        {
            return await AdLiveStreamDAO.Instance.UpdateRangeAdLiveStream(adLiveStreams);
        }

        public void UpdateStatus(int adLiveStreamId)
        {
            AdLiveStreamDAO.Instance.UpdateStatus(adLiveStreamId);
        }

        public async Task UpdateStatusAlternative(int adLiveStreamId)
        {
            await AdLiveStreamDAO.Instance.UpdateStatusAlternative(adLiveStreamId);
        }
    }
}
