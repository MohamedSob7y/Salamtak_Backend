using AutoMapper;
using FluentValidation;
using Salamtak.Domain.Interfaces.UnitOfWork;
using Salamtak.Domain.Models;
using Salamtak.Domain.Models.Enums;
using Salamtak.services.Abstractions.Interfaces_Services;
using Salamtak.services.Exceptions;
using Salamtak.Shared.DTOs.Appointments;
using Salamtak.Shared.DTOs.MedicalReports;
using Salamtak.Shared.DTOs.Patients;
using Salamtak.Shared.DTOs.Prescriptions;
using Salamtak.Shared.Responses;

namespace Salamtak.services.Implementation_Of_Services
{
    public class PatientService : IPatientService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        private readonly IValidator<UpdatePatientProfileDto>
            _updateValidator;

        public PatientService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IValidator<UpdatePatientProfileDto> updateValidator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _updateValidator = updateValidator;
        }

        
        public async Task<ApiResponse<PatientProfileDto>>
            GetProfileAsync(Guid patientUserId)
        {
            var patient =
                await GetPatientByUserIdAsync(patientUserId);

            var user = await _unitOfWork
                .Repository<User>()
                .FirstOrDefaultAsync(u =>
                    u.Id == patient.UserId &&
                    !u.IsDeleted);

            if (user is null)
            {
                throw new NotFoundException(
                    "Patient user account not found.");
            }

            patient.User = user;

            var result =
                _mapper.Map<PatientProfileDto>(patient);

            return ApiResponse<PatientProfileDto>.Ok(
                result,
                "Patient profile retrieved successfully.");
        }

        public async Task<ApiResponse<PatientProfileDto>>
            UpdateProfileAsync(
                Guid patientUserId,
                UpdatePatientProfileDto dto)
        {
            var validationResult =
                await _updateValidator.ValidateAsync(dto);

            if (!validationResult.IsValid)
            {
                throw new AppValidationException(
                    validationResult.Errors
                        .Select(error =>
                            error.ErrorMessage));
            }

            var patient =
                await GetPatientByUserIdAsync(patientUserId);

            if (!Enum.TryParse<Gender>(
                    dto.Gender?.Trim(),
                    true,
                    out var gender) ||
                !Enum.IsDefined(typeof(Gender), gender))
            {
                throw new BadRequestException(
                    "Invalid gender.");
            }

            patient.DateOfBirth =
                dto.DateOfBirth;

            patient.Gender =
                gender;

            patient.Address =
                NormalizeOptionalText(dto.Address);

            patient.BloodType =
                NormalizeBloodType(dto.BloodType);

            patient.Height =
                dto.Height;

            patient.Weight =
                dto.Weight;

            _unitOfWork
                .Repository<Patient>()
                .Update(patient);

            await _unitOfWork.SaveChangesAsync();

            var user = await _unitOfWork
                .Repository<User>()
                .FirstOrDefaultAsync(u =>
                    u.Id == patient.UserId &&
                    !u.IsDeleted);

            if (user is null)
            {
                throw new NotFoundException(
                    "Patient user account not found.");
            }

            patient.User = user;

            var result =
                _mapper.Map<PatientProfileDto>(patient);

            return ApiResponse<PatientProfileDto>.Ok(
                result,
                "Patient profile updated successfully.");
        }

       
        public async Task<
            ApiResponse<IReadOnlyList<PatientAppointmentDto>>>
            GetAppointmentsAsync(Guid patientUserId)
        {
            var patient =
                await GetPatientByUserIdAsync(patientUserId);

            var appointments = await _unitOfWork
                .Repository<Appointment>()
                .GetAllAsync(a =>
                    a.PatientId == patient.Id &&
                    !a.IsDeleted);

            var appointmentList =
                appointments
                    .OrderByDescending(a => a.CreatedAt)
                    .ToList();

            foreach (var appointment in appointmentList)
            {
                var doctor = await _unitOfWork
                    .Repository<Doctor>()
                    .FirstOrDefaultAsync(d =>
                        d.Id == appointment.DoctorId &&
                        !d.IsDeleted);

                if (doctor is not null)
                {
                    var doctorUser = await _unitOfWork
                        .Repository<User>()
                        .FirstOrDefaultAsync(u =>
                            u.Id == doctor.UserId &&
                            !u.IsDeleted);

                    var specialty = await _unitOfWork
                        .Repository<Specialty>()
                        .FirstOrDefaultAsync(s =>
                            s.Id == doctor.SpecialtyId &&
                            !s.IsDeleted);

                    if (doctorUser is not null)
                    {
                        doctor.User = doctorUser;
                    }

                    if (specialty is not null)
                    {
                        doctor.Specialty = specialty;
                    }

                    appointment.Doctor = doctor;
                }

                var availabilitySlot =
                    await _unitOfWork
                        .Repository<AvailabilitySlot>()
                        .FirstOrDefaultAsync(slot =>
                            slot.Id ==
                                appointment.AvailabilitySlotId &&
                            !slot.IsDeleted);

                if (availabilitySlot is not null)
                {
                    appointment.AvailabilitySlot =
                        availabilitySlot;
                }
            }

            var result =
                _mapper.Map<
                    IReadOnlyList<PatientAppointmentDto>>(
                        appointmentList);

            return ApiResponse<
                IReadOnlyList<PatientAppointmentDto>>.Ok(
                    result,
                    "Patient appointments retrieved successfully.");
        }

      
        public async Task<ApiResponse<MedicalReportDto>>
            GetMedicalHistoryAsync(Guid patientUserId)
        {
            var patient =
                await GetPatientByUserIdAsync(patientUserId);

            var patientUser = await _unitOfWork
                .Repository<User>()
                .FirstOrDefaultAsync(u =>
                    u.Id == patient.UserId &&
                    !u.IsDeleted);

            if (patientUser is null)
            {
                throw new NotFoundException(
                    "Patient user account not found.");
            }

            patient.User =
                patientUser;

            var report = await _unitOfWork
                .Repository<MedicalReport>()
                .FirstOrDefaultAsync(r =>
                    r.PatientId == patient.Id &&
                    !r.IsDeleted);

            if (report is null)
            {
                throw new NotFoundException(
                    "Medical history not found.");
            }

            report.Patient =
                patient;

            var reportAttachments = await _unitOfWork
                .Repository<MedicalReportAttachment>()
                .GetAllAsync(attachment =>
                    attachment.MedicalReportId ==
                        report.Id &&
                    attachment.MedicalReportEntryId == null &&
                    !attachment.IsDeleted);

            report.Attachments =
                reportAttachments
                    .OrderByDescending(a => a.CreatedAt)
                    .ToList();

            var entries = await _unitOfWork
                .Repository<MedicalReportEntry>()
                .GetAllAsync(entry =>
                    entry.MedicalReportId == report.Id &&
                    !entry.IsDeleted);

            var entryList =
                entries
                    .OrderByDescending(e => e.CreatedAt)
                    .ToList();

            foreach (var entry in entryList)
            {
                await LoadEntryRelationsAsync(entry);
            }

            report.Entries =
                entryList;

            var result =
                _mapper.Map<MedicalReportDto>(report);

            return ApiResponse<MedicalReportDto>.Ok(
                result,
                "Medical history retrieved successfully.");
        }

      
        private async Task<Patient>
            GetPatientByUserIdAsync(Guid patientUserId)
        {
            if (patientUserId == Guid.Empty)
            {
                throw new UnauthorizedAccessException(
                    "Invalid patient user id.");
            }

            var patient = await _unitOfWork
                .Repository<Patient>()
                .FirstOrDefaultAsync(p =>
                    p.UserId == patientUserId &&
                    !p.IsDeleted);

            if (patient is null)
            {
                throw new NotFoundException(
                    "Patient profile not found.");
            }

            return patient;
        }

        private async Task LoadEntryRelationsAsync(
            MedicalReportEntry entry)
        {
            var doctor = await _unitOfWork
                .Repository<Doctor>()
                .FirstOrDefaultAsync(d =>
                    d.Id == entry.DoctorId &&
                    !d.IsDeleted);

            if (doctor is not null)
            {
                var doctorUser = await _unitOfWork
                    .Repository<User>()
                    .FirstOrDefaultAsync(u =>
                        u.Id == doctor.UserId &&
                        !u.IsDeleted);

                if (doctorUser is not null)
                {
                    doctor.User =
                        doctorUser;
                }

                entry.Doctor =
                    doctor;
            }

            var prescriptions = await _unitOfWork
                .Repository<Prescription>()
                .GetAllAsync(p =>
                    p.MedicalReportEntryId == entry.Id &&
                    !p.IsDeleted);

            entry.Prescriptions =
                prescriptions
                    .OrderBy(p => p.CreatedAt)
                    .ToList();

            var attachments = await _unitOfWork
                .Repository<MedicalReportAttachment>()
                .GetAllAsync(attachment =>
                    attachment.MedicalReportEntryId ==
                        entry.Id &&
                    !attachment.IsDeleted);

            entry.Attachments =
                attachments
                    .OrderByDescending(a => a.CreatedAt)
                    .ToList();
        }

        private static string?
            NormalizeOptionalText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }

        private static string?
            NormalizeBloodType(string? bloodType)
        {
            return string.IsNullOrWhiteSpace(bloodType)
                ? null
                : bloodType.Trim().ToUpperInvariant();
        }
    }
}
