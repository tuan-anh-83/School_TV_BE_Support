using BOs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repos
{
    public interface ILiveStreamRepo
    {
        Task<bool> AddVideoHistoryAsync(VideoHistory stream);
        Task<bool> UpdateVideoHistoryAsync(VideoHistory stream);
        Task<List<Schedule>> GetWaitingToStartStreamsAsync();
        Task<Program> GetProgramByIdAsync(int id);
        Task<bool> UpdateProgramAsync(Program program);
        Task<bool> AddLikeAsync(VideoLike like);
        Task<bool> AddViewAsync(VideoView view);
        Task<bool> AddShareAsync(Share share);
        Task<AdSchedule?> GetNextAvailableAdAsync();
        Task<IEnumerable<Schedule>> GetSchedulesBySchoolChannelIdAsync(int schoolChannelId);
        Task<VideoHistory> GetVideoHistoryByStreamIdAsync(string cloudflareStreamId);
        Task<VideoHistory> GetLiveStreamByIdAsync(int id);
        Task<IEnumerable<VideoHistory>> GetActiveLiveStreamsAsync();
        Task<bool> CreateScheduleAsync(Schedule schedule);
        Task<bool> CreateProgramAsync(Program program);
        Task<VideoHistory> GetRecordedVideoByStreamIdAsync(string streamId);
        Task<List<Schedule>> GetLateStartCandidatesAsync(DateTime thresholdTime);
        void UpdateSchedule(Schedule schedule);
        Task<List<Schedule>> GetLiveSchedulesAsync();
        Task<List<Schedule>> GetOverdueSchedulesAsync(DateTime currentTime);
        Task<List<Schedule>> GetPendingSchedulesAsync(DateTime time);
        Task<List<Schedule>> GetReadySchedulesAsync(DateTime time);
        Task<List<Schedule>> GetEndingSchedulesAsync(DateTime time);
        Task<VideoHistory?> GetVideoHistoryByIdAsync(int id);
        Task<int?> GetFallbackAdVideoHistoryIdAsync();
        Task AddScheduleAsync(Schedule schedule);
        Task SaveChangesAsync();
        Task<List<Schedule>> GetLateStartSchedulesPastEndTimeAsync(DateTime now);
        Task UpdateAsync(Schedule schedule);
        Task<VideoHistory?> GetVideoHistoryByProgramIdAsync(int programId);
        Task<SchoolChannel?> GetSchoolChannelByIdAsync(int schoolChannelId);
        Task<VideoHistory?> GetLiveVideoHistoryByStreamIdAsync(string liveInputId);
        Task<VideoHistory?> GetReadyVideoHistoryByProgramIdAsync(int programId);
    }
}
