using AutoMapper;
using FluentValidation;
using Salamtak.Domain.Interfaces.UnitOfWork;
using Salamtak.Domain.Models;
using Salamtak.Domain.Models.Enums;
using Salamtak.services.Abstractions.Interfaces_Services;
using Salamtak.services.Exceptions;
using Salamtak.Shared.DTOs.Clinics;
using Salamtak.Shared.Responses;

namespace Salamtak.services.Implementation_Of_Services
{
    public class ClinicService : IClinicService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<CreateClinicDto>
            _createValidator;

        public ClinicService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IValidator<CreateClinicDto> createValidator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _createValidator = createValidator;
        }

        public async Task<
            ApiResponse<IReadOnlyList<ClinicDto>>>
            GetDoctorClinicsAsync(Guid doctorId)
        {
            var doctorExists = await _unitOfWork
                .Repository<Doctor>()
                .AnyAsync(doctor =>
                    doctor.Id == doctorId);

            if (!doctorExists)
            {
                throw new NotFoundException(
                    "Doctor not found.");
            }

            var relations = await _unitOfWork
                .Repository<DoctorClinic>()
                .GetAllAsync(relation =>
                    relation.DoctorId == doctorId);

            var clinicIds = relations
                .Select(relation =>
                    relation.ClinicId)
                .Distinct()
                .ToList();

            if (clinicIds.Count == 0)
            {
                return ApiResponse<
                    IReadOnlyList<ClinicDto>>.Ok(
                        Array.Empty<ClinicDto>());
            }

            var clinics = await _unitOfWork
                .Repository<Clinic>()
                .GetAllAsync(clinic =>
                    clinicIds.Contains(clinic.Id));

            var orderedClinics = clinics
                .OrderBy(clinic => clinic.Name)
                .ThenBy(clinic => clinic.City)
                .ToList();

            var result = _mapper.Map<
                IReadOnlyList<ClinicDto>>(
                    orderedClinics);

            return ApiResponse<
                IReadOnlyList<ClinicDto>>.Ok(
                    result);
        }

        public async Task<
            ApiResponse<IReadOnlyList<ClinicDto>>>
            GetMyClinicsAsync(Guid doctorUserId)
        {
            var doctor =
                await GetDoctorByUserIdAsync(
                    doctorUserId);

            return await GetDoctorClinicsAsync(
                doctor.Id);
        }

        public async Task<ApiResponse<ClinicDto>>
            GetByIdAsync(Guid clinicId)
        {
            var clinic = await _unitOfWork
                .Repository<Clinic>()
                .GetByIdAsync(clinicId);

            if (clinic is null)
            {
                throw new NotFoundException(
                    "Clinic not found.");
            }

            var result =
                _mapper.Map<ClinicDto>(clinic);

            return ApiResponse<ClinicDto>.Ok(
                result);
        }

        public async Task<ApiResponse<ClinicDto>>
            CreateAsync(
                Guid doctorUserId,
                CreateClinicDto dto)
        {
            var validationResult =
                await _createValidator
                    .ValidateAsync(dto);

            if (!validationResult.IsValid)
            {
                throw new AppValidationException(
                    validationResult.Errors
                        .Select(error =>
                            error.ErrorMessage));
            }

            var doctor =
                await GetDoctorByUserIdAsync(
                    doctorUserId);

            EnsureDoctorIsVerified(doctor);

            var name = dto.Name.Trim();
            var address = dto.Address.Trim();
            var city = dto.City.Trim();
            var phoneNumber =
                dto.PhoneNumber.Trim();

            var normalizedName =
                name.ToLower();

            var normalizedAddress =
                address.ToLower();

            var normalizedCity =
                city.ToLower();

            /*
             * نفس اسم العيادة يمكن أن يتكرر في أماكن مختلفة.
             * لكن نفس الاسم + العنوان + المدينة يمثل نفس المكان.
             */
            var existingClinic = await _unitOfWork
                .Repository<Clinic>()
                .FirstOrDefaultAsync(clinic =>
                    clinic.Name.ToLower() ==
                        normalizedName &&
                    clinic.Address.ToLower() ==
                        normalizedAddress &&
                    clinic.City.ToLower() ==
                        normalizedCity);

            if (existingClinic is not null)
            {
                var existingRelation =
                    await _unitOfWork
                        .Repository<DoctorClinic>()
                        .FirstOrDefaultIncludingDeletedAsync(
                            relation =>
                                relation.DoctorId ==
                                    doctor.Id &&
                                relation.ClinicId ==
                                    existingClinic.Id);

                if (existingRelation is not null &&
                    !existingRelation.IsDeleted)
                {
                    throw new ConflictException(
                        "You are already registered at this clinic.");
                }

                throw new ConflictException(
                    $"This clinic already exists with Id '{existingClinic.Id}'. Join the existing clinic instead.");
            }

            var clinic = new Clinic
            {
                Name = name,
                Address = address,
                City = city,
                PhoneNumber = phoneNumber
            };

            var doctorClinic = new DoctorClinic
            {
                DoctorId = doctor.Id,
                ClinicId = clinic.Id,
                Doctor = doctor,
                Clinic = clinic
            };

            await _unitOfWork
                .Repository<Clinic>()
                .AddAsync(clinic);

            await _unitOfWork
                .Repository<DoctorClinic>()
                .AddAsync(doctorClinic);

            await _unitOfWork.SaveChangesAsync();

            var result =
                _mapper.Map<ClinicDto>(clinic);

            return ApiResponse<ClinicDto>.Ok(
                result,
                "Clinic created and linked to doctor successfully.");
        }

        public async Task<ApiResponse>
            JoinClinicAsync(
                Guid doctorUserId,
                Guid clinicId)
        {
            var doctor =
                await GetDoctorByUserIdAsync(
                    doctorUserId);

            EnsureDoctorIsVerified(doctor);

            var clinic = await _unitOfWork
                .Repository<Clinic>()
                .GetByIdAsync(clinicId);

            if (clinic is null)
            {
                throw new NotFoundException(
                    "Clinic not found.");
            }

            /*
             * هنا يجب البحث داخل المحذوف Soft Delete أيضًا.
             * لأن عندنا Unique Index على DoctorId + ClinicId.
             */
            var existingRelation =
                await _unitOfWork
                    .Repository<DoctorClinic>()
                    .FirstOrDefaultIncludingDeletedAsync(
                        relation =>
                            relation.DoctorId ==
                                doctor.Id &&
                            relation.ClinicId ==
                                clinic.Id);

            if (existingRelation is not null)
            {
                if (!existingRelation.IsDeleted)
                {
                    throw new ConflictException(
                        "You are already registered at this clinic.");
                }

                existingRelation.IsDeleted = false;
                existingRelation.UpdatedAt =
                    DateTime.UtcNow;

                _unitOfWork
                    .Repository<DoctorClinic>()
                    .Update(existingRelation);

                await _unitOfWork
                    .SaveChangesAsync();

                return ApiResponse.Ok(
                    "Doctor rejoined clinic successfully.");
            }

            var relation = new DoctorClinic
            {
                DoctorId = doctor.Id,
                ClinicId = clinic.Id
            };

            await _unitOfWork
                .Repository<DoctorClinic>()
                .AddAsync(relation);

            await _unitOfWork
                .SaveChangesAsync();

            return ApiResponse.Ok(
                "Doctor joined clinic successfully.");
        }

        public async Task<ApiResponse>
            LeaveClinicAsync(
                Guid doctorUserId,
                Guid clinicId)
        {
            var doctor =
                await GetDoctorByUserIdAsync(
                    doctorUserId);

            var clinicExists =
                await _unitOfWork
                    .Repository<Clinic>()
                    .AnyAsync(clinic =>
                        clinic.Id == clinicId);

            if (!clinicExists)
            {
                throw new NotFoundException(
                    "Clinic not found.");
            }

            var relation = await _unitOfWork
                .Repository<DoctorClinic>()
                .FirstOrDefaultAsync(item =>
                    item.DoctorId == doctor.Id &&
                    item.ClinicId == clinicId);

            if (relation is null)
            {
                throw new NotFoundException(
                    "Doctor is not registered at this clinic.");
            }

            /*
             * نمنع ترك العيادة عند وجود مواعيد مستقبلية
             * ما زالت متاحة.
             */
            var futureSlots = await _unitOfWork
                .Repository<AvailabilitySlot>()
                .GetAllAsync(slot =>
                    slot.DoctorId == doctor.Id &&
                    slot.ClinicId == clinicId &&
                    slot.StartTime > DateTime.UtcNow);

            var futureSlotList =
                futureSlots.ToList();

            if (futureSlotList.Count > 0)
            {
                var futureSlotIds =
                    futureSlotList
                        .Select(slot => slot.Id)
                        .ToList();

                var hasActiveAppointments =
                    await _unitOfWork
                        .Repository<Appointment>()
                        .AnyAsync(appointment =>
                            appointment.DoctorId ==
                                doctor.Id &&
                            appointment.ClinicId ==
                                clinicId &&
                            futureSlotIds.Contains(
                                appointment
                                    .AvailabilitySlotId) &&
                            (
                                appointment.Status ==
                                    AppointmentStatus.Pending ||
                                appointment.Status ==
                                    AppointmentStatus.Confirmed
                            ));

                if (hasActiveAppointments)
                {
                    throw new ConflictException(
                        "Cannot leave clinic because there are active future appointments at this clinic.");
                }

                throw new ConflictException(
                    "Cannot leave clinic because there are future availability slots. Delete the future slots first.");
            }

            _unitOfWork
                .Repository<DoctorClinic>()
                .SoftDelete(relation);

            await _unitOfWork
                .SaveChangesAsync();

            return ApiResponse.Ok(
                "Doctor left clinic successfully.");
        }

        private async Task<Doctor>
            GetDoctorByUserIdAsync(
                Guid doctorUserId)
        {
            var doctor = await _unitOfWork
                .Repository<Doctor>()
                .FirstOrDefaultAsync(item =>
                    item.UserId == doctorUserId);

            if (doctor is null)
            {
                throw new NotFoundException(
                    "Doctor profile not found.");
            }

            return doctor;
        }

        private static void
            EnsureDoctorIsVerified(
                Doctor doctor)
        {
            if (!doctor.IsVerified ||
                doctor.VerificationStatus !=
                    DoctorVerificationStatus.Verified)
            {
                throw new ForbiddenException(
                    "Doctor must be verified before managing clinics.");
            }
        }
    }
}