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
                services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());//To Inject Validators
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
                services.AddScoped<IAdminService, AdminService>();
                services.AddScoped<IJwtService, JwtService>();
                services.AddScoped<IFileStorageService,LocalFileStorageService>();
                services.AddScoped<IProfileImageService,ProfileImageService>();
                services.AddScoped<IAiChatService, GeminiChatService>();
                services.AddScoped<IMedicalReportAttachmentService,MedicalReportAttachmentService>();
                services.AddScoped<IPaymentService, PaymentService>();
                return services;
            }
        }
    }
