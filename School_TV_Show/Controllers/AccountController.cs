﻿using Azure.Core;
using BOs.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using School_TV_Show.DTO;
using School_TV_Show.Helpers;
using Services;
using Services.Email;
using Services.Token;
using Services.Hubs;
using System.Security.Claims;

namespace School_TV_Show.Controllers
{

    [ApiController]
    [Route("api/accounts")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly IAccountPackageService _accountPackageService;
        private readonly IPasswordHasher<Account> _passwordHasher;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AccountController> _logger;
        private readonly IHubContext<AccountStatusHub> _accountStatusHub;
        private readonly ILiveStreamService _liveStreamService;
        private readonly TimeZoneInfo vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

        public AccountController(
            IAccountService accountService,
            ITokenService tokenService,
            IEmailService emailService,
            IAccountPackageService accountPackageService,
            IPasswordHasher<Account> passwordHasher,
            ILogger<AccountController> logger,
            IConfiguration configuration,
            IHubContext<AccountStatusHub> accountStatusHub,
            ILiveStreamService liveStreamService)
        {
            _accountService = accountService;
            _tokenService = tokenService;
            _emailService = emailService;
            _accountPackageService = accountPackageService;
            _passwordHasher = passwordHasher;
            _logger = logger;
            _configuration = configuration;
            _accountStatusHub = accountStatusHub;
            _liveStreamService = liveStreamService;
        }

        #region Registration Endpoints

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] AccountRequestDTO accountRequest)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                return BadRequest(new { errors });
            }

            var account = new Account
            {
                Username = accountRequest.Username,
                Email = accountRequest.Email,
                Password = accountRequest.Password,
                Fullname = accountRequest.Fullname,
                Address = accountRequest.Address,
                PhoneNumber = accountRequest.PhoneNumber,
                RoleID = 1,
                Status = "Active",
                CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone),
                UpdatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone)
            };

            bool result = await _accountService.SignUpAsync(account);
            if (!result)
                return Conflict("Username or Email already exists.");

            return Ok(new
            {
                message = "Account successfully registered.",
                account = new
                {
                    account.AccountID,
                    account.Username,
                    account.Email,
                    account.Fullname
                }
            });
        }

        [HttpPost("account-package")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateAccountPackage([FromBody] AccountPackage accountRequest)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                return BadRequest(new { errors });
            }

            bool result = await _accountPackageService.CreateAccountPackageAsync(accountRequest);

            if (!result)
                return Conflict("Exists.");

            return Ok(new
            {
                message = "Account successfully registered.",
                package = accountRequest
            });
        }

        [HttpPost("update-account-package")]
        [AllowAnonymous]
        public async Task<IActionResult> UpdateAccountPackage([FromBody] AccountPackage accountRequest)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                return BadRequest(new { errors });
            }

            var currentPackage = await _accountPackageService.GetActiveAccountPackageAsync(accountRequest.AccountID);

            if (currentPackage == null)
                return Conflict("Exists.");

            var result = await _accountPackageService.UpdateAccountPackageAsync(new AccountPackage
            {
                AccountPackageID = currentPackage.AccountPackageID,
                AccountID = currentPackage.AccountID,
                PackageID = accountRequest.PackageID,
                TotalMinutesAllowed = currentPackage.TotalMinutesAllowed + 10,
                MinutesUsed = currentPackage.MinutesUsed,
                RemainingMinutes = currentPackage.RemainingMinutes + 10,
                StartDate = currentPackage.StartDate,
                ExpiredAt = currentPackage.ExpiredAt != null ?
                    currentPackage.ExpiredAt.Value.AddDays(10) :
                    TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone).AddDays(10)
            });

            if (!result)
                return Conflict("Exists.");

            return Ok(new
            {
                message = "Account successfully registered.",
                package = accountRequest
            });
        }

        [HttpPost("schoolowner/signup")]
        public async Task<IActionResult> SchoolOwnerSignUp([FromBody] SchoolOwnerSignUpRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                return BadRequest(new { errors });
            }

            if (request.Password != request.ConfirmPassword)
                return BadRequest(new { error = "Password and Confirm Password do not match." });

            var account = new Account
            {
                Username = request.Username,
                Email = request.Email,
                Password = request.Password,
                Fullname = request.Fullname,
                Address = request.Address ?? string.Empty,
                PhoneNumber = request.PhoneNumber ?? string.Empty,
                RoleID = 2,
                Status = "Pending",
                CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone),
                UpdatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone)
            };

            bool result = await _accountService.SignUpAsync(account);
            if (!result)
                return Conflict("Username or Email already exists and is not eligible for re-registration.");

            var otpCode = new Random().Next(100000, 999999).ToString();
            var expiration = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone).AddMinutes(5);
            bool otpSaved = await _accountService.SaveOtpAsync(account.Email, otpCode, expiration);

            if (otpSaved)
            {
                try
                {
                    await _emailService.SendOtpEmailAsync(account.Email, otpCode);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send OTP email to {Email}", account.Email);
                    return StatusCode(500, "Failed to send OTP email.");
                }
            }

            return Ok(new
            {
                message = "School Owner registered. OTP has been sent to your email. Please verify.",
                account = new
                {
                    account.AccountID,
                    account.Username,
                    account.Email,
                    account.Fullname
                }
            });
        }

        [HttpPost("advertiser/signup")]
        public async Task<IActionResult> AdvertiserSignUp([FromBody] SchoolOwnerSignUpRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                return BadRequest(new { errors });
            }

            if (request.Password != request.ConfirmPassword)
                return BadRequest(new { error = "Password and Confirm Password do not match." });

            var account = new Account
            {
                Username = request.Username,
                Email = request.Email,
                Password = request.Password,
                Fullname = request.Fullname,
                Address = request.Address ?? string.Empty,
                PhoneNumber = request.PhoneNumber ?? string.Empty,
                RoleID = 4,
                Status = "Pending",
                CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone),
                UpdatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone)
            };

            bool result = await _accountService.SignUpAsync(account);
            if (!result)
                return Conflict("Username or Email already exists and is not eligible for re-registration.");

            var otpCode = new Random().Next(100000, 999999).ToString();
            var expiration = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone).AddMinutes(5);
            bool otpSaved = await _accountService.SaveOtpAsync(account.Email, otpCode, expiration);

            if (otpSaved)
            {
                try
                {
                    await _emailService.SendOtpEmailAsync(account.Email, otpCode);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send OTP email to {Email}", account.Email);
                    return StatusCode(500, "Failed to send OTP email.");
                }
            }

            return Ok(new
            {
                message = "School Owner registered. OTP has been sent to your email. Please verify.",
                account = new
                {
                    account.AccountID,
                    account.Username,
                    account.Email,
                    account.Fullname
                }
            });
        }

        [HttpPost("otp/schoolowner/register")]
        public async Task<IActionResult> SchoolOwnerOtpRegister([FromBody] OtpRegistrationRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                return BadRequest(new { errors });
            }

            if (request.Password != request.ConfirmPassword)
                return BadRequest(new { error = "Password and Confirm Password do not match." });

            var account = new Account
            {
                Username = request.Username,
                Email = request.Email,
                Password = request.Password,
                Fullname = request.Fullname,
                Address = request.Address,
                PhoneNumber = request.PhoneNumber,
                RoleID = 2,
                Status = "Pending",
                CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone),
                UpdatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone)
            };

            bool result = await _accountService.SignUpAsync(account);
            if (!result)
                return Conflict("Username or Email already exists.");

            var otpCode = new Random().Next(100000, 999999).ToString();
            var expiration = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone).AddMinutes(5);

            bool otpSaved = await _accountService.SaveOtpAsync(request.Email, otpCode, expiration);
            if (!otpSaved)
                return StatusCode(500, "Failed to generate OTP.");

            try
            {
                await _emailService.SendOtpEmailAsync(request.Email, otpCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send OTP email to {Email}", request.Email);
                return StatusCode(500, "Failed to send OTP email.");
            }

            return Ok(new
            {
                message = "School Owner registration successful. An OTP has been sent to your email. Please verify to complete registration."
            });
        }

        [HttpPost("otp/register")]
        public async Task<IActionResult> OtpRegister([FromBody] OtpRegistrationRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                return BadRequest(new { errors });
            }

            if (request.Password != request.ConfirmPassword)
                return BadRequest(new { error = "Password and Confirm Password do not match." });

            var account = new Account
            {
                Username = request.Username,
                Email = request.Email,
                Password = request.Password,
                Fullname = request.Fullname,
                Address = request.Address,
                PhoneNumber = request.PhoneNumber,
                RoleID = 1,
                Status = "Pending",
                CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone),
                UpdatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone)
            };

            bool result = await _accountService.SignUpAsync(account);
            if (!result)
                return Conflict("Username or Email already exists.");

            var otpCode = new Random().Next(100000, 999999).ToString();
            var expiration = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone).AddMinutes(5);

            bool otpSaved = await _accountService.SaveOtpAsync(request.Email, otpCode, expiration);
            if (!otpSaved)
                return StatusCode(500, "Failed to generate OTP.");

            try
            {
                await _emailService.SendOtpEmailAsync(request.Email, otpCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send OTP email to {Email}", request.Email);
                return StatusCode(500, "Failed to send OTP email.");
            }

            return Ok(new
            {
                message = "Registration successful. An OTP has been sent to your email. Please verify to activate your account."
            });
        }

        [HttpPost("otp/schoolowner/verify")]
        public async Task<IActionResult> VerifySchoolOwnerOtp([FromBody] VerifyOtpRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                return BadRequest(new { errors });
            }

            var account = await _accountService.GetAccountByEmailAsync(request.Email);
            if (account == null)
                return NotFound("Account not found.");
            if (!account.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Account is not in a state that requires OTP verification.");

            bool isValid = await _accountService.VerifyOtpAsync(request.Email, request.OtpCode);
            if (!isValid)
                return BadRequest("Invalid or expired OTP.");
            bool updateResult = await _accountService.UpdateAccountAsync(account);
            if (!updateResult)
            {

                _logger.LogInformation("No changes detected during school owner OTP verification; treating as success.");
            }
            await _accountService.InvalidateOtpAsync(request.Email);

            return Ok(new { message = "OTP verified successfully. Your account is pending admin approval." });
        }

        [HttpPost("otp/verify")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                return BadRequest(new { errors });
            }

            var account = await _accountService.GetAccountByEmailAsync(request.Email);
            if (account == null)
                return NotFound("Account not found.");
            if (account.Status.Equals("Active", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Account is already active.");

            bool isValid = await _accountService.VerifyOtpAsync(request.Email, request.OtpCode);
            if (!isValid)
                return BadRequest("Invalid or expired OTP.");
            account.Status = "Active";
            bool updateResult = await _accountService.UpdateAccountAsync(account);
            if (!updateResult)
            {
                _logger.LogInformation("No changes detected during OTP verification; treating as success.");
            }
            await _accountService.InvalidateOtpAsync(request.Email);

            return Ok(new { message = "Account verified successfully." });
        }

        [HttpPost("otp/resend")]
        public async Task<IActionResult> ResendOtp([FromBody] ResendOtpRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                return BadRequest(new { errors });
            }

            var account = await _accountService.GetAccountByEmailAsync(request.Email);
            if (account == null)
                return NotFound("Account not found.");

            if (!account.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase))
                return Ok(new { message = "Account is already verified." });

            var currentOtp = await _accountService.GetCurrentOtpAsync(request.Email);
            if (currentOtp != null && currentOtp.Expiration > TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone))
            {
                return Ok(new { message = "Your OTP is still active. Please use the existing OTP." });
            }

            var otpCode = new Random().Next(100000, 999999).ToString();
            var expiration = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone).AddMinutes(5);

            bool otpSaved = await _accountService.SaveOtpAsync(request.Email, otpCode, expiration);
            if (!otpSaved)
                return StatusCode(500, "Failed to generate OTP.");

            try
            {
                await _emailService.SendOtpEmailAsync(request.Email, otpCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to resend OTP email to {Email}", request.Email);
                return StatusCode(500, "Failed to resend OTP email.");
            }

            return Ok(new { message = "A new OTP has been sent to your email." });
        }

        #endregion

        #region Login & External Authentication

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO loginRequest)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                return BadRequest(new { errors });
            }

            var account = await _accountService.Login(loginRequest.Email, loginRequest.Password);
            if (account == null)
                return Unauthorized("Invalid login information or account is inactive.");
            if (!account.Status.Equals("Active", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new
                {
                    message = "Tài khoản của bạn đã bị khóa, nếu bạn nghĩ đây là một sự hiểu lầm, hãy gửi yêu cầu mở khóa cho chúng tôi.",
                    url = "mailto:admin@example.com?subject=Yêu cầu hỗ trợ mở tài khoản&body=Chào bạn,%0ATôi cần hỗ trợ về vấn đề mở tài khoản với lí do:"
                });
            if (account.RoleID == 0)
                return Unauthorized("Account is not permitted to login due to invalid role.");
            var token = _tokenService.GenerateToken(account);
            return Ok(new
            {
                message = "Login successful.",
                token,
                account = new
                {
                    account.AccountID,
                    account.Username,
                    account.Email,
                    account.Fullname,
                    RoleName = account.Role?.RoleName
                }
            });
        }

        [HttpGet("google-login")]
        public IActionResult GoogleLogin(string returnUrl = "/")
        {
            var state = Guid.NewGuid().ToString(); // Generate unique state
            HttpContext.Session.SetString("OAuthState", state);

            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleResponse", new { returnUrl }),
                Items = { { "LoginProvider", "Google" }, { "state", state } }
            };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("google-response")]
        public async Task<IActionResult> GoogleResponse(string returnUrl = "/")
        {
            var savedState = HttpContext.Session.GetString("OAuthState");
            var returnedState = Request.Query["state"];

            if (savedState != returnedState)
            {
                _logger.LogError("OAuth state mismatch!");
                return BadRequest("Invalid OAuth state.");
            }

            var result = await HttpContext.AuthenticateAsync("ExternalCookie");
            if (!result.Succeeded)
            {
                _logger.LogWarning("External authentication failed.");
                return BadRequest("External authentication error.");
            }

            var externalUser = result.Principal;
            var email = externalUser.FindFirst(ClaimTypes.Email)?.Value;
            var name = externalUser.FindFirst(ClaimTypes.Name)?.Value;
            var googleId = externalUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(googleId))
                return BadRequest("Necessary claims not received from Google.");

            var account = await _accountService.Login(email, string.Empty);
            if (account != null)
            {
                if (string.IsNullOrEmpty(account.ExternalProvider))
                {
                    account.ExternalProvider = "Google";
                    account.ExternalProviderKey = googleId;
                    await _accountService.UpdateAccountAsync(account);
                }
            }
            else
            {
                var newAccount = new Account
                {
                    Username = email,
                    Email = email,
                    Fullname = name,
                    Password = string.Empty,
                    RoleID = 1,
                    Status = "Active",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    ExternalProvider = "Google",
                    ExternalProviderKey = googleId
                };
                bool created = await _accountService.SignUpAsync(newAccount);
                if (!created)
                    return Conflict("Unable to create account.");

                account = newAccount;
            }

            var token = _tokenService.GenerateToken(account);
            await HttpContext.SignOutAsync("ExternalCookie");
            return Ok(new
            {
                message = "Google login successful.",
                token,
                account = new
                {
                    account.AccountID,
                    account.Username,
                    account.Email,
                    account.Fullname,
                    RoleName = account.Role?.RoleName
                }
            });
        }

        #endregion

        #region Account Management

        [HttpGet("info")]
        [Authorize(Roles = "User,SchoolOwner,Admin,Advertiser")]
        public async Task<IActionResult> GetAccountInformation()
        {
            try
            {
                var accountIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (accountIdClaim == null || !int.TryParse(accountIdClaim.Value, out int accountId))
                    return Unauthorized("Invalid or missing token.");
                var account = await _accountService.GetAccountByIdAsync(accountId);
                if (account == null)
                    return NotFound("Account not found.");
                if (!account.Status.Equals("Active", StringComparison.OrdinalIgnoreCase))
                    return Unauthorized("Account is not active.");
                var accountInfo = new
                {
                    account.AccountID,
                    account.Username,
                    account.Email,
                    account.Fullname,
                    account.Address,
                    account.PhoneNumber,
                    AccountPackage = account.AccountPackages.Select(ap => new
                    {
                        ap.AccountPackageID,
                        ap.MinutesUsed,
                        ap.RemainingMinutes
                    }).FirstOrDefault()
                };
                return Ok(accountInfo);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPatch("update")]
        [Authorize(Roles = "User,SchoolOwner,Admin,Advertiser")]
        public async Task<IActionResult> UpdateAccount([FromBody] PartialAccountUpdateRequest updateRequest)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                return BadRequest(new { errors });
            }

            var accountIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (accountIdClaim == null || !int.TryParse(accountIdClaim.Value, out int accountId))
                return Unauthorized("Invalid or missing token.");
            var account = await _accountService.GetAccountByIdAsync(accountId);
            if (account == null)
                return NotFound("Account not found.");
            if (!string.IsNullOrEmpty(updateRequest.Username))
            {
                var existingByUsername = await _accountService.GetAccountByUsernameAsync(updateRequest.Username);
                if (existingByUsername != null && existingByUsername.AccountID != accountId)
                    return Conflict("Username already exists.");
                account.Username = updateRequest.Username;
            }
            if (!string.IsNullOrEmpty(updateRequest.Email))
            {
                var existingByEmail = await _accountService.GetAccountByEmailAsync(updateRequest.Email);
                if (existingByEmail != null && existingByEmail.AccountID != accountId)
                    return Conflict("Email already exists.");
                account.Email = updateRequest.Email;
            }
            if (!string.IsNullOrEmpty(updateRequest.Fullname))
                account.Fullname = updateRequest.Fullname;
            if (updateRequest.Address != null)
                account.Address = updateRequest.Address;
            if (!string.IsNullOrEmpty(updateRequest.PhoneNumber))
                account.PhoneNumber = updateRequest.PhoneNumber;

            // Update the UpdatedAt timestamp
            account.UpdatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);

            bool updateResult = await _accountService.UpdateAccountAsync(account);
            if (!updateResult)
            {
                _logger.LogError("Failed to update account for AccountID: {AccountId}", accountId);
                return StatusCode(500, "A problem occurred while processing your request.");
            }
            return Ok(new
            {
                message = "Account updated successfully.",
                account = new
                {
                    account.AccountID,
                    account.Username,
                    account.Email,
                    account.Fullname,
                    account.Address,
                    account.PhoneNumber
                }
            });
        }

        [HttpPatch("change-password")]
        [Authorize(Roles = "User,SchoolOwner,Admin,Advertiser")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDTO changePasswordRequest)
        {
            var accountIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (accountIdClaim == null || !int.TryParse(accountIdClaim.Value, out int accountId))
                return Unauthorized("Invalid or missing token.");
            var account = await _accountService.GetAccountByIdAsync(accountId);
            if (account == null)
                return NotFound("Account not found.");
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                return BadRequest(new { errors });
            }
            bool isCurrentPasswordValid = BCrypt.Net.BCrypt.Verify(changePasswordRequest.CurrentPassword, account.Password);
            if (!isCurrentPasswordValid)
                return BadRequest("Current password is incorrect.");
            if (BCrypt.Net.BCrypt.Verify(changePasswordRequest.NewPassword, account.Password))
                return BadRequest("New password cannot be the same as the current password.");

            // Set the new password (will be hashed in DAO)
            account.Password = changePasswordRequest.NewPassword;
            account.UpdatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);
            // Ensure Status remains unchanged
            // account.Status is already loaded from database, so no need to change it

            bool updateResult = await _accountService.UpdateAccountAsync(account);
            if (!updateResult)
            {
                _logger.LogError("Failed to change password for AccountID: {AccountId}", accountId);
                return StatusCode(500, "A problem occurred while processing your request.");
            }
            return Ok(new { message = "Password successfully changed." });
        }

        #endregion

        #region Password Management

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var account = await _accountService.GetAccountByEmailAsync(request.Email);
            if (account == null)
            {
                return Ok(new { message = "If an account with that email exists, you will receive a password reset email." });
            }

            var token = Guid.NewGuid().ToString();
            var expiration = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone).AddHours(1);
            await _accountService.SavePasswordResetTokenAsync(account.AccountID, token, expiration);

            await _emailService.SendPasswordResetEmailAsync(request.Email, token);

            return Ok(new { message = "If an account with that email exists, you will receive a password reset email." });
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var account = await _accountService.GetAccountByEmailAsync(request.Email);
            if (account == null)
                return BadRequest("Invalid request.");
            var tokenValid = await _accountService.VerifyPasswordResetTokenAsync(account.AccountID, request.Token);
            if (!tokenValid)
                return BadRequest("Invalid or expired token.");

            // Set the new password (will be hashed in DAO)
            account.Password = request.NewPassword;
            account.UpdatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);

            await _accountService.UpdateAccountAsync(account);
            await _accountService.InvalidatePasswordResetTokenAsync(account.AccountID, request.Token);
            return Ok(new { message = "Password reset successfully." });
        }

        #endregion

        #region Admin Endpoints

        [Authorize(Roles = "Admin")]
        [HttpPatch("admin/update-status/{id}")]
        public async Task<IActionResult> UpdateAccountStatus(int id, [FromBody] StatusUpdateRequestDTO request)
        {
            if (id <= 0)
                return BadRequest("Invalid account ID.");
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                return BadRequest(new { errors });
            }
            var account = await _accountService.GetAccountByIdAsync(id);
            if (account == null)
                return NotFound("Account not found.");
            if (account.RoleID == 2)
            {
                var allowedStatuses = new[] { "Pending", "Active", "Reject", "InActive" };
                if (!allowedStatuses.Contains(request.Status, StringComparer.OrdinalIgnoreCase))
                    return BadRequest("Invalid status for SchoolOwner account.");

                var currentOtp = await _accountService.GetCurrentOtpAsync(account.Email);
                if (currentOtp != null && currentOtp.Expiration > TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone))
                {
                    return BadRequest("Cannot update status: School owner OTP verification is not complete.");
                }
            }
            else
            {
                var allowedStatuses = new[] { "Active", "InActive" };
                if (!allowedStatuses.Contains(request.Status, StringComparer.OrdinalIgnoreCase))
                    return BadRequest("Invalid status for User account.");
            }
            bool result = await _accountService.UpdateAccountStatusAsync(account, request.Status);
            if (!result)
                return StatusCode(500, "Failed to update account status.");

            // Send SignalR notification about status change
            await _accountStatusHub.Clients.Group($"Account_{id}")
                .SendAsync("AccountStatusChanged", new { accountId = id, newStatus = request.Status });

            // Nếu là SchoolOwner và bị ban (status khác Active), tắt tất cả live stream đang active
            if (account.RoleID == 2 && !request.Status.Equals("Active", StringComparison.OrdinalIgnoreCase))
            {
                // Lấy tất cả live stream đang active của SchoolOwner
                var activeStreams = (await _liveStreamService.GetActiveLiveStreamsAsync())
                    .Where(v => v.Program != null && v.Program.SchoolChannel != null && v.Program.SchoolChannel.AccountID == id)
                    .ToList();
                foreach (var stream in activeStreams)
                {
                    await _liveStreamService.EndStreamAndReturnLinksAsync(stream);
                }
            }

            await _emailService.SendStatusUserAsync(account.Email, request.Status);

            return Ok(new { message = "Account status updated successfully." });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin/statistics/signup-count")]
        public async Task<IActionResult> GetSignUpCounts()
        {
            int userCount = await _accountService.GetUserCountAsync();
            int schoolOwnerCount = await _accountService.GetSchoolOwnerCountAsync();
            return Ok(new { userCount, schoolOwnerCount });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin/all")]
        public async Task<IActionResult> GetAllAccounts()
        {
            var accounts = await _accountService.GetAllAccountsAsync();
            return Ok(accounts);
        }
        [Authorize(Roles = "Admin")]
        [HttpPatch("admin/assign-role/{id}")]
        public async Task<IActionResult> AssignRole(int id, [FromBody] RoleAssignmentRequestDTO request)
        {
            if (id <= 0)
                return BadRequest("Invalid account ID.");
            var targetAccount = await _accountService.GetAccountByIdAsync(id);
            if (targetAccount == null)
                return NotFound("Account not found.");
            if (targetAccount.RoleID == 3)
            {
                return BadRequest("You can't change the role of another admin.");
            }
            targetAccount.RoleID = request.RoleID;
            bool result = await _accountService.UpdateAccountAsync(targetAccount);
            return result ? Ok("Role assigned successfully.") : StatusCode(500, "Failed to assign role.");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin/{id}")]
        public async Task<IActionResult> GetAccountById(int id)
        {
            var account = await _accountService.GetAccountByIdAsync(id);
            return account != null ? Ok(account) : NotFound("Account not found.");
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("admin/delete/{id}")]
        public async Task<IActionResult> DeleteAccount(int id)
        {
            var account = await _accountService.GetAccountByIdAsync(id);
            bool result = await _accountService.DeleteAccountAsync(id);
            if (result)
            {
                // Send SignalR notification about account inactivation
                await _accountStatusHub.Clients.Group($"Account_{id}")
                    .SendAsync("AccountStatusChanged", new { accountId = id, newStatus = "InActive" });

                // Nếu là SchoolOwner, tắt tất cả live stream đang active
                if (account != null && account.RoleID == 2)
                {
                    var activeStreams = (await _liveStreamService.GetActiveLiveStreamsAsync())
                        .Where(v => v.Program != null && v.Program.SchoolChannel != null && v.Program.SchoolChannel.AccountID == id)
                        .ToList();
                    foreach (var stream in activeStreams)
                    {
                        await _liveStreamService.EndStreamAndReturnLinksAsync(stream);
                    }

                    await _emailService.SendStatusUserAsync(account.Email, "InActive");
                }

                return Ok("Account deleted successfully.");
            }
            return NotFound("Account not found.");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin/pending-schoolowners")]
        public async Task<IActionResult> GetAllPendingSchoolOwners()
        {
            var pendingAccounts = await _accountService.GetAllPendingSchoolOwnerAsync();
            return Ok(pendingAccounts);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin/pending-advertisers")]
        public async Task<IActionResult> GetAllPendingAdvertisers()
        {
            var pendingAccounts = await _accountService.GetAllPendingAdvertiserAsync();
            return Ok(pendingAccounts);
        }

        #endregion

        #region Search Endpoint

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string name)
        {
            if (string.IsNullOrEmpty(name))
                return BadRequest("The 'name' query parameter is required.");
            var accounts = await _accountService.SearchAccountsByNameAsync(name);
            if (accounts == null || accounts.Count == 0)
                return NotFound("No accounts found matching the provided name.");
            return Ok(accounts);
        }

        #endregion
    }

}
