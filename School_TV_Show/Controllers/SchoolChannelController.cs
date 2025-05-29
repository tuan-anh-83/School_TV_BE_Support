using BOs.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using School_TV_Show.DTO;
using School_TV_Show.Helpers;
using Services;
using System.Security.Claims;

namespace School_TV_Show.Controllers
{
    [Route("api/schoolchannels")]
    [ApiController]
    public class SchoolChannelController : ControllerBase
    {
        private readonly ISchoolChannelService _service;
        private readonly ILogger<SchoolChannelController> _logger;
        private readonly ICloudflareUploadService _cloudflareUploadService;
        private readonly IAccountService _accountService;
        TimeZoneInfo vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

        public SchoolChannelController(
            ISchoolChannelService service, 
            IAccountService accountService, 
            ILogger<SchoolChannelController> logger,
            ICloudflareUploadService cloudflareUploadService
        )
        {
            _service = service;
            _accountService = accountService;
            _logger = logger;
            _cloudflareUploadService = cloudflareUploadService;
        }


        // GET: api/schoolchannels/all
        [Authorize(Roles = "Admin")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAllSchoolChannels()
        {
            try
            {
                var schoolChannels = await _service.GetAllAsync();
                if (!schoolChannels.Any())
                    return NotFound("No school channels found.");

                var response = schoolChannels.Select(sc => FormatSchoolChannelResponse(sc));
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all school channels");
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/schoolchannels/active
        [Authorize(Roles = "User,SchoolOwner,Admin,Advertiser")]
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveSchoolChannels()
        {
            try
            {
                var schoolChannels = await _service.GetAllActiveAsync();
                if (!schoolChannels.Any())
                    return NotFound("No active school channels found.");

                var response = schoolChannels.Select(sc => FormatSchoolChannelResponse(sc));
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active school channels");
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/schoolchannels/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSchoolChannelById(int id)
        {
            try
            {
                var schoolChannel = await _service.GetByIdAsync(id);
                if (schoolChannel == null)
                    return NotFound("School channel not found");

                if (schoolChannel.Account == null || 
                    !schoolChannel.Account.Status.Equals("Active", StringComparison.OrdinalIgnoreCase))
                {
                    return NotFound("School channel not found");
                }

                return Ok(FormatSchoolChannelResponse(schoolChannel));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving school channel by id");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("upload-image")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadImage([FromForm] IFormFile file)
        {
            var url = await _cloudflareUploadService.UploadImageAsync(file);
            return Ok(new {url});
        }

        // POST: api/schoolchannels
        [Authorize(Roles = "SchoolOwner")]
        [HttpPost]
        public async Task<IActionResult> CreateSchoolChannel([FromForm] CreateSchoolChannelRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                return BadRequest(new { errors });
            }

            var (hasViolation, message) = ContentModerationHelper.ValidateAllStringProperties(request);

            if (hasViolation)
            {
                return BadRequest(new { message });
            }

            if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Description))
                return BadRequest("Name and Description are required.");

            var accountId = GetAuthenticatedUserId();
            if (accountId == null)
                return Unauthorized("User is not authenticated.");

            bool alreadyExists = await _service.DoesAccountHaveSchoolChannelAsync(accountId.Value);
            if (alreadyExists)
            {
                return BadRequest("Each account can only create one school channel.");
            }

            var account = await _accountService.GetAccountByIdAsync(accountId.Value);
            if (account == null || string.IsNullOrWhiteSpace(account.Email))
                return BadRequest("Unable to retrieve email from account.");

            string? logoUrl = null;

            if(request.LogoFile != null)
            {
                logoUrl = await _cloudflareUploadService.UploadImageAsync(request.LogoFile);
            }

            var schoolChannel = new SchoolChannel
            {
                Name = request.Name,
                Description = request.Description,
                Website = request.Website ?? string.Empty,
                Email = account.Email,
                Address = request.Address ?? string.Empty,
                AccountID = accountId.Value,
                Status = true,
                LogoUrl = logoUrl,
                CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone),
                UpdatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone)
            };

            try
            {
                await _service.CreateAsync(schoolChannel);
                var createdSchoolChannel = await _service.GetByIdAsync(schoolChannel.SchoolChannelID);
                return CreatedAtAction(nameof(GetSchoolChannelById), new { id = schoolChannel.SchoolChannelID },
                    FormatSchoolChannelResponse(createdSchoolChannel));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating school channel");
                return StatusCode(500, "Internal server error");
            }
        }
        [Authorize(Roles = "SchoolOwner")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSchoolChannel(int id, [FromForm] UpdateSchoolChannelRequestDTO request)
        {
            var (hasViolation, message) = ContentModerationHelper.ValidateAllStringProperties(request);

            if (hasViolation)
            {
                return BadRequest(new { message });
            }

            var accountId = GetAuthenticatedUserId();
            if (accountId == null)
                return Unauthorized("User is not authenticated.");

            var schoolChannel = await _service.GetByIdAsync(id);
            if (schoolChannel == null)
                return NotFound("School channel not found");

            if (schoolChannel.AccountID != accountId)
                return Forbid("You do not have permission to update this school channel.");

            var account = schoolChannel.Account;
            if (account == null || !account.Status.Equals("Active", StringComparison.OrdinalIgnoreCase))
                return Forbid("Your account is not active.");

            if (!schoolChannel.Status)
                return BadRequest("This school channel is not active and cannot be updated.");

            if (!string.IsNullOrWhiteSpace(request.Name))
                schoolChannel.Name = request.Name;

            if (!string.IsNullOrWhiteSpace(request.Description))
                schoolChannel.Description = request.Description;

            if (!string.IsNullOrWhiteSpace(request.Address))
                schoolChannel.Address = request.Address;

            if (!string.IsNullOrWhiteSpace(request.Website))
                schoolChannel.Website = request.Website;

            string? logoUrl = null;
            if(request.LogoFile != null)
            {
                logoUrl = await _cloudflareUploadService.UploadImageAsync(request.LogoFile);
            }

            schoolChannel.UpdatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);
            schoolChannel.LogoUrl = logoUrl;

            try
            {
                await _service.UpdateAsync(schoolChannel);
                var updatedSchoolChannel = await _service.GetByIdAsync(id);
                return Ok(FormatSchoolChannelResponse(updatedSchoolChannel));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating school channel");
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/schoolchannels/search
        [Authorize(Roles = "User,SchoolOwner,Admin")]
        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<IActionResult> SearchSchoolChannels(
            [FromQuery] string? keyword,
            [FromQuery] string? address,
            [FromQuery] int? accountId)
        {
            try
            {
                var schoolChannels = await _service.SearchAsync(keyword, address, accountId);
                if (!schoolChannels.Any())
                    return NotFound("No active school channels matched your search criteria.");

                var response = schoolChannels.Select(sc => FormatSchoolChannelResponse(sc));
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching school channels");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("search-by-name")]
        [AllowAnonymous]
        public async Task<IActionResult> SearchSchoolChannelsByName([FromQuery] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest("Name parameter is required.");
            }
            try
            {
                var schoolChannels = await _service.SearchAsync(name, null, null);
                if (!schoolChannels.Any())
                    return NotFound("No school channels found with the provided name.");

                var response = schoolChannels.Select(sc => FormatSchoolChannelResponse(sc));
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching school channels by name");
                return StatusCode(500, "Internal server error");
            }
        }

        [NonAction]
        private int? GetAuthenticatedUserId()
        {
            var accountIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (accountIdClaim == null || !int.TryParse(accountIdClaim.Value, out int accountId))
            {
                return null;
            }
            return accountId;
        }

        [NonAction]
        private object FormatSchoolChannelResponse(SchoolChannel sc)
        {
            return new
            {
                sc.SchoolChannelID,
                sc.Name,
                sc.Description,
                sc.AccountID,
                sc.Status,
                sc.Website,
                sc.Email,
                sc.Address,
                sc.CreatedAt,
                sc.UpdatedAt,
                sc.LogoUrl,
                Followers = sc.Followers?.Count() ?? 0,
                Account = sc.Account != null ? new
                {
                    sc.Account.AccountID,
                    sc.Account.Username,
                    sc.Account.Email,
                    sc.Account.Fullname,
                    sc.Account.Address,
                    sc.Account.PhoneNumber,
                    sc.Account.AccountPackages
                } : null,
                sc.News,
                Programs = sc.Programs
            };
        }

        [NonAction]
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
