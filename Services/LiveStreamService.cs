﻿using BOs.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Repos;
using Services.ClouflareDTO;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Services
{
    public class LiveStreamService : ILiveStreamService
    {
        private readonly ILiveStreamRepo _repository;
        private readonly IPackageRepo _packageRepo;
        private readonly IAccountPackageRepo _accountPackageRepo;
        private readonly HttpClient _httpClient;
        private readonly ILogger<LiveStreamService> _logger;
        private readonly CloudflareSettings _cloudflareSettings;

        public LiveStreamService(
            ILiveStreamRepo repository,
            IPackageRepo packageRepo,
            IAccountPackageRepo accountPackageRepo,
            HttpClient httpClient,
            ILogger<LiveStreamService> logger,
            IOptions<CloudflareSettings> cloudflareSettings)
        {
            _repository = repository;
            _packageRepo = packageRepo;
            _accountPackageRepo = accountPackageRepo;
            _httpClient = httpClient;
            _logger = logger;
            _cloudflareSettings = cloudflareSettings.Value;

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _cloudflareSettings.ApiToken);
        }

        public async Task<bool> StartLiveStreamAsync(VideoHistory stream)
        {
            if (stream.ProgramID == null) return false;

            var program = await _repository.GetProgramByIdAsync(stream.ProgramID.Value);
            if (program == null) return false;

            if (!string.IsNullOrEmpty(program.CloudflareStreamId))
            {
                var checkUrl = $"https://api.cloudflare.com/client/v4/accounts/{_cloudflareSettings.AccountId}/stream/live_inputs/{program.CloudflareStreamId}";
                var checkResponse = await _httpClient.GetAsync(checkUrl);

                if (checkResponse.IsSuccessStatusCode)
                {
                    var checkJson = await checkResponse.Content.ReadAsStringAsync();
                    var existingStream = JsonSerializer.Deserialize<CloudflareLiveInputResponse>(checkJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    var rtmps = existingStream?.Result?.Rtmps;
                    if (rtmps != null)
                    {
                        stream.CloudflareStreamId = program.CloudflareStreamId;
                        stream.URL = $"{rtmps.Url}{rtmps.StreamKey}";
                        stream.PlaybackUrl = string.Empty;
                        stream.CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
                        stream.Status = true;

                        return await _repository.AddVideoHistoryAsync(stream);
                    }
                }

                program.CloudflareStreamId = null;
            }

            var payload = new
            {
                meta = new { name = stream.Description },
                recording = new { mode = "automatic" },
                mode = "push",
                playback_policy = new[] { "public" }
            };

            var jsonPayload = JsonSerializer.Serialize(payload);
            var requestContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var url = $"https://api.cloudflare.com/client/v4/accounts/{_cloudflareSettings.AccountId}/stream/live_inputs";
            var response = await _httpClient.PostAsync(url, requestContent);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode) return false;

            var cloudflareResponse = JsonSerializer.Deserialize<CloudflareLiveInputResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (cloudflareResponse?.Result == null) return false;

            stream.CloudflareStreamId = cloudflareResponse.Result.Uid;
            program.CloudflareStreamId = cloudflareResponse.Result.Uid;
            await _repository.UpdateProgramAsync(program);

            stream.URL = $"{cloudflareResponse.Result.Rtmps.Url}{cloudflareResponse.Result.Rtmps.StreamKey}";
            stream.PlaybackUrl = cloudflareResponse.Result.WebRTCPlayback?.Url ?? string.Empty;
            stream.CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
            stream.Status = true;

            return await _repository.AddVideoHistoryAsync(stream);
        }

        public async Task<bool> CheckStreamerStartedAsync(string cloudflareStreamId)
        {
            if (string.IsNullOrEmpty(cloudflareStreamId)) return false;

            var videosUrl = $"https://api.cloudflare.com/client/v4/accounts/{_cloudflareSettings.AccountId}/stream/live_inputs/{cloudflareStreamId}/videos";
            var response = await _httpClient.GetAsync(videosUrl);
            if (!response.IsSuccessStatusCode) return false;

            var json = await response.Content.ReadAsStringAsync();
            var videoDetails = JsonSerializer.Deserialize<CloudflareVideoListResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return videoDetails?.Result?.Any(v => v.Status?.State == "live-inprogress") ?? false;
        }

        public async Task<bool> CheckLiveInputExistsAsync(string streamId)
        {
            if (string.IsNullOrEmpty(streamId)) return false;

            var url = $"https://api.cloudflare.com/client/v4/accounts/{_cloudflareSettings.AccountId}/stream/live_inputs/{streamId}";
            var response = await _httpClient.GetAsync(url);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> SaveRecordedVideoFromWebhookAsync(string cloudflareInputUid, string downloadableUrl, string hlsUrl)
        {
            var existing = await _repository.GetRecordedVideoByStreamIdAsync(cloudflareInputUid);
            if (existing != null) return false;

            var video = await _repository.GetVideoHistoryByStreamIdAsync(cloudflareInputUid);
            if (video == null) return false;

            video.MP4Url = downloadableUrl;
            video.PlaybackUrl = hlsUrl;
            video.Status = false;
            video.Type = "Recorded";
            video.UpdatedAt = DateTime.UtcNow;

            return await _repository.UpdateVideoHistoryAsync(video);
        }

        public async Task<bool> EndStreamAndReturnLinksAsync(VideoHistory stream)
        {
            stream.Status = false;
            stream.Type = "Recorded";
            stream.UpdatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));

            var stopSuccess = await EndLiveStreamAsync(stream);
            if (!stopSuccess)
            {
                _logger.LogWarning($"Không thể dừng live input {stream.CloudflareStreamId}");
                return false;
            }

            CloudflareVideoResult? recorded = null;
            int tryCount = 0;

            while (recorded == null && tryCount < 10)
            {
                await Task.Delay(TimeSpan.FromSeconds(10));

                var videosUrl = $"https://api.cloudflare.com/client/v4/accounts/{_cloudflareSettings.AccountId}/stream/live_inputs/{stream.CloudflareStreamId}/videos";
                var response = await _httpClient.GetAsync(videosUrl);
                if (!response.IsSuccessStatusCode)
                {
                    tryCount++;
                    continue;
                }

                var json = await response.Content.ReadAsStringAsync();
                var videoDetails = JsonSerializer.Deserialize<CloudflareVideoListResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                recorded = videoDetails?.Result?.FirstOrDefault();

                tryCount++;
            }

            if (recorded == null)
            {
                _logger.LogWarning($"Không tìm thấy video sau khi kết thúc stream Cloudflare.");
                return false;
            }

            stream.PlaybackUrl = $"https://customer-{_cloudflareSettings.StreamDomain}.cloudflarestream.com/{recorded.Uid}/iframe";

            var videoReady = false;
            int waitCount = 0;

            while (!videoReady && waitCount < 10)
            {
                await Task.Delay(TimeSpan.FromSeconds(10));

                var videoStatusUrl = $"https://api.cloudflare.com/client/v4/accounts/{_cloudflareSettings.AccountId}/stream/{recorded.Uid}";
                var statusResp = await _httpClient.GetAsync(videoStatusUrl);

                if (statusResp.IsSuccessStatusCode)
                {
                    var statusJson = await statusResp.Content.ReadAsStringAsync();
                    var metaParsed = JsonSerializer.Deserialize<CloudflareMetadataResponse>(statusJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (metaParsed?.Result?.Status?.State == "ready")
                    {
                        videoReady = true;
                        stream.Duration = metaParsed.Result.Duration;
                        break;
                    }
                }

                waitCount++;
            }

            if (!videoReady)
            {
                _logger.LogWarning($"Video {recorded.Uid} chưa sẵn sàng.");
                return false;
            }

            var downloadUrl = $"https://api.cloudflare.com/client/v4/accounts/{_cloudflareSettings.AccountId}/stream/{recorded.Uid}/downloads";
            var startDownload = await _httpClient.PostAsync(downloadUrl, null);

            if (startDownload.IsSuccessStatusCode)
            {
                int retry = 0;
                while (retry < 5)
                {
                    await Task.Delay(TimeSpan.FromSeconds(15));

                    var statusResp = await _httpClient.GetAsync(downloadUrl);
                    var statusJson = await statusResp.Content.ReadAsStringAsync();
                    var parsed = JsonSerializer.Deserialize<CloudflareDownloadStatusResponse>(statusJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (parsed?.Result?.Default?.Status == "ready")
                    {
                        stream.MP4Url = parsed.Result.Default.Url;
                        break;
                    }

                    retry++;
                }
            }

            var updated = await _repository.UpdateVideoHistoryAsync(stream);

            if (stream.ProgramID != null)
            {
                var accountPackage = await _packageRepo.GetCurrentPackageAndDurationByProgramIdAsync(stream.ProgramID.Value);
                if (accountPackage != null && stream.Duration.HasValue)
                {
                    accountPackage.MinutesUsed += (stream.Duration.Value / 60.0);
                    accountPackage.RemainingMinutes = accountPackage.TotalMinutesAllowed - accountPackage.MinutesUsed;
                    await _accountPackageRepo.UpdateAccountPackageAsync(accountPackage);

                    _logger.LogInformation($"Cập nhật gói - Đã dùng: {accountPackage.MinutesUsed}, Còn lại: {accountPackage.RemainingMinutes}");
                }
            }

            return updated;
        }

        public async Task<bool> EndLiveStreamAsync(VideoHistory stream)
        {
            var deleteUrl = $"https://api.cloudflare.com/client/v4/accounts/{_cloudflareSettings.AccountId}/stream/live_inputs/{stream.CloudflareStreamId}";
            var response = await _httpClient.DeleteAsync(deleteUrl);

            if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogInformation($"Live input {stream.CloudflareStreamId} đã được xóa hoặc không còn tồn tại.");
                return true;
            }

            var body = await response.Content.ReadAsStringAsync();
            _logger.LogError($"Lỗi khi xóa live input {stream.CloudflareStreamId}: {response.StatusCode} - {body}");
            return false;
        }

        public async Task<bool> IsStreamLiveAsync(string cloudflareStreamId)
        {
            var url = $"https://api.cloudflare.com/client/v4/accounts/{_cloudflareSettings.AccountId}/stream/live_inputs/{cloudflareStreamId}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode) return false;

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<CloudflareLiveInputResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return result?.Result?.Status?.State?.ToLower() == "live";
        }

        public async Task<VideoHistory> GetLiveStreamByIdAsync(int id) => await _repository.GetLiveStreamByIdAsync(id);
        public async Task<IEnumerable<VideoHistory>> GetActiveLiveStreamsAsync() => await _repository.GetActiveLiveStreamsAsync();
        public async Task<bool> AddLikeAsync(VideoLike like) => await _repository.AddLikeAsync(like);
        public async Task<bool> AddViewAsync(VideoView view) => await _repository.AddViewAsync(view);
        public async Task<bool> AddShareAsync(Share share) => await _repository.AddShareAsync(share);
        public async Task<bool> CreateScheduleAsync(Schedule schedule) => await _repository.CreateScheduleAsync(schedule);
        public async Task<IEnumerable<Schedule>> GetSchedulesBySchoolChannelIdAsync(int schoolChannelId) => await _repository.GetSchedulesBySchoolChannelIdAsync(schoolChannelId);
        public async Task<bool> CreateProgramAsync(Program program) => await _repository.CreateProgramAsync(program);

        public async Task<VideoHistory?> GetLiveStreamByCloudflareUIDAsync(string uid) => await _repository.GetVideoHistoryByStreamIdAsync(uid);

        public async Task<bool> UpdateLiveStreamAsync(VideoHistory stream) => await _repository.UpdateVideoHistoryAsync(stream);

        public async Task<VideoHistory?> GetActiveLiveStreamByCloudflareUIDAsync(string uid) => await _repository.GetLiveVideoHistoryByStreamIdAsync(uid);

        #region Cloudflare Models
        private class CloudflareVideoListResponse { public List<CloudflareVideoResult> Result { get; set; } }
        private class CloudflareVideoResult { public string Uid { get; set; } public CloudflareVideoStatus Status { get; set; } public double Duration { get; set; } }
        private class CloudflareVideoStatus { public string State { get; set; } }
        private class CloudflareDownloadStatusResponse { public CloudflareDownloadResult Result { get; set; } }
        private class CloudflareDownloadResult { public CloudflareDownloadDefault Default { get; set; } }
        private class CloudflareDownloadDefault { public string Status { get; set; } public string Url { get; set; } }
        private class CloudflareLiveInputResponse { public CloudflareLiveInputResult Result { get; set; } }
        private class CloudflareLiveInputResult
        {
            public string Uid { get; set; }
            public CloudflareRtmps Rtmps { get; set; }
            public CloudflareWebRTCPlayback WebRTCPlayback { get; set; }
            public CloudflareStatus Status { get; set; }
        }
        private class CloudflareRtmps { public string Url { get; set; } public string StreamKey { get; set; } }
        private class CloudflareWebRTCPlayback { public string Url { get; set; } }
        private class CloudflareStatus { public string State { get; set; } }
        #endregion
    }
}
