
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Salamtak.Domain.Interfaces.Repository;
using Salamtak.Domain.Interfaces.UnitOfWork;
using Salamtak.Persistance.Context;
using Salamtak.Persistance.Implementation.Repository;
using Salamtak.Persistance.Implementation.Unite_Of_Work;
using Salamtak.services.Mapping;

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
            builder.Services.AddDbContext<SalamtakDBContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("Default"));
            });
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddAutoMapper(T => T.AddProfile(typeof(MappingProfile)));
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

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();
            #endregion
            //============================================
            #region Run Application
            app.Run();
            #endregion
        }
    }
}
