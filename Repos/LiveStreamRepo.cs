using BOs.Models;
using DAOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repos
{
    public class LiveStreamRepo : ILiveStreamRepo
    {
        public async Task<bool> AddLikeAsync(VideoLike like)
        {
            return await LiveStreamDAO.Instance.AddLikeAsync(like);
        }

        public Task AddScheduleAsync(Schedule schedule)
        {
            return LiveStreamDAO.Instance.AddScheduleAsync(schedule);
        }

        public async Task<bool> AddShareAsync(Share share)
        {
            return await LiveStreamDAO.Instance.AddShareAsync(share);
        }

        public async Task<bool> AddVideoHistoryAsync(VideoHistory stream)
        {
            return await LiveStreamDAO.Instance.AddVideoHistoryAsync(stream);
        }

        public async Task<bool> AddViewAsync(VideoView view)
        {
            return await LiveStreamDAO.Instance.AddViewAsync(view);
        }

        public async Task<bool> CreateProgramAsync(Program program)
        {
            return await LiveStreamDAO.Instance.CreateProgramAsync(program);    
        }

        public async Task<bool> CreateScheduleAsync(Schedule schedule)
        {
            return await LiveStreamDAO.Instance.CreateScheduleAsync(schedule);
        }

        public async Task<IEnumerable<VideoHistory>> GetActiveLiveStreamsAsync()
        {
            return await LiveStreamDAO.Instance.GetActiveLiveStreamsAsync();
        }

        public async Task<List<Schedule>> GetEndingSchedulesAsync(DateTime time)
        {
            return await LiveStreamDAO.Instance.GetEndingSchedulesAsync(time);
        }

        public async Task<int?> GetFallbackAdVideoHistoryIdAsync()
        {
            return await LiveStreamDAO.Instance.GetFallbackAdVideoHistoryIdAsync();
        }

        public async Task<List<Schedule>> GetLateStartCandidatesAsync(DateTime thresholdTime)
        {
            return await LiveStreamDAO.Instance.GetLateStartCandidatesAsync(thresholdTime);
        }

        public async Task<List<Schedule>> GetLateStartSchedulesPastEndTimeAsync(DateTime now)
        {
            return await LiveStreamDAO.Instance.GetLateStartSchedulesPastEndTimeAsync(now);
        }

        public async Task<List<Schedule>> GetLiveSchedulesAsync()
        {
            return await LiveStreamDAO.Instance.GetLiveSchedulesAsync();
        }

        public async Task<VideoHistory> GetLiveStreamByIdAsync(int id)
        {
            return await LiveStreamDAO.Instance.GetLiveStreamByIdAsync(id);
        }

        public async Task<AdSchedule?> GetNextAvailableAdAsync()
        {
            return await LiveStreamDAO.Instance.GetNextAvailableAdAsync();
        }

        public async Task<List<Schedule>> GetOverdueSchedulesAsync(DateTime currentTime)
        {
            return await LiveStreamDAO.Instance.GetOverdueSchedulesAsync(currentTime);
        }

        public async Task<List<Schedule>> GetPendingSchedulesAsync(DateTime time)
        {
            return await LiveStreamDAO.Instance.GetPendingSchedulesAsync(time);
        }

        public async Task<Program> GetProgramByIdAsync(int id)
        {
            return await LiveStreamDAO.Instance.GetProgramByIdAsync(id);
        }

        public async Task<List<Schedule>> GetReadySchedulesAsync(DateTime time)
        {
            return await LiveStreamDAO.Instance.GetReadySchedulesAsync(time);
        }

        public async Task<VideoHistory> GetRecordedVideoByStreamIdAsync(string streamId)
        {
            return await LiveStreamDAO.Instance.GetRecordedVideoByStreamIdAsync(streamId);
        }

        public async Task<IEnumerable<Schedule>> GetSchedulesBySchoolChannelIdAsync(int schoolChannelId)
        {
            return await LiveStreamDAO.Instance.GetSchedulesBySchoolChannelIdAsync(schoolChannelId);
        }

        public async Task<SchoolChannel?> GetSchoolChannelByIdAsync(int schoolChannelId)
        {
            return await LiveStreamDAO.Instance.GetSchoolChannelByIdAsync(schoolChannelId);
        }

        public async Task<VideoHistory?> GetVideoHistoryByIdAsync(int id)
        {
            return await LiveStreamDAO.Instance.GetVideoHistoryByIdAsync(id);  
        }

        public async Task<VideoHistory?> GetVideoHistoryByProgramIdAsync(int programId)
        {
            return await LiveStreamDAO.Instance.GetVideoHistoryByProgramIdAsync(programId);
        }

        public async Task<VideoHistory> GetVideoHistoryByStreamIdAsync(string cloudflareStreamId)
        {
           return await LiveStreamDAO.Instance.GetVideoHistoryByStreamIdAsync(cloudflareStreamId);
        }

        public async Task<VideoHistory?> GetLiveVideoHistoryByStreamIdAsync(string cloudflareStreamId)
        {
            return await LiveStreamDAO.Instance.GetLiveVideoHistoryByStreamIdAsync(cloudflareStreamId);
        }

        public async Task<List<Schedule>> GetWaitingToStartStreamsAsync()
        {
            return await LiveStreamDAO.Instance.GetWaitingToStartStreamsAsync();
        }

        public async Task SaveChangesAsync()
        {
            await LiveStreamDAO.Instance.SaveChangesAsync();
        }

        public Task UpdateAsync(Schedule schedule)
        {
            return LiveStreamDAO.Instance.UpdateAsync(schedule);    
        }

        public async Task<bool> UpdateProgramAsync(Program program)
        {
            return await LiveStreamDAO.Instance.UpdateProgramAsync(program);
        }

        public void UpdateSchedule(Schedule schedule)
        {
            LiveStreamDAO.Instance.UpdateSchedule(schedule);
        }

        public async Task<bool> UpdateVideoHistoryAsync(VideoHistory stream)
        {
            return await LiveStreamDAO.Instance.UpdateVideoHistoryAsync(stream);
        }

        public async Task<VideoHistory?> GetReadyVideoHistoryByProgramIdAsync(int programId)
        {
            return await LiveStreamDAO.Instance.GetReadyVideoHistoryByProgramIdAsync(programId);
        }
    }
}
