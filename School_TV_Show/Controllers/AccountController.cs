using BOs.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using School_TV_Show.DTO;
using Services;
using Services.Email;
using Services.Token;
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
        private readonly IPasswordHasher<Account> _passwordHasher;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            IAccountService accountService,
            ITokenService tokenService,
            IEmailService emailService,
            IPasswordHasher<Account> passwordHasher,
            ILogger<AccountController> logger,
            IConfiguration configuration)
        {
            _accountService = accountService;
            _tokenService = tokenService;
            _emailService = emailService;
            _passwordHasher = passwordHasher;
            _logger = logger;
            _configuration = configuration;
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
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
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
                Address = request.Address,
                PhoneNumber = request.PhoneNumber,
                RoleID = 2,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            bool result = await _accountService.SignUpAsync(account);
            if (!result)
                return Conflict("Username or Email already exists and is not eligible for re-registration.");

            var otpCode = new Random().Next(100000, 999999).ToString();
            var expiration = DateTime.UtcNow.AddMinutes(5);
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
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            bool result = await _accountService.SignUpAsync(account);
            if (!result)
                return Conflict("Username or Email already exists.");

            var otpCode = new Random().Next(100000, 999999).ToString();
            var expiration = DateTime.UtcNow.AddMinutes(5);

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
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            bool result = await _accountService.SignUpAsync(account);
            if (!result)
                return Conflict("Username or Email already exists.");

            var otpCode = new Random().Next(100000, 999999).ToString();
            var expiration = DateTime.UtcNow.AddMinutes(5);

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
            if (currentOtp != null && currentOtp.Expiration > DateTime.UtcNow)
            {
                return Ok(new { message = "Your OTP is still active. Please use the existing OTP." });
            }

            var otpCode = new Random().Next(100000, 999999).ToString();
            var expiration = DateTime.UtcNow.AddMinutes(5);

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
            if (account == null || !account.Status.Equals("Active", StringComparison.OrdinalIgnoreCase))
                return Unauthorized("Invalid login information or account is inactive.");
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
        [Authorize(Roles = "User,SchoolOwner,Admin")]
        public async Task<IActionResult> GetAccountInformation()
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
                account.PhoneNumber
            };
            return Ok(accountInfo);
        }

        [HttpPatch("update")]
        [Authorize(Roles = "User,SchoolOwner,Admin")]
        public async Task<IActionResult> UpdateAccount([FromBody] PartialAccountUpdateRequest updateRequest)
        {
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
            if (!string.IsNullOrEmpty(updateRequest.Address))
                account.Address = updateRequest.Address;
            if (!string.IsNullOrEmpty(updateRequest.PhoneNumber))
                account.PhoneNumber = updateRequest.PhoneNumber;
            bool updateResult = await _accountService.UpdateAccountAsync(account);
            if (!updateResult)
                return StatusCode(500, "A problem occurred while processing your request.");
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
        [Authorize(Roles = "User,SchoolOwner,Admin")]
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
            account.Password = BCrypt.Net.BCrypt.HashPassword(changePasswordRequest.NewPassword);
            bool updateResult = await _accountService.UpdateAccountAsync(account);
            if (!updateResult)
                return StatusCode(500, "A problem occurred while processing your request.");
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
            var expiration = DateTime.UtcNow.AddHours(1);
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
            account.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
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
                if (currentOtp != null && currentOtp.Expiration > DateTime.UtcNow)
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
            bool result = await _accountService.UpdateAccountStatusAsync(id, request.Status);
            if (!result)
                return StatusCode(500, "Failed to update account status.");
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
            bool result = await _accountService.DeleteAccountAsync(id);
            return result ? Ok("Account deleted successfully.") : NotFound("Account not found.");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin/pending-schoolowners")]
        public async Task<IActionResult> GetAllPendingSchoolOwners()
        {
            var pendingAccounts = await _accountService.GetAllPendingSchoolOwnerAsync();
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
