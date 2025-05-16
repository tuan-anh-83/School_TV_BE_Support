using BOs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Services
{
    public interface ICloudflareUploadService
    {
        Task<(string StreamId, string PlaybackUrl, string Mp4Url)> UploadVideoAsync(IFormFile videoFile);
        Task<string?> UploadImageAsync(IFormFile imageFile);
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

        public async Task<string?> UploadImageAsync(IFormFile imageFile)
        {
            var url = $"https://api.cloudflare.com/client/v4/accounts/{_settings.AccountId}/images/v1";

            using var content = new MultipartFormDataContent();
            using var stream = imageFile.OpenReadStream();
            var streamContent = new StreamContent(stream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(imageFile.ContentType);

            content.Add(streamContent, "file", imageFile.FileName);

            var response = await _httpClient.PostAsync(url, content);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Cloudflare upload failed: {errorContent}");
            }

            // Trả về dữ liệu JSON từ Cloudflare cho client
            using var doc = JsonDocument.Parse(responseString);
            var root = doc.RootElement;

            if (root.TryGetProperty("result", out var result) &&
                result.TryGetProperty("variants", out var variants) &&
                variants.GetArrayLength() > 0)
            {
                var imageUrl = variants[0].GetString();
                return imageUrl;
            }
            return null;
        }
    }
}

