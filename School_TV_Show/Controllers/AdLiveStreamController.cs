using BOs.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_TV_Show.DTO;
using Services;
using System.Globalization;

namespace School_TV_Show.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdLiveStreamController : ControllerBase
    {
        private readonly IAdLiveStreamService _adLiveStreamService;
        private readonly IAccountPackageService _accountPackageService;

        public AdLiveStreamController(IAdLiveStreamService adLiveStreamService, IAccountPackageService accountPackageService)
        {
            _adLiveStreamService = adLiveStreamService;
            _accountPackageService = accountPackageService;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("add-to-livestream")]
        public async Task<IActionResult> AddAdsToLiveStream([FromBody] AddAdToLiveStreamRequestDTO request)
        {
            try
            {
                int count = 0;
                List<int> confligIds = new List<int>();
                if (request == null || request.Ads == null || request.Ads.Count == 0)
                    return BadRequest("Invalid request data");

                var existing = await _adLiveStreamService.GetExistsAdLiveStreams(request.ScheduleId);

                var newEntries = new List<AdLiveStream>();

                foreach (var ad in request.Ads)
                {
                    if (!DateTime.TryParseExact(ad.PlayAt, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var startDate))
                        continue;

                    var endDate = startDate.AddSeconds(ad.Duration);

                    bool conflict = existing.Any(x =>
                        !x.IsPlayed &&
                        (
                            // thời gian xung đột
                            (x.PlayAt < endDate && x.PlayAt.AddSeconds(x.Duration) > startDate)
                        ));

                    if (!conflict)
                    {
                        newEntries.Add(new AdLiveStream
                        {
                            AdScheduleID = ad.AdScheduleId,
                            ScheduleID = request.ScheduleId,
                            AccountID = request.AccountId,
                            PlayAt = startDate,
                            Duration = ad.Duration,
                            IsPlayed = false
                        });
                    }
                    else
                    {
                        confligIds.Add(ad.AdScheduleId);
                    }
                }

                if (newEntries.Any())
                {
                    count = await _adLiveStreamService.AddRangeAdLiveStream(newEntries);
                }

                return Ok(new
                {
                    added = count,
                    skipped = request.Ads.Count - count,
                    message = confligIds.Count > 0 ? $"Quảng cáo bị trùng thời gian - Mã: {string.Join(", ", confligIds)}" : string.Empty
                });
            }
            catch (Exception)
            {
                throw;
            }
        }

        [AllowAnonymous]
        [HttpPost("ads-hook")]
        public async Task<IActionResult> AdsHook([FromQuery] int accountID, [FromQuery] int duration)
        {
            try
            {
                var package = await _accountPackageService.GetActiveAccountPackageAsync(accountID);

                if (package != null)
                {
                    package.MinutesUsed += (duration / 60);
                    package.RemainingMinutes = package.TotalMinutesAllowed - package.MinutesUsed;
                    await _accountPackageService.UpdateAccountPackageAsync(package);
                }

                return Ok();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
