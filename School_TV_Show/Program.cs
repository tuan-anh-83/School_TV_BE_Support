using BLL.Services.LiveStream.Implements;
using BOs.Data;
using BOs.Models;
using DAOs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Net.payOS;
using Repos;
using Services;


using Services.Email;
using Services.HostedServices;
using Services.Hubs;
using Services.Token;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSignalR();

// 🛠 Cấu hình Swagger với hỗ trợ Bearer Token
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "School_TV_Show API",
        Version = "v1",
        Description = "API for School TV Show project with JWT Authentication"
    });


    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Nhập Bearer Token theo format: {your_token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new List<string>()
        }
    });
});

// Repositories
builder.Services.AddScoped<IScheduleRepo, ScheduleRepo>();
builder.Services.AddScoped<IProgramRepo, ProgramRepo>();
//builder.Services.AddScoped<IReportRepo, ReportRepo>();
builder.Services.AddScoped<IAccountRepo, AccountRepo>();
builder.Services.AddScoped<IVideoHistoryRepo, VideoHistoryRepo>();
builder.Services.AddScoped<IVideoViewRepo, VideoViewRepo>();
builder.Services.AddScoped<IVideoLikeRepo, VideoLikeRepo>();
builder.Services.AddScoped<IShareRepo, ShareRepo>();
builder.Services.AddScoped<IPackageRepo, PackageRepo>();
builder.Services.AddScoped<ICommentRepo, CommentRepo>();
builder.Services.AddScoped<ISchoolChannelRepo, SchoolChannelRepo>();
builder.Services.AddScoped<IPaymentRepo, PaymentRepo>();
builder.Services.AddScoped<IOrderRepo, OrderRepo>();
builder.Services.AddScoped<IOrderDetailRepo, OrderDetailRepo>();
builder.Services.AddScoped<INewsRepo, NewsRepo>();
builder.Services.AddScoped<IFollowRepo, FollowRepo>();
builder.Services.AddScoped<IFollowService, FollowService>();
builder.Services.AddScoped<IPaymentHistoryRepo, PaymentHistoryRepo>();
builder.Services.AddScoped<IProgramFollowRepo, ProgramFollowRepo>();
builder.Services.AddScoped<ICategoryNewsRepo, CategoryNewsRepo>();
//builder.Services.AddScoped<ICloudflareUploadService, CloudflareUploadService>();
builder.Services.AddScoped<INotificationRepo, NotificationRepo>();
builder.Services.AddScoped<IAccountPackageRepo, AccountPackageRepo>();


// Services
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IPasswordHasher<Account>, PasswordHasher<Account>>();
builder.Services.AddScoped<IScheduleService, ScheduleService>();
builder.Services.AddScoped<IProgramService, ProgramService>();
//builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<INewsService, NewsService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ISchoolChannelService, SchoolChannelService>();
builder.Services.AddScoped<IOrderDetailService, OrderDetailService>();
builder.Services.AddScoped<IPackageService, PackageService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IVideoHistoryService, VideoHistoryService>();
builder.Services.AddScoped<IVideoViewService, VideoViewService>();
builder.Services.AddScoped<IVideoLikeService, VideoLikeService>();
builder.Services.AddScoped<IShareService, ShareService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddSingleton<ITokenService, TokenService>();
builder.Services.AddHttpClient<ILiveStreamService, LiveStreamService>();
builder.Services.AddScoped<IFollowRepo, FollowRepo>();
builder.Services.AddScoped<IPaymentHistoryService, PaymentHistoryService>();
builder.Services.AddHostedService<PendingAccountReminderService>();
builder.Services.AddHostedService<ExpiredOrderCheckerService>();
builder.Services.AddHostedService<DurationTrackingService>();
builder.Services.AddScoped<IAccountPackageService, AccountPackageService>();
builder.Services.AddScoped<ISchoolChannelFollowService, SchoolChannelFollowsService>();

//builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
//builder.Services.AddProblemDetails();
builder.Services.AddSingleton<OrderTrackingService>();
builder.Services.AddScoped<ILiveStreamRepo, LiveStreamRepo>();
builder.Services.AddHostedService<LiveStreamScheduler>();
builder.Services.AddScoped<IAdScheduleRepo, AdScheduleRepo>();
builder.Services.AddScoped<IAdScheduleService, AdScheduleService>();

builder.Services.AddScoped<IProgramFollowService, ProgramFollowService>();
builder.Services.AddScoped<ICategoryNewsService, CategoryNewsService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ISchoolChannelFollowRepo, SchoolChannelFollowRepo>();


builder.Services.AddHttpClient<ICloudflareUploadService, CloudflareUploadService>();

//DAO
builder.Services.AddScoped<PaymentDAO>();


builder.Services.AddSingleton<OrderTrackingService>();
builder.Services.AddDistributedMemoryCache();

// Register IConfiguration
IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true)
                .Build();

// Configure DbContext
/*builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));*/
// Configure DbContext with Retry Logic
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,                     // số lần thử lại
                maxRetryDelay: TimeSpan.FromSeconds(5), // thời gian chờ giữa các lần
                errorNumbersToAdd: null               // để mặc định
            );
        }));


// Cloudflare configuration
builder.Services.Configure<CloudflareSettings>(builder.Configuration.GetSection("Cloudflare"));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .SetIsOriginAllowed(_ => true) // ✅ Cho tất cả origin
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();           // ✅ Cho phép credentials
    });
});


// CORS Configuration
/*builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        policy => policy.AllowAnyOrigin() // Cho phép tất cả các nguồn
                        .AllowAnyHeader()
                        .AllowAnyMethod());
});*/


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,  // Đảm bảo bật Audience
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"], // Đảm bảo khớp Audience
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
            RoleClaimType = ClaimTypes.Role,
            NameClaimType = ClaimTypes.Name
        };
    });

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
});

var payOSConfig = builder.Configuration.GetSection("Environment");
var clientId = payOSConfig["PAYOS_CLIENT_ID"];
var apiKey = payOSConfig["PAYOS_API_KEY"];
var checksumKey = payOSConfig["PAYOS_CHECKSUM_KEY"];

builder.Services.AddSingleton(new PayOS(clientId, apiKey, checksumKey));
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();
//app.UseCors("AllowAllOrigins"); // Apply CORS policy
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapHub<LiveStreamHub>("/hubs/livestream");
app.MapHub<NotificationHub>("/hubs/notification");
app.MapControllers();
app.Run();
