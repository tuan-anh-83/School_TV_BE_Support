using BOs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface IAdLiveStreamService
    {
        Task<IEnumerable<AdLiveStream>> GetExistsAdLiveStreams(int scheduleID);
        Task<int> AddRangeAdLiveStream(List<AdLiveStream> ads);
        void UpdateStatusAlternative(int adLiveStreamId);
    }
}
