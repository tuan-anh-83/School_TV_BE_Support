using Microsoft.AspNetCore.Mvc;
using School_TV_Show.DTO;
using BOs.Models;
using Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using School_TV_Show.Helpers;

namespace School_TV_Show.Controllers
{
    [Authorize(Roles = "SchoolOwner,Admin,User")]
    [ApiController]
    [Route("api/[controller]")]
    public class ProgramController : ControllerBase
    {
        private readonly IProgramService _programService;
        private readonly ISchoolChannelService _schoolChannelService;
        private readonly IPackageService _packageService;
        TimeZoneInfo vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

        public ProgramController(
            IProgramService programService, 
            ISchoolChannelService schoolChannelService,
            IPackageService packageService
        )
        {
            _programService = programService;
            _schoolChannelService = schoolChannelService;
            _packageService = packageService;
        }

        [HttpGet("with-videos")]
        public async Task<IActionResult> GetProgramsWithVideos([FromQuery] int? schoolChannelId)
        {
            var programs = await _programService.GetProgramsWithVideosAsync();

            if (schoolChannelId.HasValue)
                programs = programs.Where(p => p.SchoolChannelID == schoolChannelId.Value).ToList();

            return Ok(programs.Select(p => new {
                p.ProgramID,
                p.ProgramName,
                p.Title
            }));
        }


        [HttpGet("without-videos")]
        public async Task<IActionResult> GetProgramsWithoutVideos([FromQuery] int? schoolChannelId)
        {
            var programs = await _programService.GetProgramsWithoutVideosAsync();

            if (schoolChannelId.HasValue)
                programs = programs.Where(p => p.SchoolChannelID == schoolChannelId.Value).ToList();

            return Ok(programs.Select(p => new {
                p.ProgramID,
                p.ProgramName,
                p.Title
            }));
        }



        [HttpGet("all")]
        public async Task<IActionResult> GetAllPrograms()
        {
            var programs = await _programService.GetAllProgramsAsync();

            var result = programs.Select(p => new ProgramResponse
            {
                ProgramID = p.ProgramID,
                ProgramName = p.ProgramName,
                Title = p.Title,
                Status = p.Status,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                SchoolChannelID = p.SchoolChannelID,
                SchoolChannel = p.SchoolChannel == null ? null : new SchoolChannelResponse
                {
                    SchoolChannelID = p.SchoolChannel.SchoolChannelID,
                    Name = p.SchoolChannel.Name,
                    Description = p.SchoolChannel.Description,
                    Website = p.SchoolChannel.Website,
                    Email = p.SchoolChannel.Email,
                    Address = p.SchoolChannel.Address
                },
                Schedules = p.Schedules?.Select(s => new ScheduleResponse
                {
                    ScheduleID = s.ScheduleID,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    Status = s.Status,
                    LiveStreamStarted = s.LiveStreamStarted,
                    LiveStreamEnded = s.LiveStreamEnded,
                    ProgramID = s.ProgramID
                }).ToList()
            });

            return Ok(new ApiResponse(true, "All programs with schedule & channel", result));
        }

        [HttpPost]
        public async Task<IActionResult> CreateProgram([FromBody] CreateProgramRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse(false, "Invalid input", ModelState));

            var (hasViolation, message) = ContentModerationHelper.ValidateAllStringProperties(request);

            if (hasViolation)
            {
                return BadRequest(new { message });
            }

            var accountIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (accountIdClaim == null || !int.TryParse(accountIdClaim.Value, out int accountId))
                return Unauthorized("Invalid account");

            var currentPackage = await _packageService.GetCurrentPackageAndDurationByAccountIdAsync(accountId);

            if (currentPackage == null)
                return NotFound(new { error = "No active package found." });

            var (package, remainingDuration, expiredAt) = currentPackage.Value;

            if (remainingDuration == null || remainingDuration <= 0 || expiredAt < TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone))
                return BadRequest(new { error = "Your package was expired." });

            var program = new BOs.Models.Program
            {
                ProgramName = request.ProgramName,
                Title = request.Title,
                Status = "Active",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                SchoolChannelID = request.SchoolChannelID
            };

            var created = await _programService.CreateProgramAsync(program);
            return Ok(new ApiResponse(true, "Program created", new { programId = created.ProgramID }));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProgram(int id, [FromBody] UpdateProgramRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse(false, "Invalid input", ModelState));


            var (hasViolation, message) = ContentModerationHelper.ValidateAllStringProperties(request);

            if (hasViolation)
            {
                return BadRequest(new { message });
            }

            var program = await _programService.GetProgramByIdAsync(id);
            if (program == null)
                return NotFound(new ApiResponse(false, "Program not found"));

            program.ProgramName = request.ProgramName;
            program.Title = request.Title;
            program.UpdatedAt = DateTime.UtcNow;

            var success = await _programService.UpdateProgramAsync(program);
            if (!success)
                return StatusCode(500, new ApiResponse(false, "Failed to update program"));

            return Ok(new ApiResponse(true, "Program updated successfully"));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProgram(int id)
        {
            var program = await _programService.GetProgramByIdAsync(id);
            if (program == null)
                return NotFound(new ApiResponse(false, "Program not found"));

            var success = await _programService.DeleteProgramAsync(id);
            if (!success)
                return StatusCode(500, new ApiResponse(false, "Failed to delete program"));

            return Ok(new ApiResponse(true, "Program deleted successfully"));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProgramById(int id)
        {
            var p = await _programService.GetProgramByIdAsync(id);
            if (p == null)
                return NotFound(new ApiResponse(false, "Program not found"));

            var response = new ProgramResponse
            {
                ProgramID = p.ProgramID,
                ProgramName = p.ProgramName,
                Title = p.Title,
                Status = p.Status,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                SchoolChannelID = p.SchoolChannelID,
                SchoolChannel = p.SchoolChannel == null ? null : new SchoolChannelResponse
                {
                    SchoolChannelID = p.SchoolChannel.SchoolChannelID,
                    Name = p.SchoolChannel.Name,
                    Description = p.SchoolChannel.Description,
                    Website = p.SchoolChannel.Website,
                    Email = p.SchoolChannel.Email,
                    Address = p.SchoolChannel.Address
                },
                Schedules = p.Schedules?.Select(s => new ScheduleResponse
                {
                    ScheduleID = s.ScheduleID,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    Status = s.Status,
                    LiveStreamStarted = s.LiveStreamStarted,
                    LiveStreamEnded = s.LiveStreamEnded,
                    ProgramID = s.ProgramID
                }).ToList()
            };

            return Ok(new ApiResponse(true, "Program found", response));
        }
        [HttpGet("by-channel/{channelId}")]
        public async Task<ActionResult<IEnumerable<BOs.Models.Program>>> GetByChannelId(int channelId)
        {
            var programs = await _programService.GetProgramsByChannelIdAsync(channelId);
            return Ok(programs);
        }
    }
}
