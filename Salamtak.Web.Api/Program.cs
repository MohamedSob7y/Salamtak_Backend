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
using Salamtak.services.Mapping;
using Salamtak.Web.Api.Extentions;
using Salamtak.Web.Api.Hubs_Real_Time;
using Salamtak.Web.Api.Middlewares;
using Salamtak.Web.Api.Realtime;
using System.Text;
namespace Salamtak.Web.Api
{
    public class Program
    {
        public static void Main(string[] args)
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

            #region DbContext + UnitOfWork + Application Services

            builder.Services.AddDbContext<SalamtakDBContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("Default"));
            });

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            builder.Services.AddApplicationServices();

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

            app.MigrateDatabase();

            app.SeedData();

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

            app.UseCors("AllowFrontend");

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllers();

            app.MapHub<NotificationHub>("/hubs/notifications");

            #endregion

            //============================================

            #region Run Application

            app.Run();

            #endregion
        }
    }
}
