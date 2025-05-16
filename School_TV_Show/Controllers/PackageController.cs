using BOs.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using School_TV_Show.DTO;
using School_TV_Show.Helpers;
using Services;
using System.Security.Claims;

namespace School_TV_Show.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class PackageController : ControllerBase
    {
        private readonly IPackageService _packageService;
        private readonly ILogger<PackageController> _logger;
        TimeZoneInfo vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

        public PackageController(IPackageService packageService, ILogger<PackageController> logger)
        {
            _packageService = packageService;
            _logger = logger;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllPackages()
        {
            try
            {
                var packages = await _packageService.GetAllPackagesAsync();
                return Ok(packages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving packages");
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPackageById(int id)
        {
            try
            {
                var package = await _packageService.GetPackageByIdAsync(id);
                if (package == null)
                    return NotFound("Package not found");

                return Ok(package);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving package by id");
                return StatusCode(500, "Internal server error");
            }
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string name)
        {
            if (string.IsNullOrEmpty(name))
                return BadRequest("Name parameter is required.");

            List<Package> packages = await _packageService.SearchPackagesByNameAsync(name);
            if (packages == null || packages.Count == 0)
                return NotFound("No packages found with the provided name.");

            return Ok(packages);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> AddPackage([FromBody] CreatePackageRequestDTO request)
        {
            if (request == null)
                return BadRequest("Invalid package data.");

            var package = new Package
            {
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                Duration = request.Duration,
                TimeDuration = request.TimeDuration,
                Status = "Active",
                ForType = request.ForType,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            try
            {
                await _packageService.AddPackageAsync(package);
                return CreatedAtAction(nameof(GetPackageById), new { id = package.PackageID }, package);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating package");
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePackage(int id, [FromBody] UpdatePackageRequestDTO request)
        {
            try
            {
                var existingPackage = await _packageService.GetPackageByIdAsync(id);
                if (existingPackage == null)
                    return NotFound("Package not found");

                existingPackage.Name = request.Name;
                existingPackage.Description = request.Description;
                existingPackage.Price = request.Price;
                existingPackage.Duration = request.Duration;
                existingPackage.TimeDuration = request.TimeDuration;
                existingPackage.Status = request.Status;
                existingPackage.UpdatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);
                existingPackage.ForType = request.ForType;

                bool isUpdated = await _packageService.UpdatePackageAsync(existingPackage);
                if (!isUpdated)
                    return StatusCode(500, "Failed to update package");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating package");
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePackage(int id)
        {
            try
            {
                var result = await _packageService.DeletePackageAsync(id);
                if (!result)
                    return NotFound("Package not found");

                return Ok(new { message = "Package marked as inactive successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting package");
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Roles = "SchoolOwner,Advertiser")]
        [HttpGet("active")]
        public async Task<IActionResult> GetAllActivePackages()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            try
            {
                var packages = await _packageService.GetAllActivePackagesAsync();
                var result = packages.Where(p => p.ForType.Equals(role)).Select(p => new PackageAdminResponse
                {
                    PackageID = p.PackageID,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Duration = p.Duration,
                    TimeDuration = p.TimeDuration,
                    Status = p.Status,
                    ForType = p.ForType,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving admin packages");
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("statistics/top-packages")]
        public async Task<IActionResult> GetTopPurchasedPackages()
        {
            try
            {
                var topPackages = await _packageService.GetTopPurchasedPackagesAsync();
                return Ok(topPackages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving top purchased packages");
                return StatusCode(500, "Internal server error");
            }
        }
        [Authorize(Roles = "SchoolOwner,Advertiser")]
        [HttpGet("current")]
        public async Task<IActionResult> GetCurrentPackageInfo()
        {
            try
            {
                var accountIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (accountIdClaim == null || !int.TryParse(accountIdClaim.Value, out int accountId))
                    return Unauthorized("Invalid account");

                var result = await _packageService.GetCurrentPackageAndDurationByAccountIdAsync(accountId);

                if (result == null || result.Value.Item1 == null)
                    return NotFound("No active package found.");

                var (package, remainingDuration, expiredAt) = result.Value;

                var dto = new CurrentPackageInfoDTO
                {
                    PackageID = package.PackageID,
                    PackageName = package.Name,
                    Duration = package.Duration,
                    TimeDuration = package.TimeDuration,
                    Price = package.Price,
                    RemainingDuration = remainingDuration
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving current package info");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
