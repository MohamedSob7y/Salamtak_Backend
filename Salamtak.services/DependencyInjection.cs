using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Salamtak.services.Abstractions.Interfaces_Services;
using Salamtak.services.Implementation_Of_Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.services
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddAutoMapper(Assembly.GetExecutingAssembly());
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IPatientService, PatientService>();
            services.AddScoped<IDoctorService, DoctorService>();
            services.AddScoped<ISpecialtyService, SpecialtyService>();
            services.AddScoped<IClinicService, ClinicService>();
            services.AddScoped<IAvailabilitySlotService, AvailabilitySlotService>();
            services.AddScoped<IAppointmentService, AppointmentService>();
            services.AddScoped<IMedicalReportService, MedicalReportService>();
            services.AddScoped<IDoctorDocumentService, DoctorDocumentService>();
            services.AddScoped<IFeedbackService, FeedbackService>();
            services.AddScoped<INotificationService, NotificationService>();
            //services.AddScoped<IRealtimeNotificationService, RealtimeNotificationService>();
            services.AddScoped<IAdminService, AdminService>();
            services.AddScoped<IAIService, AIService>();
            services.AddScoped<IJwtService, JwtService>();
            return services;
        }
    }
}
