using BOs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Repos;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


namespace Services
{
    public class VideoHistoryService : IVideoHistoryService
    {
        private readonly IVideoHistoryRepo _videoRepo;
        private readonly ILogger<VideoHistoryService> _logger;
        private readonly CloudflareSettings _cloudflareSettings;

        public VideoHistoryService(
            IVideoHistoryRepo videoRepo,
            ILogger<VideoHistoryService> logger,
            IOptions<CloudflareSettings> cloudflareOptions)
        {
            _videoRepo = videoRepo;
            _logger = logger;
            _cloudflareSettings = cloudflareOptions.Value;
        }

        public async Task<List<VideoHistory>> GetAllVideosAsync()
        {
            return await _videoRepo.GetAllVideosAsync();
        }

        public async Task<List<VideoHistory>> GetAllVideoHistoriesAsync()
        {
            return await _videoRepo.GetAllVideoHistoriesAsync();
        }

        public async Task<VideoHistory?> GetVideoByIdAsync(int videoHistoryId)
        {
            return await _videoRepo.GetVideoByIdAsync(videoHistoryId);
        }

        public async Task<VideoHistory?> GetLatestLiveStreamByProgramIdAsync(int programId)
        {
            return await _videoRepo.GetLatestLiveStreamByProgramIdAsync(programId);
        }

        public async Task<bool> UpdateVideoAsync(VideoHistory videoHistory)
        {
            return await _videoRepo.UpdateVideoAsync(videoHistory);
        }

        public async Task<bool> DeleteVideoAsync(int videoHistoryId)
        {
            return await _videoRepo.DeleteVideoAsync(videoHistoryId);
        }

        public async Task<int> GetTotalVideosAsync()
        {
            return await _videoRepo.CountTotalVideosAsync();
        }

        public async Task<int> GetTotalVideosByStatusAsync(bool status)
        {
            return await _videoRepo.CountByStatusAsync(status);
        }

        public async Task<(int totalViews, int totalLikes)> GetTotalViewsAndLikesAsync()
        {
            return await _videoRepo.GetTotalViewsAndLikesAsync();
        }

        public async Task<int> GetVideosByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _videoRepo.CountByDateRangeAsync(startDate, endDate);
        }

        public async Task<List<VideoHistory>> GetVideosByDateAsync(DateTime date)
        {
            return await _videoRepo.GetVideosByDateAsync(date);
        }

        public async Task<VideoHistory?> GetReplayVideoAsync(int programId, DateTime startTime, DateTime endTime)
        {
            return await _videoRepo.GetReplayVideoAsync(programId, startTime, endTime);
        }

        public async Task<VideoHistory?> GetReplayVideoByProgramAndTimeAsync(int programId, DateTime start, DateTime end)
        {
            return await _videoRepo.GetReplayVideoByProgramAndTimeAsync(programId, start, end);
        }

        public async Task<bool> AddVideoWithCloudflareAsync(IFormFile file, VideoHistory videoHistory)
        {
            try
            {
                var stream = file.OpenReadStream();
                var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _cloudflareSettings.ApiToken);

                var requestContent = new MultipartFormDataContent();
                requestContent.Add(new StreamContent(stream), "file", file.FileName);

                var uploadUrl = $"https://api.cloudflare.com/client/v4/accounts/{_cloudflareSettings.AccountId}/stream";
                var response = await httpClient.PostAsync(uploadUrl, requestContent);

                if (!response.IsSuccessStatusCode)
                    return false;

                var uploadResult = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(uploadResult);
                var uid = doc.RootElement.GetProperty("result").GetProperty("uid").GetString();
                var playbackUrl = $"https://videodelivery.net/{uid}/manifest/video.m3u8";
                var mp4Url = $"https://videodelivery.net/{uid}/downloads/default.mp4";

                videoHistory.CloudflareStreamId = uid;
                videoHistory.URL = playbackUrl;
                videoHistory.PlaybackUrl = playbackUrl;
                videoHistory.MP4Url = mp4Url;

                double? duration = null;
                for (int i = 0; i < 5; i++)
                {
                    await Task.Delay(4000);

                    var detailsUrl = $"https://api.cloudflare.com/client/v4/accounts/{_cloudflareSettings.AccountId}/stream/{uid}";
                    var detailsResponse = await httpClient.GetAsync(detailsUrl);

                    if (detailsResponse.IsSuccessStatusCode)
                    {
                        var detailsJson = await detailsResponse.Content.ReadAsStringAsync();
                        using var detailsDoc = JsonDocument.Parse(detailsJson);

                        if (detailsDoc.RootElement.TryGetProperty("result", out var resultElement) &&
                            resultElement.TryGetProperty("duration", out var durationElement))
                        {
                            duration = durationElement.GetDouble();
                            if (duration > 0)
                            {
                                videoHistory.Duration = duration;
                                break;
                            }
                        }
                        else
                        {
                            _logger.LogWarning("[Cloudflare] Duration missing for video UID={Uid}. Raw response: {Response}",
                                uid, detailsJson);
                        }
                    }
                }

                if (videoHistory.Duration == null)
                {
                    _logger.LogWarning("Duration for video UID={Uid} could not be fetched after retries.", uid);
                }

                return await _videoRepo.AddVideoAsync(videoHistory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading video to Cloudflare.");
                return false;
            }
        }

        public async Task<List<VideoHistory>> GetVideosByProgramIdAsync(int programId)
        {
            return await _videoRepo.GetVideosByProgramIdAsync(programId);
        }
    }
}