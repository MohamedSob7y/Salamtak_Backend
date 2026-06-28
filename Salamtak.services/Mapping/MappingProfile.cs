using AutoMapper;
using Salamtak.Domain.Models;
using Salamtak.Shared.DTOs.Admin;
using Salamtak.Shared.DTOs.Appointments;
using Salamtak.Shared.DTOs.AvailabilitySlots;
using Salamtak.Shared.DTOs.Clinics;
using Salamtak.Shared.DTOs.DoctorDocuments;
using Salamtak.Shared.DTOs.Doctors;
using Salamtak.Shared.DTOs.Feedbacks;
using Salamtak.Shared.DTOs.MedicalReports;
using Salamtak.Shared.DTOs.Notifications;
using Salamtak.Shared.DTOs.Patients;
using Salamtak.Shared.DTOs.Prescriptions;
using Salamtak.Shared.DTOs.Specialties;
using Salamtak.Shared.DTOs.Users;

namespace Salamtak.services.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            SpecialtyMapping();
            ClinicMapping();
            AvailabilitySlotMapping();
            AppointmentMapping();
            UserMapping();
            PatientMapping();
            DoctorMapping();
            DoctorDocumentMapping();
            FeedbackMapping();
            NotificationMapping();
            MedicalReportMapping();
        }

        private void SpecialtyMapping()
        {
            CreateMap<Specialty, SpecialtyDto>()
                .ForMember(dest => dest.SpecialtyId,
                    opt => opt.MapFrom(src => src.Id));
        }

        private void ClinicMapping()
        {
            CreateMap<Clinic, ClinicDto>()
                .ForMember(dest => dest.ClinicId,
                    opt => opt.MapFrom(src => src.Id));
        }

        private void AvailabilitySlotMapping()
        {
            CreateMap<AvailabilitySlot, AvailabilitySlotDto>()
        .ForMember(dest => dest.SlotId,
            opt => opt.MapFrom(src => src.Id));

            CreateMap<CreateAvailabilitySlotDto, AvailabilitySlot>()
                .ForMember(dest => dest.Id,
                    opt => opt.Ignore())
                .ForMember(dest => dest.DoctorId,
                    opt => opt.Ignore())
                .ForMember(dest => dest.Doctor,
                    opt => opt.Ignore())
                .ForMember(dest => dest.Clinic,
                    opt => opt.Ignore())
                .ForMember(dest => dest.Appointment,
                    opt => opt.Ignore())
                .ForMember(dest => dest.IsAvailable,
                    opt => opt.Ignore());

            CreateMap<UpdateAvailabilitySlotDto, AvailabilitySlot>()
                .ForMember(dest => dest.Id,
                    opt => opt.Ignore())
                .ForMember(dest => dest.DoctorId,
                    opt => opt.Ignore())
                .ForMember(dest => dest.ClinicId,
                    opt => opt.Ignore())
                .ForMember(dest => dest.IsAvailable,
                    opt => opt.Ignore())
                .ForMember(dest => dest.Doctor,
                    opt => opt.Ignore())
                .ForMember(dest => dest.Clinic,
                    opt => opt.Ignore())
                .ForMember(dest => dest.Appointment,
                    opt => opt.Ignore());
        }

        private void AppointmentMapping()
        {
            CreateMap<Appointment, AppointmentDto>()
                .ForMember(dest => dest.Id,
                    opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.AppointmentDate,
                    opt => opt.MapFrom(src =>
                        src.AvailabilitySlot != null
                            ? src.AvailabilitySlot.StartTime
                            : src.CreatedAt))
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()));

            CreateMap<Appointment, AppointmentDetailsDto>()
                .ForMember(dest => dest.Id,
                    opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.PatientName,
                    opt => opt.MapFrom(src =>
                        src.Patient != null && src.Patient.User != null
                            ? src.Patient.User.FullName
                            : string.Empty))
                .ForMember(dest => dest.DoctorName,
                    opt => opt.MapFrom(src =>
                        src.Doctor != null && src.Doctor.User != null
                            ? src.Doctor.User.FullName
                            : string.Empty))
                .ForMember(dest => dest.SpecialtyName,
                    opt => opt.MapFrom(src =>
                        src.Doctor != null && src.Doctor.Specialty != null
                            ? src.Doctor.Specialty.Name
                            : string.Empty))
                .ForMember(dest => dest.ClinicName,
                    opt => opt.MapFrom(src =>
                        src.Clinic != null
                            ? src.Clinic.Name
                            : string.Empty))
                .ForMember(dest => dest.ClinicAddress,
                    opt => opt.MapFrom(src =>
                        src.Clinic != null
                            ? src.Clinic.Address
                            : string.Empty))
                .ForMember(dest => dest.AppointmentDate,
                    opt => opt.MapFrom(src =>
                        src.AvailabilitySlot != null
                            ? src.AvailabilitySlot.StartTime
                            : src.CreatedAt))
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.Reason,
                    opt => opt.MapFrom(src => src.Reason ?? string.Empty));

            CreateMap<Appointment, PatientAppointmentDto>()
                .ForMember(dest => dest.AppointmentId,
                    opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.DoctorName,
                    opt => opt.MapFrom(src =>
                        src.Doctor != null && src.Doctor.User != null
                            ? src.Doctor.User.FullName
                            : string.Empty))
                .ForMember(dest => dest.SpecialtyName,
                    opt => opt.MapFrom(src =>
                        src.Doctor != null && src.Doctor.Specialty != null
                            ? src.Doctor.Specialty.Name
                            : string.Empty))
                .ForMember(dest => dest.AppointmentDate,
                    opt => opt.MapFrom(src =>
                        src.AvailabilitySlot != null
                            ? src.AvailabilitySlot.StartTime
                            : src.CreatedAt))
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()));

            //CreateMap<Appointment, DoctorAppointmentDto>()
            //    .ForMember(dest => dest.AppointmentId,
            //        opt => opt.MapFrom(src => src.Id))
            //    .ForMember(dest => dest.PatientName,
            //        opt => opt.MapFrom(src =>
            //            src.Patient != null && src.Patient.User != null
            //                ? src.Patient.User.FullName
            //                : string.Empty))
            //    .ForMember(dest => dest.AppointmentDate,
            //        opt => opt.MapFrom(src =>
            //            src.AvailabilitySlot != null
            //                ? src.AvailabilitySlot.StartTime
            //                : src.CreatedAt))
            //    .ForMember(dest => dest.Status,
            //        opt => opt.MapFrom(src => src.Status.ToString()))
            //    .ForMember(dest => dest.Reason,
            //        opt => opt.MapFrom(src => src.Reason ?? string.Empty));
            CreateMap<Appointment, DoctorAppointmentDto>()
    .ForMember(
        dest => dest.AppointmentId,
        opt => opt.MapFrom(src => src.Id))
    .ForMember(
        dest => dest.PatientName,
        opt => opt.MapFrom(src => src.Patient.User.FullName))
    .ForMember(
        dest => dest.AppointmentDate,
        opt => opt.MapFrom(src => src.AvailabilitySlot.StartTime))
    .ForMember(
        dest => dest.Status,
        opt => opt.MapFrom(src => src.Status.ToString()))
    .ForMember(dest => dest.Reason,opt => opt.MapFrom(src => src.Reason ?? string.Empty));}

        private void UserMapping()
        {
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.UserId,
                    opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Role,
                    opt => opt.MapFrom(src => src.Role.ToString()))
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()));
        }

        private void PatientMapping()
        {
            CreateMap<Patient, PatientProfileDto>()
                .ForMember(dest => dest.PatientId,
                    opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.FullName,
                    opt => opt.MapFrom(src =>
                        src.User != null ? src.User.FullName : string.Empty))
                .ForMember(dest => dest.Email,
                    opt => opt.MapFrom(src =>
                        src.User != null ? src.User.Email : string.Empty))
                .ForMember(dest => dest.PhoneNumber,
                    opt => opt.MapFrom(src =>
                        src.User != null ? src.User.PhoneNumber : string.Empty))
                .ForMember(dest => dest.Gender,
                    opt => opt.MapFrom(src => src.Gender.ToString()));

            CreateMap<Patient, PatientSummaryDto>()
                .ForMember(dest => dest.PatientId,
                    opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.FullName,
                    opt => opt.MapFrom(src =>
                        src.User != null ? src.User.FullName : string.Empty))
                .ForMember(dest => dest.PhoneNumber,
                    opt => opt.MapFrom(src =>
                        src.User != null ? src.User.PhoneNumber : string.Empty))
                .ForMember(dest => dest.Gender,
                    opt => opt.MapFrom(src => src.Gender.ToString()));
        }

        private void DoctorMapping()
        {
            CreateMap<Doctor, DoctorCardDto>()
                .ForMember(dest => dest.DoctorId,
                    opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.FullName,
                    opt => opt.MapFrom(src =>
                        src.User != null ? src.User.FullName : string.Empty))
                .ForMember(dest => dest.SpecialtyName,
                    opt => opt.MapFrom(src =>
                        src.Specialty != null ? src.Specialty.Name : string.Empty))
                .ForMember(dest => dest.City,
                    opt => opt.MapFrom(src =>
                        src.Clinics.Select(c => c.City).FirstOrDefault()))
                .ForMember(dest => dest.ClinicAddress,
                    opt => opt.MapFrom(src =>
                        src.Clinics.Select(c => c.Address).FirstOrDefault()))
                .ForMember(dest => dest.ReviewsCount,
                    opt => opt.MapFrom(src => src.Feedbacks.Count))
                .ForMember(dest => dest.ConsultationFee,
                    opt => opt.MapFrom(src => src.ConsultationFee ?? 0));

            CreateMap<Doctor, DoctorDetailsDto>()
                .ForMember(dest => dest.DoctorId,
                    opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.FullName,
                    opt => opt.MapFrom(src =>
                        src.User != null ? src.User.FullName : string.Empty))
                .ForMember(dest => dest.Email,
                    opt => opt.MapFrom(src =>
                        src.User != null ? src.User.Email : string.Empty))
                .ForMember(dest => dest.PhoneNumber,
                    opt => opt.MapFrom(src =>
                        src.User != null ? src.User.PhoneNumber : string.Empty))
                .ForMember(dest => dest.SpecialtyName,
                    opt => opt.MapFrom(src =>
                        src.Specialty != null ? src.Specialty.Name : string.Empty))
                .ForMember(dest => dest.VerificationStatus,
                    opt => opt.MapFrom(src => src.VerificationStatus.ToString()))
                .ForMember(dest => dest.ReviewsCount,
                    opt => opt.MapFrom(src => src.Feedbacks.Count))
                .ForMember(dest => dest.ConsultationFee,
                    opt => opt.MapFrom(src => src.ConsultationFee ?? 0));

            CreateMap<Doctor, DoctorProfileDto>()
                .ForMember(dest => dest.DoctorId,
                    opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.FullName,
                    opt => opt.MapFrom(src =>
                        src.User != null ? src.User.FullName : string.Empty))
                .ForMember(dest => dest.Email,
                    opt => opt.MapFrom(src =>
                        src.User != null ? src.User.Email : string.Empty))
                .ForMember(dest => dest.PhoneNumber,
                    opt => opt.MapFrom(src =>
                        src.User != null ? src.User.PhoneNumber : string.Empty))
                .ForMember(dest => dest.SpecialtyName,
                    opt => opt.MapFrom(src =>
                        src.Specialty != null ? src.Specialty.Name : string.Empty))
                .ForMember(dest => dest.ConsultationFee,
                    opt => opt.MapFrom(src => src.ConsultationFee ?? 0));

            CreateMap<Doctor, DoctorRatingSummaryDto>()
                .ForMember(dest => dest.DoctorId,
                    opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.TotalReviews,
                    opt => opt.MapFrom(src => src.Feedbacks.Count));

            CreateMap<Doctor, DoctorVerificationRequestDto>()
                .ForMember(dest => dest.DoctorId,
                    opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.DoctorName,
                    opt => opt.MapFrom(src =>
                        src.User != null ? src.User.FullName : string.Empty))
                .ForMember(dest => dest.Email,
                    opt => opt.MapFrom(src =>
                        src.User != null ? src.User.Email : string.Empty))
                .ForMember(dest => dest.PhoneNumber,
                    opt => opt.MapFrom(src =>
                        src.User != null ? src.User.PhoneNumber : string.Empty))
                .ForMember(dest => dest.SpecialtyName,
                    opt => opt.MapFrom(src =>
                        src.Specialty != null ? src.Specialty.Name : string.Empty))
                .ForMember(dest => dest.VerificationStatus,
                    opt => opt.MapFrom(src => src.VerificationStatus.ToString()))
                .ForMember(dest => dest.DocumentsCount,
                    opt => opt.MapFrom(src => src.DoctorDocuments.Count));
        }

        private void DoctorDocumentMapping()
        {
            CreateMap<DoctorDocument, DoctorDocumentDto>()
                .ForMember(dest => dest.DocumentId,
                    opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.DoctorName,
                    opt => opt.MapFrom(src =>
                        src.Doctor != null && src.Doctor.User != null
                            ? src.Doctor.User.FullName
                            : string.Empty))
                .ForMember(dest => dest.DocumentType,
                    opt => opt.MapFrom(src => src.DocumentType.ToString()))
                .ForMember(dest => dest.UploadedAt,
                    opt => opt.MapFrom(src => src.CreatedAt));
        }

        private void FeedbackMapping()
        {
            CreateMap<Feedback, FeedbackDto>()
                .ForMember(dest => dest.FeedbackId,
                    opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.PatientName,
                    opt => opt.MapFrom(src =>
                        src.Patient != null && src.Patient.User != null
                            ? src.Patient.User.FullName
                            : string.Empty))
                .ForMember(dest => dest.DoctorName,
                    opt => opt.MapFrom(src =>
                        src.Doctor != null && src.Doctor.User != null
                            ? src.Doctor.User.FullName
                            : string.Empty))
                .ForMember(dest => dest.Comment,
                    opt => opt.MapFrom(src => src.Comment ?? string.Empty));

            CreateMap<Feedback, DoctorFeedbackDto>()
                .ForMember(dest => dest.FeedbackId,
                    opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.PatientName,
                    opt => opt.MapFrom(src =>
                        src.Patient != null && src.Patient.User != null
                            ? src.Patient.User.FullName
                            : string.Empty))
                .ForMember(dest => dest.Comment,
                    opt => opt.MapFrom(src => src.Comment ?? string.Empty));
        }

        private void NotificationMapping()
        {
            CreateMap<Notification, NotificationDto>()
                .ForMember(dest => dest.NotificationId,
                    opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Type,
                    opt => opt.MapFrom(src => src.Type.ToString()))
                .ForMember(dest => dest.Channel,
                    opt => opt.MapFrom(src => src.Channel.ToString()))
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()));

            CreateMap<Notification, RealtimeNotificationDto>()
                .ForMember(dest => dest.NotificationId,
                    opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Type,
                    opt => opt.MapFrom(src => src.Type.ToString()));
        }

        private void MedicalReportMapping()
        {
            CreateMap<MedicalReport, MedicalReportDto>()
                .ForMember(dest => dest.MedicalReportId,
                    opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.PatientName,
                    opt => opt.MapFrom(src =>
                        src.Patient != null && src.Patient.User != null
                            ? src.Patient.User.FullName
                            : string.Empty))
                .ForMember(dest => dest.Entries,
                    opt => opt.MapFrom(src => src.Entries));

            CreateMap<MedicalReportEntry, MedicalReportEntryDto>()
                .ForMember(dest => dest.EntryId,
                    opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.DoctorName,
                    opt => opt.MapFrom(src =>
                        src.Doctor != null && src.Doctor.User != null
                            ? src.Doctor.User.FullName
                            : string.Empty))
                .ForMember(dest => dest.Prescriptions,
                    opt => opt.MapFrom(src => src.Prescriptions));

            CreateMap<Prescription, PrescriptionDto>()
                .ForMember(dest => dest.PrescriptionId,
                    opt => opt.MapFrom(src => src.Id));
        }
    }
}