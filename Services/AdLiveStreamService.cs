using BOs.Models;
using Repos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class AdLiveStreamService : IAdLiveStreamService
    {
        private readonly IAdLiveStreamRepo _adLiveStreamRepo;

        public AdLiveStreamService(IAdLiveStreamRepo adLiveStreamRepo)
        {
            _adLiveStreamRepo = adLiveStreamRepo;
        }

        public async Task<int> AddRangeAdLiveStream(List<AdLiveStream> ads)
        {
            return await _adLiveStreamRepo.AddRangeAdLiveStream(ads);
        }

        public async Task<IEnumerable<AdLiveStream>> GetExistsAdLiveStreams(int scheduleID)
        {
            return await _adLiveStreamRepo.GetExistsAdLiveStreams(scheduleID);
        }
    }
}
