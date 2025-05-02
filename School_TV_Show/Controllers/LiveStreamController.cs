using Microsoft.AspNetCore.Mvc;

using Services;
using BOs.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using School_TV_Show.DTO;
using System.Text.Json;
using System.Collections.Concurrent;
using School_TV_Show.Helpers;
using Azure.Core;
using Microsoft.Identity.Client;

namespace School_TV_Show.Controllers
{
    [ApiController]
    [Route("api/livestreams")]
    public class LiveStreamController : ControllerBase
    {
        private readonly ILiveStreamService _liveStreamService;
        private readonly IVideoHistoryService _videoHistoryService;
        private readonly IPackageService _packageService;
        private readonly IAccountPackageService _accountPackageService;
        private readonly IScheduleService _scheduleService;
        private readonly ILogger<LiveStreamController> _logger;
        TimeZoneInfo vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _webhookLocks = new();

        public LiveStreamController(
            ILiveStreamService liveStreamService,
            IVideoHistoryService videoHistoryService,
            IPackageService packageService,
            IAccountPackageService accountPackageService,
            IScheduleService scheduleService,
            ILogger<LiveStreamController> logger)
        {
            _liveStreamService = liveStreamService;
            _videoHistoryService = videoHistoryService;
            _packageService = packageService;
            _accountPackageService = accountPackageService;
            _scheduleService = scheduleService;
            _logger = logger;
        }


/*        [HttpPost("start")]
        public async Task<IActionResult> StartLiveStream([FromBody] LiveStreamStartRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (hasViolation, message) = ContentModerationHelper.ValidateAllStringProperties(request);

            if (hasViolation)
            {
                return BadRequest(new { message });
            }

            var currentPackage = await _packageService.GetCurrentPackageAndDurationByProgramIdAsync(request.ProgramID);

            var stream = new VideoHistory
            {
                ProgramID = request.ProgramID,
                Description = request.Description,
                Status = true,
                Type = "Live",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                StreamAt = DateTime.UtcNow
            };

            if (currentPackage == null)
            {
                await _liveStreamService.EndLiveStreamAsync(stream);
                await _liveStreamService.EndStreamAndReturnLinksAsync(stream);
                return NotFound(new { error = "No active package found." });
            }

            if (currentPackage.RemainingHours <= 0 || currentPackage.ExpiredAt < TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone))
            {
                await _liveStreamService.EndLiveStreamAsync(stream);
                await _liveStreamService.EndStreamAndReturnLinksAsync(stream);
                return BadRequest(new { error = "Your package was expired." });
            }

            var success = await _liveStreamService.StartLiveStreamAsync(stream);

            if (!success)
                return StatusCode(500, "Failed to start live stream.");

            return Ok(new
            {
                message = "Live stream started successfully.",
                streamId = stream.VideoHistoryID,
                stream.URL,
                stream.PlaybackUrl
            });
        }*/
        [HttpPost("finalize/{id}")]
        public async Task<IActionResult> FinalizeStreamAndGetLinks(int id)
        {
            var stream = await _liveStreamService.GetLiveStreamByIdAsync(id);
            if (stream == null || !stream.Status)
                return NotFound("Live stream not found or already ended.");

            var result = await _liveStreamService.EndStreamAndReturnLinksAsync(stream);
            if (!result)
                return StatusCode(500, "Failed to finalize and fetch stream links.");

            return Ok(new
            {
                message = "Stream finalized successfully.",
                playbackUrl = stream.PlaybackUrl,
                mp4Url = stream.MP4Url
            });
        }

