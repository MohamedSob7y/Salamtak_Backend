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

            var symptoms = dto.Symptoms.ToLower();

            var keyword = symptoms.Contains("heart") || symptoms.Contains("chest") ? "card" :
                          symptoms.Contains("skin") || symptoms.Contains("rash") || symptoms.Contains("acne") ? "derm" :
                          symptoms.Contains("tooth") || symptoms.Contains("teeth") || symptoms.Contains("gum") ? "dent" :
                          symptoms.Contains("eye") || symptoms.Contains("vision") ? "oph" :
                          symptoms.Contains("bone") || symptoms.Contains("joint") || symptoms.Contains("back") ? "orth" :
                          symptoms.Contains("child") || symptoms.Contains("baby") ? "pedi" :
                          string.Empty;

            Specialty? specialty;

            if (string.IsNullOrWhiteSpace(keyword))
            {
                specialty = (await _unitOfWork.Repository<Specialty>().GetAllAsync()).FirstOrDefault();
            }
            else
            {
                specialty = await _unitOfWork.Repository<Specialty>()
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

            return ApiResponse<AiSpecialtyRecommendationDto>.Ok(result, "Specialty recommendation generated successfully.");
        }

        public async Task<ApiResponse<AiDoctorRecommendationDto>> RecommendDoctorsAsync(AiSymptomRequestDto dto)
        {
            var specialtyResponse = await RecommendSpecialtyAsync(dto);

            if (specialtyResponse.Data is null || specialtyResponse.Data.SpecialtyId is null)
                throw new NotFoundException("No specialty recommendation found.");

            var doctors = await _unitOfWork.Repository<Doctor>()
                .GetAllAsync(d => d.IsVerified && d.SpecialtyId == specialtyResponse.Data.SpecialtyId.Value);

            if (!string.IsNullOrWhiteSpace(dto.City))
            {
                doctors = doctors.Where(d => d.Clinics.Any(c =>
                    c.City.Equals(dto.City.Trim(), StringComparison.OrdinalIgnoreCase))).ToList();
            }

            var doctor = doctors
                .OrderByDescending(d => d.AverageRating)
                .ThenBy(d => d.ConsultationFee)
                .FirstOrDefault();

            if (doctor is null)
                throw new NotFoundException("No doctor recommendation found.");

            var result = new AiDoctorRecommendationDto
            {
                DoctorId = doctor.Id,
                DoctorName = doctor.User?.FullName ?? string.Empty,
                SpecialtyName = doctor.Specialty?.Name ?? specialtyResponse.Data.SpecialtyName,
                Rating = doctor.AverageRating,
                ConsultationFee = doctor.ConsultationFee ?? 0,
                Reason = $"Recommended because the symptoms match {specialtyResponse.Data.SpecialtyName}."
            };

            return ApiResponse<AiDoctorRecommendationDto>.Ok(result, "Doctor recommendation generated successfully.");
        }
    }
}
