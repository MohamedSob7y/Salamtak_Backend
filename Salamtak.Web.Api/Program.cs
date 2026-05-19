using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Salamtak.Domain.Interfaces.Repository;
using Salamtak.Domain.Interfaces.UnitOfWork;
using Salamtak.Persistance.Context;
using Salamtak.Persistance.Implementation.Repository;
using Salamtak.Persistance.Implementation.Unite_Of_Work;
using Salamtak.services;
using Salamtak.services.Abstractions.Interfaces_Services;
using Salamtak.services.Mapping;
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
            #region Service For Application
            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            #endregion
            //============================================
            #region Inject Dependency
            //Dependency For DbContext+ UnitOfWork + Mapping Profile + Injection For all Methods in Dependency injection File
            builder.Services.AddDbContext<SalamtakDBContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("Default"));
            });
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddApplicationServices();//call Dependency injection File For All Services 
            builder.Services.AddScoped<IRealtimeNotificationService, SignalRRealtimeNotificationService>();
            builder.Services.AddSignalR();
            #endregion
            //============================================
            #region Authorization
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
            #endregion
            //============================================
            #region Connection SignalR With Frontend
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
            #region Build Application on server
            var app = builder.Build();
            #endregion
            //============================================
            #region Configuration using MiddleWare
            // Configure the HTTP request pipeline.
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
