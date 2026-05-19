using FluentValidation;
using Salamtak.Domain.Interfaces.UnitOfWork;
using Salamtak.Domain.Models;
using Salamtak.services.Abstractions.Interfaces_Services;
using Salamtak.services.Exceptions;
using Salamtak.Shared.DTOs.AI;
using Salamtak.Shared.Responses;

namespace Salamtak.services.Implementation_Of_Services
{
    public class AIService : IAIService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<AiSymptomRequestDto> _validator;

        public AIService(
            IUnitOfWork unitOfWork,
            IValidator<AiSymptomRequestDto> validator)
        {
            _unitOfWork = unitOfWork;
            _validator = validator;
        }

        public async Task<ApiResponse<AiSpecialtyRecommendationDto>> RecommendSpecialtyAsync(AiSymptomRequestDto dto)
        {
            var validationResult = await _validator.ValidateAsync(dto);

            if (!validationResult.IsValid)
                throw new AppValidationException(validationResult.Errors.Select(e => e.ErrorMessage));

            var symptoms = dto.Symptoms.Trim().ToLower();

            var keyword = GetSpecialtyKeyword(symptoms);

            Specialty? specialty;

            if (string.IsNullOrWhiteSpace(keyword))
            {
                specialty = (await _unitOfWork
                    .Repository<Specialty>()
                    .GetAllAsync())
                    .FirstOrDefault();
            }
            else
            {
                specialty = await _unitOfWork
                    .Repository<Specialty>()
                    .FirstOrDefaultAsync(s => s.Name.ToLower().Contains(keyword));
            }

            if (specialty is null)
                throw new NotFoundException("No specialty recommendation found.");

            var result = new AiSpecialtyRecommendationDto
            {
                SpecialtyId = specialty.Id,
                SpecialtyName = specialty.Name,
                Confidence = string.IsNullOrWhiteSpace(keyword) ? 0.60 : 0.80
            };

            return ApiResponse<AiSpecialtyRecommendationDto>.Ok(
                result,
                "Specialty recommendation generated successfully.");
        }

        public async Task<ApiResponse<AiDoctorRecommendationDto>> RecommendDoctorsAsync(AiSymptomRequestDto dto)
        {
            var specialtyResponse = await RecommendSpecialtyAsync(dto);

            if (specialtyResponse.Data is null || specialtyResponse.Data.SpecialtyId is null)
                throw new NotFoundException("No specialty recommendation found.");

            var specialtyId = specialtyResponse.Data.SpecialtyId.Value;

            var doctors = await _unitOfWork
                .Repository<Doctor>()
                .GetAllAsync(d => d.IsVerified && d.SpecialtyId == specialtyId);

            if (!doctors.Any())
                throw new NotFoundException("No verified doctors found for this specialty.");

            if (!string.IsNullOrWhiteSpace(dto.City))
            {
                var city = dto.City.Trim().ToLower();

                var clinicsInCity = await _unitOfWork
                    .Repository<Clinic>()
                    .GetAllAsync(c => c.City.ToLower() == city);

                var doctorIdsInCity = clinicsInCity
                    .Select(c => c.DoctorId)
                    .Distinct()
                    .ToHashSet();

                doctors = doctors
                    .Where(d => doctorIdsInCity.Contains(d.Id))
                    .ToList();
            }

            var doctor = doctors
                .OrderByDescending(d => d.AverageRating)
                .ThenBy(d => d.ConsultationFee ?? decimal.MaxValue)
                .FirstOrDefault();

            if (doctor is null)
                throw new NotFoundException("No doctor recommendation found.");

            var doctorUser = await _unitOfWork
                .Repository<User>()
                .GetByIdAsync(doctor.UserId);

            var specialty = await _unitOfWork
                .Repository<Specialty>()
                .GetByIdAsync(doctor.SpecialtyId);

            var result = new AiDoctorRecommendationDto
            {
                DoctorId = doctor.Id,
                DoctorName = doctorUser?.FullName ?? string.Empty,
                SpecialtyName = specialty?.Name ?? specialtyResponse.Data.SpecialtyName,
                Rating = doctor.AverageRating,
                ConsultationFee = doctor.ConsultationFee ?? 0,
                Reason = $"Recommended because the symptoms match {specialtyResponse.Data.SpecialtyName}."
            };

            return ApiResponse<AiDoctorRecommendationDto>.Ok(
                result,
                "Doctor recommendation generated successfully.");
        }

        private static string GetSpecialtyKeyword(string symptoms)
        {
            if (symptoms.Contains("heart") || symptoms.Contains("chest") || symptoms.Contains("cardio"))
                return "card";

            if (symptoms.Contains("skin") || symptoms.Contains("rash") || symptoms.Contains("acne"))
                return "derm";

            if (symptoms.Contains("tooth") || symptoms.Contains("teeth") || symptoms.Contains("gum"))
                return "dent";

            if (symptoms.Contains("eye") || symptoms.Contains("vision"))
                return "oph";

            if (symptoms.Contains("bone") || symptoms.Contains("joint") || symptoms.Contains("back"))
                return "orth";

            if (symptoms.Contains("child") || symptoms.Contains("baby") || symptoms.Contains("pediatric"))
                return "pedi";

            return string.Empty;
        }
    }    //لما اعمل Integeration with Api For ChatBot Delete This sErvice لانه مش RealChat Just For Prototype
}
