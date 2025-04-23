using BOs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Services
{
    public interface ICloudflareUploadService
    {
        Task<(string StreamId, string PlaybackUrl, string Mp4Url)> UploadVideoAsync(IFormFile videoFile);
    }

    public class CloudflareUploadService : ICloudflareUploadService
    {
        private readonly CloudflareSettings _settings;
        private readonly HttpClient _httpClient;

        public CloudflareUploadService(IOptions<CloudflareSettings> settings, HttpClient httpClient)
        {
            _settings = settings.Value;
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _settings.ApiToken);
        }

        public async Task<(string StreamId, string PlaybackUrl, string Mp4Url)> UploadVideoAsync(IFormFile videoFile)
        {
            var url = $"https://api.cloudflare.com/client/v4/accounts/{_settings.AccountId}/stream";

            using var content = new MultipartFormDataContent();
            using var streamContent = new StreamContent(videoFile.OpenReadStream());
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(videoFile.ContentType);

            content.Add(streamContent, "file", videoFile.FileName);

            var response = await _httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Cloudflare upload failed: {errorContent}");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(jsonResponse);
            var result = doc.RootElement.GetProperty("result");
            var uid = result.GetProperty("uid").GetString();
            var playbackUrl = result.GetProperty("playback").GetProperty("hls").GetString();
            var mp4Url = $"https://{_settings.StreamDomain}.cloudflarestream.com/{uid}/downloads/default.mp4";

            return (uid, playbackUrl, mp4Url);
        }
    }
}