        [HttpPost("disable-input/{id}")]
        public async Task<IActionResult> DisableStreamInput(int id)
        {
            var stream = await _liveStreamService.GetLiveStreamByIdAsync(id);
            if (stream == null || !stream.Status)
                return NotFound("Live stream not found or already ended.");

            var result = await _liveStreamService.EndLiveStreamAsync(stream);
            if (!result)
                return StatusCode(500, "Failed to disable live stream input.");

            return Ok(new
            {
                message = "Live stream input disabled. Cloudflare input removed.",
                streamId = stream.VideoHistoryID
            });
        }
        [HttpPost("{videoHistoryId}/end")]
        [Authorize(Roles = "Admin,SchoolOwner")]
        public async Task<IActionResult> EndLivestreamNow(int videoHistoryId)
        {
            var video = await _videoHistoryService.GetVideoByIdAsync(videoHistoryId);
            if (video == null || !video.Status)
                return NotFound(new { message = "Live video not found or already ended." });

            var success = await _liveStreamService.EndStreamAndReturnLinksAsync(video);

            if (!success)
                return StatusCode(500, new { message = "Failed to end livestream or save recorded video." });

            return Ok(new
            {
                message = "Livestream ended and video saved successfully.",
                video.VideoHistoryID,
                video.MP4Url,
                video.PlaybackUrl
            });
        }


