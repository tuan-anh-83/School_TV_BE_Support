using BOs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface ILiveStreamService
    {
        Task<bool> StartLiveStreamAsync(VideoHistory stream);
        Task<bool> CheckStreamerStartedAsync(string cloudflareStreamId);
        Task<bool> EndLiveStreamAsync(VideoHistory stream);
        Task<IEnumerable<VideoHistory>> GetActiveLiveStreamsAsync();
        Task<VideoHistory> GetLiveStreamByIdAsync(int id);
        Task<bool> AddLikeAsync(VideoLike like);
        Task<bool> AddViewAsync(VideoView view);
        Task<bool> AddShareAsync(Share share);
        Task<bool> CreateScheduleAsync(Schedule schedule);
        Task<IEnumerable<Schedule>> GetSchedulesBySchoolChannelIdAsync(int schoolChannelId);
        Task<bool> CreateProgramAsync(Program program);
        Task<bool> SaveRecordedVideoFromWebhookAsync(string cloudflareInputUid, string downloadableUrl, string hlsUrl);
        Task<bool> EndStreamAndReturnLinksAsync(VideoHistory stream);
        Task<bool> IsStreamLiveAsync(string cloudflareStreamId);
        Task<bool> CheckLiveInputExistsAsync(string streamId);
        Task<VideoHistory?> GetLiveStreamByCloudflareUIDAsync(string uid);
        Task<bool> UpdateLiveStreamAsync(VideoHistory stream);
    }
}
