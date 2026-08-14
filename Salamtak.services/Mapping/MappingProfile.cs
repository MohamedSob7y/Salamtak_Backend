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
                .ForMember(
                    destination => destination.SpecialtyId,
                    options => options.MapFrom(
                        source => source.Id));
        }
        private void ClinicMapping()
        {
            CreateMap<Clinic, ClinicDto>()
                .ForMember(
                    destination => destination.ClinicId,
                    options => options.MapFrom(
                        source => source.Id));

            CreateMap<DoctorClinic, DoctorClinicDto>()
                .ForMember(
                    destination =>
                        destination.DoctorClinicId,
                    options => options.MapFrom(
                        source => source.Id))
                .ForMember(
                    destination =>
                        destination.DoctorId,
                    options => options.MapFrom(
                        source => source.DoctorId))
                .ForMember(
                    destination =>
                        destination.ClinicId,
                    options => options.MapFrom(
                        source => source.ClinicId))
                .ForMember(
                    destination =>
                        destination.ClinicName,
                    options => options.MapFrom(
                        source =>
                            source.Clinic != null
                                ? source.Clinic.Name
                                : string.Empty))
                .ForMember(
                    destination =>
                        destination.ClinicAddress,
                    options => options.MapFrom(
                        source =>
                            source.Clinic != null
                                ? source.Clinic.Address
                                : string.Empty))
                .ForMember(
                    destination =>
                        destination.ClinicCity,
                    options => options.MapFrom(
                        source =>
                            source.Clinic != null
                                ? source.Clinic.City
                                : string.Empty))
                .ForMember(
                    destination =>
                        destination.ClinicPhoneNumber,
                    options => options.MapFrom(
                        source =>
                            source.Clinic != null
                                ? source.Clinic.PhoneNumber
                                : null))
                .ForMember(
                    destination =>
                        destination.JoinedAt,
                    options => options.MapFrom(
                        source => source.CreatedAt));
        }
        private void AvailabilitySlotMapping()
        {
            CreateMap<
                    AvailabilitySlot,
                    AvailabilitySlotDto>()
                .ForMember(
                    destination =>
                        destination.SlotId,
                    options => options.MapFrom(
                        source => source.Id));

            CreateMap<
                    CreateAvailabilitySlotDto,
                    AvailabilitySlot>()
                .ForMember(
                    destination =>
                        destination.Id,
                    options => options.Ignore())
                .ForMember(
                    destination =>
                        destination.DoctorId,
                    options => options.Ignore())
                .ForMember(
                    destination =>
                        destination.IsAvailable,
                    options => options.Ignore())
                .ForMember(
                    destination =>
                        destination.CreatedAt,
                    options => options.Ignore())
                .ForMember(
                    destination =>
                        destination.UpdatedAt,
                    options => options.Ignore())
                .ForMember(
                    destination =>
                        destination.IsDeleted,
                    options => options.Ignore())
                .ForMember(
                    destination =>
                        destination.Doctor,
                    options => options.Ignore())
                .ForMember(
                    destination =>
                        destination.Clinic,
                    options => options.Ignore())
                .ForMember(
                    destination =>
                        destination.Appointments,
                    options => options.Ignore());

            CreateMap<
                    UpdateAvailabilitySlotDto,
                    AvailabilitySlot>()
                .ForMember(
                    destination =>
                        destination.Id,
                    options => options.Ignore())
                .ForMember(
                    destination =>
                        destination.DoctorId,
                    options => options.Ignore())
                .ForMember(
                    destination =>
                        destination.ClinicId,
                    options => options.Ignore())
                .ForMember(
                    destination =>
                        destination.IsAvailable,
                    options => options.Ignore())
                .ForMember(
                    destination =>
                        destination.CreatedAt,
                    options => options.Ignore())
                .ForMember(
                    destination =>
                        destination.UpdatedAt,
                    options => options.Ignore())
                .ForMember(
                    destination =>
                        destination.IsDeleted,
                    options => options.Ignore())
                .ForMember(
                    destination =>
                        destination.Doctor,
                    options => options.Ignore())
                .ForMember(
                    destination =>
                        destination.Clinic,
                    options => options.Ignore())
                .ForMember(
                    destination =>
                        destination.Appointments,
                    options => options.Ignore());
        }
        private void AppointmentMapping()
        {
            CreateMap<Appointment, AppointmentDto>()
                .ForMember(
                    destination =>
                        destination.Id,
                    options => options.MapFrom(
                        source => source.Id))
                .ForMember(
                    destination =>
                        destination.PatientId,
                    options => options.MapFrom(
                        source => source.PatientId))
                .ForMember(
                    destination =>
                        destination.DoctorId,
                    options => options.MapFrom(
                        source => source.DoctorId))
                .ForMember(
                    destination =>
                        destination.AppointmentDate,
                    options => options.MapFrom(
                        source =>
                            source.AvailabilitySlot != null
                                ? source.AvailabilitySlot
                                    .StartTime
                                : source.CreatedAt))
                .ForMember(
                    destination =>
                        destination.Status,
                    options => options.MapFrom(
                        source =>
                            source.Status.ToString()));

            CreateMap<
                    Appointment,
                    AppointmentDetailsDto>()
                .ForMember(
                    destination =>
                        destination.Id,
                    options => options.MapFrom(
                        source => source.Id))
                .ForMember(
                    destination =>
                        destination.PatientName,
                    options => options.MapFrom(
                        source =>
                            source.Patient != null &&
                            source.Patient.User != null
                                ? source.Patient.User
                                    .FullName
                                : string.Empty))
                .ForMember(
                    destination =>
                        destination.DoctorName,
                    options => options.MapFrom(
                        source =>
                            source.Doctor != null &&
                            source.Doctor.User != null
                                ? source.Doctor.User
                                    .FullName
                                : string.Empty))
                .ForMember(
                    destination =>
                        destination.SpecialtyName,
                    options => options.MapFrom(
                        source =>
                            source.Doctor != null &&
                            source.Doctor.Specialty != null
                                ? source.Doctor.Specialty
                                    .Name
                                : string.Empty))
                .ForMember(
                    destination =>
                        destination.ClinicName,
                    options => options.MapFrom(
                        source =>
                            source.Clinic != null
                                ? source.Clinic.Name
                                : string.Empty))
                .ForMember(
                    destination =>
                        destination.ClinicAddress,
                    options => options.MapFrom(
                        source =>
                            source.Clinic != null
                                ? source.Clinic.Address
                                : string.Empty))
                .ForMember(
                    destination =>
                        destination.AppointmentDate,
                    options => options.MapFrom(
                        source =>
                            source.AvailabilitySlot != null
                                ? source.AvailabilitySlot
                                    .StartTime
                                : source.CreatedAt))
                .ForMember(
                    destination =>
                        destination.Status,
                    options => options.MapFrom(
                        source =>
                            source.Status.ToString()))
                .ForMember(
                    destination =>
                        destination.Reason,
                    options => options.MapFrom(
                        source =>
                            source.Reason ??
                            string.Empty));

            CreateMap<
                    Appointment,
                    PatientAppointmentDto>()
                .ForMember(
                    destination =>
                        destination.AppointmentId,
                    options => options.MapFrom(
                        source => source.Id))
                .ForMember(
                    destination =>
                        destination.DoctorName,
                    options => options.MapFrom(
                        source =>
                            source.Doctor != null &&
                            source.Doctor.User != null
                                ? source.Doctor.User
                                    .FullName
                                : string.Empty))
                .ForMember(
                    destination =>
                        destination.SpecialtyName,
                    options => options.MapFrom(
                        source =>
                            source.Doctor != null &&
                            source.Doctor.Specialty != null
                                ? source.Doctor.Specialty
                                    .Name
                                : string.Empty))
                .ForMember(
                    destination =>
                        destination.AppointmentDate,
                    options => options.MapFrom(
                        source =>
                            source.AvailabilitySlot != null
                                ? source.AvailabilitySlot
                                    .StartTime
                                : source.CreatedAt))
                .ForMember(
                    destination =>
                        destination.Status,
                    options => options.MapFrom(
                        source =>
                            source.Status.ToString()));

            CreateMap<
                    Appointment,
                    DoctorAppointmentDto>()
                .ForMember(
                    destination =>
                        destination.AppointmentId,
                    options => options.MapFrom(
                        source => source.Id))
                .ForMember(
                    destination =>
                        destination.PatientName,
                    options => options.MapFrom(
                        source =>
                            source.Patient != null &&
                            source.Patient.User != null
                                ? source.Patient.User
                                    .FullName
                                : string.Empty))
                .ForMember(
                    destination =>
                        destination.AppointmentDate,
                    options => options.MapFrom(
                        source =>
                            source.AvailabilitySlot != null
                                ? source.AvailabilitySlot
                                    .StartTime
                                : source.CreatedAt))
                .ForMember(
                    destination =>
                        destination.Status,
                    options => options.MapFrom(
                        source =>
                            source.Status.ToString()))
                .ForMember(
                    destination =>
                        destination.Reason,
                    options => options.MapFrom(
                        source =>
                            source.Reason ??
                            string.Empty));
        }
        private void UserMapping()
        {
            CreateMap<User, UserDto>()
                .ForMember(
                    destination =>
                        destination.UserId,
                    options => options.MapFrom(
                        source => source.Id))
                .ForMember(
                    destination =>
                        destination.Role,
                    options => options.MapFrom(
                        source =>
                            source.Role.ToString()))
                .ForMember(
                    destination =>
                        destination.Status,
                    options => options.MapFrom(
                        source =>
                            source.Status.ToString()))
                .ForMember(
                    destination =>
                        destination.ProfileImageUrl,
                    options =>
                        options.MapFrom<
                            UserProfileImageUrlResolver>());
        }
        private void PatientMapping()
        {
            CreateMap<Patient, PatientProfileDto>()
                .ForMember(
                    destination =>
                        destination.PatientId,
                    options => options.MapFrom(
                        source => source.Id))
                .ForMember(
                    destination =>
                        destination.FullName,
                    options => options.MapFrom(
                        source =>
                            source.User != null
                                ? source.User.FullName
                                : string.Empty))
                .ForMember(
                    destination =>
                        destination.Email,
                    options => options.MapFrom(
                        source =>
                            source.User != null
                                ? source.User.Email
                                : string.Empty))
                .ForMember(
                    destination =>
                        destination.PhoneNumber,
                    options => options.MapFrom(
                        source =>
                            source.User != null
                                ? source.User.PhoneNumber
                                : string.Empty))
                .ForMember(
                    destination =>
                        destination.Gender,
                    options => options.MapFrom(
                        source =>
                            source.Gender.ToString()));

            CreateMap<Patient, PatientSummaryDto>()
                .ForMember(
                    destination =>
                        destination.PatientId,
                    options => options.MapFrom(
                        source => source.Id))
                .ForMember(
                    destination =>
                        destination.FullName,
                    options => options.MapFrom(
                        source =>
                            source.User != null
                                ? source.User.FullName
                                : string.Empty))
                .ForMember(
                    destination =>
                        destination.PhoneNumber,
                    options => options.MapFrom(
                        source =>
                            source.User != null
                                ? source.User.PhoneNumber
                                : string.Empty))
                .ForMember(
                    destination =>
                        destination.Gender,
                    options => options.MapFrom(
                        source =>
                            source.Gender.ToString()));
        }

        private void DoctorMapping()
        {
            CreateMap<Doctor, DoctorCardDto>()
                .ForMember(
                    destination =>
                        destination.DoctorId,
                    options => options.MapFrom(
                        source => source.Id))
                .ForMember(
                    destination =>
                        destination.SpecialtyId,
                    options => options.MapFrom(
                        source => source.SpecialtyId))
                .ForMember(
                    destination =>
                        destination.FullName,
                    options => options.MapFrom(
                        source =>
                            source.User != null
                                ? source.User.FullName
                                : string.Empty))
                .ForMember(
                    destination =>
                        destination.SpecialtyName,
                    options => options.MapFrom(
                        source =>
                            source.Specialty != null
                                ? source.Specialty.Name
                                : string.Empty))
                .ForMember(
                    destination =>
                        destination.NearestClinicId,
                    options => options.MapFrom(
                        source =>
                            source.DoctorClinics
                                .Where(relation =>
                                    !relation.IsDeleted &&
                                    relation.Clinic != null &&
                                    !relation.Clinic.IsDeleted)
                                .Select(relation =>
                                    (Guid?)relation.ClinicId)
                                .FirstOrDefault()))
                .ForMember(
                    destination =>
                        destination.NearestClinicName,
                    options => options.MapFrom(
                        source =>
                            source.DoctorClinics
                                .Where(relation =>
                                    !relation.IsDeleted &&
                                    relation.Clinic != null &&
                                    !relation.Clinic.IsDeleted)
                                .Select(relation =>
                                    relation.Clinic.Name)
                                .FirstOrDefault()))
                .ForMember(
                    destination =>
                        destination.City,
                    options => options.MapFrom(
                        source =>
                            source.DoctorClinics
                                .Where(relation =>
                                    !relation.IsDeleted &&
                                    relation.Clinic != null &&
                                    !relation.Clinic.IsDeleted)
                                .Select(relation =>
                                    relation.Clinic.City)
                                .FirstOrDefault()))
                .ForMember(
                    destination =>
                        destination.ClinicAddress,
                    options => options.MapFrom(
                        source =>
                            source.DoctorClinics
                                .Where(relation =>
                                    !relation.IsDeleted &&
                                    relation.Clinic != null &&
                                    !relation.Clinic.IsDeleted)
                                .Select(relation =>
                                    relation.Clinic.Address)
                                .FirstOrDefault()))
                .ForMember(
                    destination =>
                        destination.DistanceKm,
                    options => options.Ignore())
                .ForMember(
                    destination =>
                        destination.AverageRating,
                    options => options.MapFrom(
                        source =>
                            source.AverageRating))
                .ForMember(
                    destination =>
                        destination.ReviewsCount,
                    options => options.MapFrom(
                        source =>
                            source.Feedbacks.Count(
                                feedback =>
                                    !feedback.IsDeleted)))
                .ForMember(
                    destination =>
                        destination.ConsultationFee,
                    options => options.MapFrom(
                        source =>
                            source.ConsultationFee ?? 0))
                .ForMember(
                    destination =>
                        destination.IsVerified,
                    options => options.MapFrom(
                        source =>
                            source.IsVerified));

            CreateMap<Doctor, DoctorDetailsDto>()
                .ForMember(
                    destination =>
                        destination.DoctorId,
                    options => options.MapFrom(
                        source => source.Id))
                .ForMember(
                    destination =>
                        destination.UserId,
                    options => options.MapFrom(
                        source => source.UserId))
                .ForMember(
                    destination =>
                        destination.FullName,
                    options => options.MapFrom(
                        source =>
                            source.User != null
                                ? source.User.FullName
                                : string.Empty))
                .ForMember(
                    destination =>
                        destination.Email,
                    options => options.MapFrom(
                        source =>
                            source.User != null
                                ? source.User.Email
                                : string.Empty))
                .ForMember(
                    destination =>
                        destination.PhoneNumber,
                    options => options.MapFrom(
                        source =>
                            source.User != null
                                ? source.User.PhoneNumber
                                : string.Empty))
                .ForMember(
                    destination =>
                        destination.SpecialtyId,
                    options => options.MapFrom(
                        source => source.SpecialtyId))
                .ForMember(
                    destination =>
                        destination.SpecialtyName,
                    options => options.MapFrom(
                        source =>
                            source.Specialty != null
                                ? source.Specialty.Name
                                : string.Empty))
                .ForMember(
                    destination =>
                        destination.VerificationStatus,
                    options => options.MapFrom(
                        source =>
                            source.VerificationStatus
                                .ToString()))
                .ForMember(
                    destination =>
                        destination.ReviewsCount,
                    options => options.MapFrom(
                        source =>
                            source.Feedbacks.Count(
                                feedback =>
                                    !feedback.IsDeleted)))
                .ForMember(
                    destination =>
                        destination.ConsultationFee,
                    options => options.MapFrom(
                        source =>
                            source.ConsultationFee ?? 0));

            CreateMap<Doctor, DoctorProfileDto>()
                .ForMember(
                    destination =>
                        destination.DoctorId,
                    options => options.MapFrom(
                        source => source.Id))
                .ForMember(
                    destination =>
                        destination.UserId,
                    options => options.MapFrom(
                        source => source.UserId))
                .ForMember(
                    destination =>
                        destination.FullName,
                    options => options.MapFrom(
                        source =>
                            source.User != null
                                ? source.User.FullName
                                : string.Empty))
                .ForMember(
                    destination =>
                        destination.Email,
                    options => options.MapFrom(
                        source =>
                            source.User != null
                                ? source.User.Email
                                : string.Empty))
                .ForMember(
                    destination =>
                        destination.PhoneNumber,
                    options => options.MapFrom(
                        source =>
                            source.User != null
                                ? source.User.PhoneNumber
                                : string.Empty))
                .ForMember(
                    destination =>
                        destination.SpecialtyId,
                    options => options.MapFrom(
                        source => source.SpecialtyId))
                .ForMember(
                    destination =>
                        destination.SpecialtyName,
                    options => options.MapFrom(
                        source =>
                            source.Specialty != null
                                ? source.Specialty.Name
                                : string.Empty))
                .ForMember(
                    destination =>
                        destination.ConsultationFee,
                    options => options.MapFrom(
                        source =>
                            source.ConsultationFee ?? 0));

            CreateMap<
                    Doctor,
                    DoctorRatingSummaryDto>()
                .ForMember(
                    destination =>
                        destination.DoctorId,
                    options => options.MapFrom(
                        source => source.Id))
                .ForMember(
                    destination =>
                        destination.TotalReviews,
                    options => options.MapFrom(
                        source =>
                            source.Feedbacks.Count(
                                feedback =>
                                    !feedback.IsDeleted)));

            CreateMap<
                    Doctor,
                    DoctorVerificationRequestDto>()
                .ForMember(
                    destination =>
                        destination.DoctorId,
                    options => options.MapFrom(
                        source => source.Id))
                .ForMember(
                    destination =>
                        destination.DoctorName,
                    options => options.MapFrom(
                        source =>
                            source.User != null
                                ? source.User.FullName
                                : string.Empty))
                .ForMember(
                    destination =>
                        destination.Email,
                    options => options.MapFrom(
                        source =>
                            source.User != null
                                ? source.User.Email
                                : string.Empty))
                .ForMember(
                    destination =>
                        destination.PhoneNumber,
                    options => options.MapFrom(
                        source =>
                            source.User != null
                                ? source.User.PhoneNumber
                                : string.Empty))
                .ForMember(
                    destination =>
                        destination.SpecialtyName,
                    options => options.MapFrom(
                        source =>
                            source.Specialty != null
                                ? source.Specialty.Name
                                : string.Empty))
                .ForMember(
                    destination =>
                        destination.VerificationStatus,
                    options => options.MapFrom(
                        source =>
                            source.VerificationStatus
                                .ToString()))
                .ForMember(
                    destination =>
                        destination.DocumentsCount,
                    options => options.MapFrom(
                        source =>
                            source.DoctorDocuments.Count(
                                document =>
                                    !document.IsDeleted)));
        }
       
        private void DoctorDocumentMapping()
        {
            CreateMap<
                    DoctorDocument,
                    DoctorDocumentDto>()
                .ForMember(
                    destination =>
                        destination.DocumentId,
                    options => options.MapFrom(
                        source => source.Id))
                .ForMember(
                    destination =>
                        destination.DoctorName,
                    options => options.MapFrom(
                        source =>
                            source.Doctor != null &&
                            source.Doctor.User != null
                                ? source.Doctor.User
                                    .FullName
                                : string.Empty))
                .ForMember(
                    destination =>
                        destination.DocumentType,
                    options => options.MapFrom(
                        source =>
                            source.DocumentType
                                .ToString()))
                .ForMember(
                    destination =>
                        destination.UploadedAt,
                    options => options.MapFrom(
                        source => source.CreatedAt));
        }

        
        private void FeedbackMapping()
        {
            CreateMap<Feedback, FeedbackDto>()
                .ForMember(
                    destination =>
                        destination.FeedbackId,
                    options => options.MapFrom(
                        source => source.Id))
                .ForMember(
                    destination =>
                        destination.PatientName,
                    options => options.MapFrom(
                        source =>
                            source.Patient != null &&
                            source.Patient.User != null
                                ? source.Patient.User
                                    .FullName
                                : string.Empty))
                .ForMember(
                    destination =>
                        destination.DoctorName,
                    options => options.MapFrom(
                        source =>
                            source.Doctor != null &&
                            source.Doctor.User != null
                                ? source.Doctor.User
                                    .FullName
                                : string.Empty))
                .ForMember(
                    destination =>
                        destination.Comment,
                    options => options.MapFrom(
                        source =>
                            source.Comment ??
                            string.Empty));

            CreateMap<
                    Feedback,
                    DoctorFeedbackDto>()
                .ForMember(
                    destination =>
                        destination.FeedbackId,
                    options => options.MapFrom(
                        source => source.Id))
                .ForMember(
                    destination =>
                        destination.PatientName,
                    options => options.MapFrom(
                        source =>
                            source.Patient != null &&
                            source.Patient.User != null
                                ? source.Patient.User
                                    .FullName
                                : string.Empty))
                .ForMember(
                    destination =>
                        destination.Comment,
                    options => options.MapFrom(
                        source =>
                            source.Comment ??
                            string.Empty));
        }

        
        private void NotificationMapping()
        {
            CreateMap<
                    Notification,
                    NotificationDto>()
                .ForMember(
                    destination =>
                        destination.NotificationId,
                    options => options.MapFrom(
                        source => source.Id))
                .ForMember(
                    destination =>
                        destination.Type,
                    options => options.MapFrom(
                        source =>
                            source.Type.ToString()))
                .ForMember(
                    destination =>
                        destination.Channel,
                    options => options.MapFrom(
                        source =>
                            source.Channel.ToString()))
                .ForMember(
                    destination =>
                        destination.Status,
                    options => options.MapFrom(
                        source =>
                            source.Status.ToString()))
                .ForMember(
                    destination =>
                        destination.RecipientName,
                    options => options.Ignore())
                .ForMember(
                    destination =>
                        destination.RecipientEmail,
                    options => options.Ignore())
                .ForMember(
                    destination =>
                        destination.RecipientRole,
                    options => options.Ignore());

            CreateMap<
                    Notification,
                    RealtimeNotificationDto>()
                .ForMember(
                    destination =>
                        destination.NotificationId,
                    options => options.MapFrom(
                        source => source.Id))
                .ForMember(
                    destination =>
                        destination.Type,
                    options => options.MapFrom(
                        source =>
                            source.Type.ToString()));
        }

        
        private void MedicalReportMapping()
        {
            CreateMap<
                    MedicalReport,
                    MedicalReportDto>()
                .ForMember(
                    destination =>
                        destination.MedicalReportId,
                    options => options.MapFrom(
                        source => source.Id))
                .ForMember(
                    destination =>
                        destination.PatientName,
                    options => options.MapFrom(
                        source =>
                            source.Patient != null &&
                            source.Patient.User != null
                                ? source.Patient.User
                                    .FullName
                                : string.Empty))
                .ForMember(
                    destination =>
                        destination.Attachments,
                    options => options.MapFrom(
                        source =>
                            source.Attachments))
                .ForMember(
                    destination =>
                        destination.Entries,
                    options => options.MapFrom(
                        source =>
                            source.Entries));

            CreateMap<
                    MedicalReportEntry,
                    MedicalReportEntryDto>()
                .ForMember(
                    destination =>
                        destination.EntryId,
                    options => options.MapFrom(
                        source => source.Id))
                .ForMember(
                    destination =>
                        destination.DoctorName,
                    options => options.MapFrom(
                        source =>
                            source.Doctor != null &&
                            source.Doctor.User != null
                                ? source.Doctor.User
                                    .FullName
                                : string.Empty))
                .ForMember(
                    destination =>
                        destination.Prescriptions,
                    options => options.MapFrom(
                        source =>
                            source.Prescriptions))
                .ForMember(
                    destination =>
                        destination.Attachments,
                    options => options.MapFrom(
                        source =>
                            source.Attachments));

            CreateMap<
                    Prescription,
                    PrescriptionDto>()
                .ForMember(
                    destination =>
                        destination.PrescriptionId,
                    options => options.MapFrom(
                        source => source.Id));

            CreateMap<
                    MedicalReportAttachment,
                    MedicalReportAttachmentDto>()
                .ForMember(
                    destination =>
                        destination.UploadedByType,
                    options => options.MapFrom(
                        source =>
                            source.UploadedByType
                                .ToString()));
        }
    }
}