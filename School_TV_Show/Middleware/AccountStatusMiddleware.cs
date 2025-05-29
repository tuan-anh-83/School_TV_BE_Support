using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Services;
using System.Security.Claims;

namespace School_TV_Show.Middleware
{
    public class AccountStatusMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceProvider _serviceProvider;

        public AccountStatusMiddleware(RequestDelegate next, IServiceProvider serviceProvider)
        {
            _next = next;
            _serviceProvider = serviceProvider;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var accountIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
                if (accountIdClaim != null && int.TryParse(accountIdClaim.Value, out int accountId))
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var accountService = scope.ServiceProvider.GetRequiredService<IAccountService>();
                        var account = await accountService.GetAccountByIdAsync(accountId);
                        if (account != null && !account.Status.Equals("Active", StringComparison.OrdinalIgnoreCase))
                        {
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            await context.Response.WriteAsJsonAsync(new { message = "Account is not active" });
                            return;
                        }
                    }
                }
            }

            await _next(context);
        }
    }
} 