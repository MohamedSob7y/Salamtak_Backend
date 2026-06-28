using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Salamtak.Domain.Models;
using Salamtak.services.Abstractions.Interfaces_Services;
using Salamtak.Shared.DTOs.MedicalReports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.Presentation.Controllers
{
    [Route("api/medical-reports")]
    [ApiController]
    [Authorize]
    public class MedicalReportsController : ControllerBase
    {
        private readonly IMedicalReportService _medicalReportService;

        public MedicalReportsController(
            IMedicalReportService medicalReportService)
        {
            _medicalReportService = medicalReportService;
        }

        // =========================================================
        // Patient: Get his own medical report
        // GET: api/medical-reports/me
        // =========================================================

        [HttpGet("me")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> GetMyReport()
        {
            var currentUserId = GetCurrentUserId();

            var response = await _medicalReportService
                .GetMyReportAsync(currentUserId);

            return Ok(response);
        }

        // =========================================================
        // Doctor: Get patient report during appointment only
        // GET: api/medical-reports/appointments/{appointmentId}
        // =========================================================

        [HttpGet("appointments/{appointmentId:guid}")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult>
            GetPatientReportForDoctor(Guid appointmentId)
        {
            var currentUserId = GetCurrentUserId();

            var response = await _medicalReportService
                .GetPatientReportForDoctorAsync(
                    currentUserId,
                    appointmentId);

            return Ok(response);
        }

        // =========================================================
        // Doctor: Add medical report entry
        // POST: api/medical-reports/entries
        // =========================================================

        [HttpPost("entries")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> AddEntry(
            [FromBody] CreateMedicalReportEntryDto dto)
        {
            var currentUserId = GetCurrentUserId();

            var response = await _medicalReportService
                .AddEntryAsync(
                    currentUserId,
                    dto);

            return Ok(response);
        }

        // =========================================================
        // Doctor: Update his own medical report entry
        // PUT: api/medical-reports/entries/{entryId}
        // =========================================================

        [HttpPut("entries/{entryId:guid}")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> UpdateEntry(
            Guid entryId,
            [FromBody] UpdateMedicalReportEntryDto dto)
        {
            dto.EntryId = entryId;

            var currentUserId = GetCurrentUserId();

            var response = await _medicalReportService
                .UpdateEntryAsync(
                    currentUserId,
                    dto);

            return Ok(response);
        }

        // =========================================================
        // Get current User.Id from JWT
        // =========================================================

        private Guid GetCurrentUserId()
        {
            var userIdClaim =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub")
                ?? User.FindFirstValue("userId")
                ?? User.FindFirstValue("uid");

            if (string.IsNullOrWhiteSpace(userIdClaim) ||
                !Guid.TryParse(
                    userIdClaim,
                    out var currentUserId))
            {
                throw new UnauthorizedAccessException(
                    "Invalid or missing user id in token.");
            }

            return currentUserId;
        }
    }
}
