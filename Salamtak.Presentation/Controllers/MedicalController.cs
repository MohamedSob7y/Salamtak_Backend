using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Salamtak.Domain.Interfaces.UnitOfWork;
using Salamtak.Domain.Models;
using Salamtak.services.Abstractions.Interfaces_Services;
using Salamtak.Shared.DTOs.MedicalReports;
using System.Security.Claims;

namespace Salamtak.Presentation.Controllers
{
    [Route("api/medical-reports")]
    [ApiController]
    [Authorize]
    public class MedicalController : ControllerBase
    {
        private readonly IMedicalReportService _medicalReportService;
        private readonly IUnitOfWork _unitOfWork;

        public MedicalController(
            IMedicalReportService medicalReportService,
            IUnitOfWork unitOfWork)
        {
            _medicalReportService = medicalReportService;
            _unitOfWork = unitOfWork;
        }

        [HttpGet("me")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> GetMyPatientReport()
        {
            var patient = await GetCurrentPatientAsync();

            if (patient is null)
                return Forbid();

            var response = await _medicalReportService.GetPatientReportAsync(patient.Id);
            return Ok(response);
        }

        [HttpGet("patients/{patientId:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPatientReport(Guid patientId)
        {
            var response = await _medicalReportService.GetPatientReportAsync(patientId);
            return Ok(response);
        }

        [HttpGet("patients/{patientId:guid}/appointments/{appointmentId:guid}")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> GetPatientReportForDoctor(Guid patientId, Guid appointmentId)
        {
            var doctor = await GetCurrentDoctorAsync();

            if (doctor is null)
                return Forbid();

            var response = await _medicalReportService.GetPatientReportForDoctorAsync(doctor.Id, patientId, appointmentId);
            return Ok(response);
        }

        [HttpPost("entries")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> AddEntry([FromBody] CreateMedicalReportEntryDto dto)
        {
            var doctor = await GetCurrentDoctorAsync();

            if (doctor is null)
                return Forbid();

            var response = await _medicalReportService.AddEntryAsync(doctor.Id, dto);
            return Ok(response);
        }

        [HttpPut("entries/{entryId:guid}")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> UpdateEntry(Guid entryId, [FromBody] UpdateMedicalReportEntryDto dto)
        {
            dto.EntryId = entryId;

            var doctor = await GetCurrentDoctorAsync();

            if (doctor is null)
                return Forbid();

            var response = await _medicalReportService.UpdateEntryAsync(doctor.Id, dto);
            return Ok(response);
        }

        private Guid GetCurrentUserId()
        {
            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue("sub") ??
                User.FindFirstValue("userId") ??
                User.FindFirstValue("uid");

            if (!Guid.TryParse(userId, out var parsedUserId))
                throw new UnauthorizedAccessException("Invalid or missing user id in token.");

            return parsedUserId;
        }

        private async Task<Patient?> GetCurrentPatientAsync()
        {
            var userId = GetCurrentUserId();

            return await _unitOfWork.Repository<Patient>()
                .FirstOrDefaultAsync(p => p.UserId == userId);
        }

        private async Task<Doctor?> GetCurrentDoctorAsync()
        {
            var userId = GetCurrentUserId();

            return await _unitOfWork.Repository<Doctor>()
                .FirstOrDefaultAsync(d => d.UserId == userId);
        }
    }
}
