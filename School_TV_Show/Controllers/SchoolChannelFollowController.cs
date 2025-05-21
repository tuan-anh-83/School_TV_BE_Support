using BOs.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;
using System.Security.Claims;

namespace School_TV_Show.Controllers
{
    [Route("api/schoolchannelfollow")]
    [ApiController]
    [Authorize(Roles = "User,SchoolOwner,Admin,Advertiser")]
    public class SchoolChannelFollowController : ControllerBase
    {
        private readonly IFollowService _followService;
        private readonly ISchoolChannelService _schoolChannelService;

        public SchoolChannelFollowController(IFollowService followService, ISchoolChannelService schoolChannelService)
        {
            _followService = followService;
            _schoolChannelService = schoolChannelService;
        }

        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllFollows()
        {
            var follows = await _followService.GetAllFollowsAsync();
            return Ok(follows);
        }

        [HttpGet("count/{schoolChannelId}")]
        public async Task<IActionResult> GetFollowCount(int schoolChannelId)
        {
            if (!await _schoolChannelService.SchoolChannelExistsAsync(schoolChannelId))
            {
                return NotFound($"SchoolChannel with ID {schoolChannelId} does not exist.");
            }

            int count = await _followService.GetFollowCountAsync(schoolChannelId);
            return Ok(new { schoolChannelId, count });
        }

        [HttpGet("status/{schoolChannelId}")]
        public async Task<IActionResult> GetFollowStatus(int schoolChannelId)
        {
            if (!await _schoolChannelService.SchoolChannelExistsAsync(schoolChannelId))
            {
                return NotFound($"SchoolChannel with ID {schoolChannelId} does not exist.");
            }

            int accountId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            bool isFollowing = await _followService.IsFollowingAsync(accountId, schoolChannelId);
            return Ok(new { accountId, schoolChannelId, isFollowing });
        }

        [HttpPost("follow/{schoolChannelId}")]
        public async Task<IActionResult> Follow(int schoolChannelId)
        {
            var accountIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (accountIdClaim == null)
            {
                return Unauthorized("Invalid token. Account ID not found.");
            }

            if (!int.TryParse(accountIdClaim.Value, out int accountId))
            {
                return BadRequest("Invalid Account ID in token.");
            }

            if (!await _schoolChannelService.SchoolChannelExistsAsync(schoolChannelId))
            {
                return NotFound($"SchoolChannel with ID {schoolChannelId} does not exist.");
            }

            await _followService.AddFollowAsync(new SchoolChannelFollow
            {
                AccountID = accountId,
                SchoolChannelID = schoolChannelId
            });

            return Ok(new { message = "Followed successfully", accountId, schoolChannelId });
        }

        [HttpPut("unfollow/{schoolChannelId}")]
        public async Task<IActionResult> Unfollow(int schoolChannelId)
        {
            var accountIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (accountIdClaim == null)
            {
                return Unauthorized("Invalid token. Account ID not found.");
            }

            if (!int.TryParse(accountIdClaim.Value, out int accountId))
            {
                return BadRequest("Invalid Account ID in token.");
            }

            if (!await _schoolChannelService.SchoolChannelExistsAsync(schoolChannelId))
            {
                return NotFound($"SchoolChannel with ID {schoolChannelId} does not exist.");
            }

            var follow = await _followService.GetFollowAsync(accountId, schoolChannelId);
            if (follow == null)
            {
                return NotFound("Follow record not found.");
            }

            if (follow.Status == "Unfollowed")
            {
                return BadRequest("You have already unfollowed this school channel.");
            }

            await _followService.UpdateFollowStatusAsync(accountId, schoolChannelId, "Unfollowed");

            return Ok(new { message = "Unfollowed successfully", accountId, schoolChannelId });
        }

        [HttpPut("refollow/{schoolChannelId}")]
        public async Task<IActionResult> ReFollow(int schoolChannelId)
        {
            var accountIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (accountIdClaim == null)
            {
                return Unauthorized("Invalid token. Account ID not found.");
            }

            if (!int.TryParse(accountIdClaim.Value, out int accountId))
            {
                return BadRequest("Invalid Account ID in token.");
            }

            if (!await _schoolChannelService.SchoolChannelExistsAsync(schoolChannelId))
            {
                return NotFound($"SchoolChannel with ID {schoolChannelId} does not exist.");
            }

            var follow = await _followService.GetFollowAsync(accountId, schoolChannelId);
            if (follow == null)
            {
                return NotFound("Follow record not found.");
            }

            if (follow.Status == "Followed")
            {
                return BadRequest("You are already following this school channel.");
            }

            await _followService.UpdateFollowStatusAsync(accountId, schoolChannelId, "Followed");

            return Ok(new { message = "Re-followed successfully", accountId, schoolChannelId });
        }

        #region Get Followed School Channels
        [HttpGet("followed")]
        public async Task<IActionResult> GetFollowedSchoolChannels()
        {
            var accountIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (accountIdClaim == null || !int.TryParse(accountIdClaim.Value, out int accountId))
            {
                return Unauthorized("Invalid token. Account ID not found.");
            }

            var followedChannels = await _followService.GetFollowedSchoolChannelsAsync(accountId);

            if (followedChannels == null || !followedChannels.Any())
            {
                return NotFound(new { message = "You are not following any school channels." });
            }

            return Ok(followedChannels);
        }
        #endregion

        [HttpGet("statistics/all-followed")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllFollowedSchoolChannels()
        {
            var followedChannels = await _followService.GetAllFollowedSchoolChannelsAsync();
            return Ok(followedChannels);
        }

    }
}
