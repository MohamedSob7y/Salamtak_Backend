using External_Services.Email;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Salamtak.Domain.Contracts;
using Salamtak.Domain.Interfaces.Repository;
using Salamtak.Domain.Interfaces.UnitOfWork;
using Salamtak.Persistance.Context;
using Salamtak.Persistance.DataSeeding;
using Salamtak.Persistance.Implementation.Repository;
using Salamtak.Persistance.Implementation.Unite_Of_Work;
using Salamtak.services;
using Salamtak.services.Abstractions.Interfaces_Services;
using Salamtak.services.BackgroundServices;
using Salamtak.services.Implementation_Of_Services;
using Salamtak.services.Mapping;
using Salamtak.services.Payments;
using Salamtak.Web.Api.Extentions;
using Salamtak.Web.Api.Hubs_Real_Time;
using Salamtak.Web.Api.Middlewares;
using Salamtak.Web.Api.Realtime;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
namespace Salamtak.Web.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            #region Create Application

            var builder = WebApplication.CreateBuilder(args);

            #endregion

            //============================================

            #region Controllers + Swagger

            builder.Services.AddControllers();

            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Salamtak API",
                    Version = "v1"
                });

               //دا يضيف Authorized in Swagger عشان لما استخدم الTockens
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter JWT token only. Example: eyJhbGciOiJIUzI1..."
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
                        Array.Empty<string>()
                    }
                });
            });

            #endregion

            //============================================

            #region DbContext + UnitOfWork + Application Services+ Value Resolver

            builder.Services.AddDbContext<SalamtakDBContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("Default"));
            });

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddTransient<UserProfileImageUrlResolver>();
            builder.Services.AddApplicationServices();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IPrivateFileStorageService,PrivateFileStorageService>();
            builder.Services.Configure<PaymobOptions>(builder.Configuration.GetSection("Paymob"));
            builder.Services.AddHttpClient<IPaymobClient, PaymobClient>(
    (serviceProvider, client) =>
    {
        var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<PaymobOptions>>().Value;

        client.BaseAddress =
            new Uri(options.BaseUrl);
    });
            builder.Services.AddHostedService<ExpiredAppointmentsBackgroundService>();
            #endregion

            //============================================
            #region Caching + Rate Limiting

            builder.Services.AddOutputCache(options =>
            {
                options.AddPolicy(
                    "PublicShort",
                    policy =>
                        policy
                            .Cache()
                            .Expire(
                                TimeSpan.FromMinutes(2))
                            .Tag("doctor-feedbacks"));
            });

            builder.Services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode =
                    StatusCodes.Status429TooManyRequests;

                options.OnRejected =
                    async (context, cancellationToken) =>
                    {
                        context.HttpContext.Response.ContentType =
                            "application/json";

                        await context.HttpContext.Response.WriteAsJsonAsync(
                            new
                            {
                                success = false,
                                message =
                                    "Too many requests. Please try again later.",
                                statusCode =
                                    StatusCodes.Status429TooManyRequests,
                                errors =
                                    Array.Empty<string>()
                            },
                            cancellationToken);
                    };
                options.AddPolicy(
    "PatientRead",
    httpContext =>
    {
        var userId =
            httpContext.User.FindFirstValue(
                ClaimTypes.NameIdentifier)
            ?? httpContext.User.FindFirstValue("sub")
            ?? httpContext.User.FindFirstValue("userId")
            ?? httpContext.Connection.RemoteIpAddress?.ToString()
            ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: $"patient-read:{userId}",
            factory: _ =>
                new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 60,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                    QueueProcessingOrder =
                        QueueProcessingOrder.OldestFirst,
                    AutoReplenishment = true
                });
    });

                options.AddPolicy(
                    "PatientWrite",
                    httpContext =>
                    {
                        var userId =
                            httpContext.User.FindFirstValue(
                                ClaimTypes.NameIdentifier)
                            ?? httpContext.User.FindFirstValue("sub")
                            ?? httpContext.User.FindFirstValue("userId")
                            ?? httpContext.Connection.RemoteIpAddress?.ToString()
                            ?? "unknown";

                        return RateLimitPartition.GetFixedWindowLimiter(
                            partitionKey: $"patient-write:{userId}",
                            factory: _ =>
                                new FixedWindowRateLimiterOptions
                                {
                                    PermitLimit = 10,
                                    Window = TimeSpan.FromMinutes(1),
                                    QueueLimit = 0,
                                    QueueProcessingOrder =
                                        QueueProcessingOrder.OldestFirst,
                                    AutoReplenishment = true
                                });
                    });
                options.AddPolicy(
                    "FeedbackWrite",
                    httpContext =>
                    {
                        var userId =
                            httpContext.User.FindFirstValue(
                                ClaimTypes.NameIdentifier)
                            ?? httpContext.User.FindFirstValue("sub")
                            ?? httpContext.Connection
                                .RemoteIpAddress?
                                .ToString()
                            ?? "unknown";

                        return RateLimitPartition
                            .GetFixedWindowLimiter(
                                partitionKey:
                                    $"feedback-write:{userId}",

                                factory:
                                    _ =>
                                        new FixedWindowRateLimiterOptions
                                        {
                                            PermitLimit = 5,

                                            Window =
                                                TimeSpan.FromMinutes(1),

                                            QueueLimit = 0,

                                            QueueProcessingOrder =
                                                QueueProcessingOrder
                                                    .OldestFirst,

                                            AutoReplenishment = true
                                        });
                    });

                options.AddPolicy(
                    "FeedbackRead",
                    httpContext =>
                    {
                        var clientKey =
                            httpContext.Connection
                                .RemoteIpAddress?
                                .ToString()
                            ?? "unknown";

                        return RateLimitPartition
                            .GetFixedWindowLimiter(
                                partitionKey:
                                    $"feedback-read:{clientKey}",

                                factory:
                                    _ =>
                                        new FixedWindowRateLimiterOptions
                                        {
                                            PermitLimit = 30,

                                            Window =
                                                TimeSpan.FromMinutes(1),

                                            QueueLimit = 0,

                                            QueueProcessingOrder =
                                                QueueProcessingOrder
                                                    .OldestFirst,

                                            AutoReplenishment = true
                                        });
                    });
            });

            #endregion

            //============================================
            #region SignalR Realtime Notification

            builder.Services.AddScoped<IRealtimeNotificationService, SignalRRealtimeNotificationService>();

            builder.Services.AddSignalR();

            #endregion

            //============================================

            #region Authentication + Authorization

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                var jwtKey = builder.Configuration["Jwt:Key"];

                if (string.IsNullOrWhiteSpace(jwtKey))
                    throw new InvalidOperationException("JWT key is not configured.");

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],

                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtKey)),

                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];

                        var path = context.HttpContext.Request.Path;

                        if (!string.IsNullOrEmpty(accessToken) &&
                            path.StartsWithSegments("/hubs/notifications"))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

            builder.Services.AddAuthorization();

            #endregion

            //============================================

            #region CORS For Angular + SignalR

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy
                        .WithOrigins("http://localhost:4200")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

            #endregion

            //============================================
            #region Data Seeding

            builder.Services.AddScoped<IDataSeeding, DataSeeding>();

            #endregion
            //============================================
            #region Build Application

            var app = builder.Build();

            #endregion

            //============================================

            #region Database Migration + Seeding

            await app.MigrateDatabaseAsync();//Call Extention Method
            await app.SeedDataAsync();//Call ExtentionMethods

            #endregion

            //============================================
            #region Middleware Pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseMiddleware<ExceptionMiddleware>();

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseCors("AllowFrontend");

            app.UseAuthentication();

            app.UseRateLimiter();

            app.UseAuthorization();

            app.UseOutputCache();

            app.MapControllers();

            app.MapHub<NotificationHub>(
                "/hubs/notifications");

            #endregion
            //============================================

            #region Run Application

            app.Run();

            #endregion
        }
    }
}