        [HttpGet("active")]
        [AllowAnonymous]
        public async Task<IActionResult> GetActiveLiveStreams()
        {
            var streams = await _liveStreamService.GetActiveLiveStreamsAsync();
            var response = streams.Select(s => new
            {
                s.VideoHistoryID,
                s.Description,
                s.URL,
                s.PlaybackUrl,
                s.Type,
                s.StreamAt
            });

            return Ok(response);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetLiveStreamById(int id)
        {
            var stream = await _liveStreamService.GetLiveStreamByIdAsync(id);
            if (stream == null)
                return NotFound("Live stream not found.");

            return Ok(new
            {
                stream.VideoHistoryID,
                stream.Description,
                stream.URL,
                stream.PlaybackUrl,
                stream.Type,
                stream.StreamAt
            });
        }

        [HttpPost("{id}/like")]
        [Authorize(Roles = "User,SchoolOwner,Admin")]
        public async Task<IActionResult> LikeStream(int id)
        {
            var userId = GetUserIdFromToken();

            var like = new VideoLike
            {
                VideoHistoryID = id,
                AccountID = userId,
                Quantity = 1
            };

            var result = await _liveStreamService.AddLikeAsync(like);
            return result ? Ok(new { message = "Like recorded successfully." }) : StatusCode(500, "Failed to record like.");
        }

        [HttpPost("{id}/view")]
        [AllowAnonymous]
        public async Task<IActionResult> ViewStream(int id)
        {
            var view = new VideoView
            {
                VideoHistoryID = id,
                Quantity = 1
            };

            var result = await _liveStreamService.AddViewAsync(view);
            return result ? Ok(new { message = "View recorded successfully." }) : StatusCode(500, "Failed to record view.");
        }

        [HttpPost("{id}/share")]
        [Authorize(Roles = "User,SchoolOwner,Admin")]
        public async Task<IActionResult> ShareStream(int id)
        {
            var userId = GetUserIdFromToken();

            var share = new Share
            {
                VideoHistoryID = id,
                AccountID = userId,
                Quantity = 1
            };

            var result = await _liveStreamService.AddShareAsync(share);
            return result ? Ok(new { message = "Share recorded successfully." }) : StatusCode(500, "Failed to record share.");
        }

        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> ReceiveCloudflareWebhook([FromBody] JsonElement payload)
        {

            try
            {
                string jsonPayload = JsonSerializer.Serialize(payload);
                _logger.LogInformation($"Received Cloudflare webhook: {jsonPayload}");

                // Extract necessary fields from the payload
                if (payload.TryGetProperty("liveInput", out JsonElement liveInputElement))
                {
                    string? liveInputId = liveInputElement.GetString();
                    _logger.LogInformation($"Processing stream with UID: {liveInputId}");

                    // Get the stream status if available
                    string? state = "unknown";
                    if (payload.TryGetProperty("status", out JsonElement statusElement) &&
                        statusElement.TryGetProperty("state", out JsonElement stateElement))
                    {
                        state = stateElement.GetString();
                    }

                    if(state != "ready")
                    {
                        return BadRequest();
                    }

                    // Get stream duration if available
                    double? streamDuration = null;
                    if (payload.TryGetProperty("duration", out JsonElement durationElement))
                    {
                        streamDuration = durationElement.GetDouble();
                    }

                    string? previewUrl = payload.GetProperty("preview").GetString();
                    string? mp4Url = GetMp4UrlFromPreview(previewUrl);

                    if (!string.IsNullOrEmpty(liveInputId))
                    {
                        // Fetch the stream from database
                        await ExecuteWebhookWithLockAsync(liveInputId, async () =>
                        {

                            var stream = await _liveStreamService.GetActiveLiveStreamByCloudflareUIDAsync(liveInputId);
                            if (stream == null || stream.ProgramID == null)
                                return;

                            // Always mark status as false to end stream
                            stream.Status = false;
                            stream.PlaybackUrl = previewUrl ?? string.Empty;
                            stream.MP4Url = mp4Url ?? string.Empty;
                            stream.Type = "Recorded";

                            // If Cloudflare duration is available, use it directly
                            if (streamDuration.HasValue)
                            {
                                stream.Duration = streamDuration.Value / 3600.0; // Convert seconds to hours

                                _logger.LogInformation($"Stream duration from Cloudflare: {stream.Duration} hours");

                                await _liveStreamService.UpdateLiveStreamAsync(stream);
                                await MarkScheduleAsEndedAsync(stream.ProgramID.Value, stream.VideoHistoryID);

                                // Update account package with hours used
                                var accountPackage = await _packageService.GetCurrentPackageAndDurationByProgramIdAsync(stream.ProgramID.Value);
                                if (accountPackage != null && stream.Duration.HasValue)
                                {
                                    accountPackage.HoursUsed += stream.Duration.Value;
                                    accountPackage.RemainingHours = accountPackage.TotalHoursAllowed - accountPackage.HoursUsed;
                                    await _accountPackageService.UpdateAccountPackageAsync(accountPackage);

                                    _logger.LogInformation($"Updated account package - Hours used: {accountPackage.HoursUsed}, Remaining: {accountPackage.RemainingHours}");
                                }
                            }
                        });
                    }

                    return Ok(new { success = true });
                }

                return BadRequest(new { error = "Invalid payload format" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Cloudflare webhook");
                return StatusCode(500, new { error = "Failed to process webhook" });
            }
        }

        private string? GetMp4UrlFromPreview(string? previewUrl)
        {
            if (string.IsNullOrEmpty(previewUrl)) return null;

            if (Uri.TryCreate(previewUrl, UriKind.Absolute, out var uri))
            {
                var host = uri.Host; // ex: customer-xxx.cloudflarestream.com
                var segments = uri.Segments;
                if (segments.Length > 1)
                {
                    var videoId = segments[1].Trim('/');
                    return $"https://{host}/{videoId}/downloads/default.mp4";
                }
            }

            return null;
        }

        private async Task MarkScheduleAsEndedAsync(int programId, int videoHistoryId)
        {
            var schedule = await _scheduleService.GetScheduleByProgramIdAsync(programId);

            if (schedule != null && schedule.Status != "Ended")
            {
                schedule.Status = "EndedEarly";
                schedule.LiveStreamEnded = true;
                schedule.VideoHistoryID = videoHistoryId;

                await _scheduleService.UpdateScheduleAsync(schedule);

                _logger.LogInformation($"📺 ScheduleID {schedule.ScheduleID} marked as Ended from Cloudflare webhook.");
            }
        }

        private async Task ExecuteWebhookWithLockAsync(string key, Func<Task> action)
        {
            var semaphore = _webhookLocks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));

            await semaphore.WaitAsync();
            try
            {
                await action();
            }
            finally
            {
                semaphore.Release();
            }
        }

        private int GetUserIdFromToken()
        {
            var accountIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return accountIdClaim != null ? int.Parse(accountIdClaim.Value) : 0;
        }
    }
}
