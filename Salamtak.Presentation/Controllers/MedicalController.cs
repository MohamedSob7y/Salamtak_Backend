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

        private readonly IMedicalReportAttachmentService
            _medicalReportAttachmentService;

        public MedicalReportsController(
            IMedicalReportService medicalReportService,
            IMedicalReportAttachmentService
                medicalReportAttachmentService)
        {
            _medicalReportService =
                medicalReportService;

            _medicalReportAttachmentService =
                medicalReportAttachmentService;
        }

       
        [HttpGet("me")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> GetMyReport()
        {
            var currentUserId =
                GetCurrentUserId();

            var response = await _medicalReportService
                .GetMyReportAsync(currentUserId);

            return Ok(response);
        }

        [HttpGet("appointments/{appointmentId:guid}")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult>
            GetPatientReportForDoctor(
                Guid appointmentId)
        {
            var currentUserId =
                GetCurrentUserId();

            var response = await _medicalReportService
                .GetPatientReportForDoctorAsync(
                    currentUserId,
                    appointmentId);

            return Ok(response);
        }

        [HttpPost("entries")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> AddEntry(
            [FromBody] CreateMedicalReportEntryDto dto)
        {
            var currentUserId =
                GetCurrentUserId();

            var response = await _medicalReportService
                .AddEntryAsync(
                    currentUserId,
                    dto);

            return Ok(response);
        }

        [HttpPut("entries/{entryId:guid}")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> UpdateEntry(
            Guid entryId,
            [FromBody] UpdateMedicalReportEntryDto dto)
        {
            dto.EntryId = entryId;

            var currentUserId =
                GetCurrentUserId();

            var response = await _medicalReportService
                .UpdateEntryAsync(
                    currentUserId,
                    dto);

            return Ok(response);
        }

       
        [HttpPost("me/attachments")]
        [Authorize(Roles = "Patient")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult>
            UploadMyAttachment(
                [FromForm]
                UploadPatientMedicalAttachmentDto dto,
                CancellationToken cancellationToken)
        {
            var currentUserId =
                GetCurrentUserId();

            var response =
                await _medicalReportAttachmentService
                    .UploadPatientAttachmentAsync(
                        currentUserId,
                        dto,
                        cancellationToken);

            return Ok(response);
        }

        
        [HttpGet("me/attachments")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult>
            GetMyAttachments(
                CancellationToken cancellationToken)
        {
            var currentUserId =
                GetCurrentUserId();

            var response =
                await _medicalReportAttachmentService
                    .GetMyAttachmentsAsync(
                        currentUserId,
                        cancellationToken);

            return Ok(response);
        }

        
        [HttpGet( "me/attachments/{attachmentId:guid}/download")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult>
            DownloadMyAttachment(
                Guid attachmentId,
                CancellationToken cancellationToken)
        {
            var currentUserId =
                GetCurrentUserId();

            var result =
                await _medicalReportAttachmentService
                    .DownloadPatientAttachmentAsync(
                        currentUserId,
                        attachmentId,
                        cancellationToken);

            return File(
                result.Stream,
                result.ContentType,
                result.FileName,
                enableRangeProcessing: true);
        }

       
        [HttpDelete(
            "me/attachments/{attachmentId:guid}")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult>
            DeleteMyAttachment(
                Guid attachmentId,
                CancellationToken cancellationToken)
        {
            var currentUserId =
                GetCurrentUserId();

            var response =
                await _medicalReportAttachmentService
                    .DeletePatientAttachmentAsync(
                        currentUserId,
                        attachmentId,
                        cancellationToken);

            return Ok(response);
        }

       
        [HttpPost(
            "entries/{entryId:guid}/attachments")]
        [Authorize(Roles = "Doctor")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult>
            UploadEntryAttachment(
                Guid entryId,
                [FromForm]
                UploadDoctorMedicalAttachmentDto dto,
                CancellationToken cancellationToken)
        {
            var currentUserId =
                GetCurrentUserId();

            var response =
                await _medicalReportAttachmentService
                    .UploadDoctorAttachmentAsync(
                        currentUserId,
                        entryId,
                        dto,
                        cancellationToken);

            return Ok(response);
        }

        [HttpGet(
            "appointments/{appointmentId:guid}/attachments")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult>
            GetPatientAttachmentsForDoctor(
                Guid appointmentId,
                CancellationToken cancellationToken)
        {
            var currentUserId =
                GetCurrentUserId();

            var response =
                await _medicalReportAttachmentService
                    .GetPatientAttachmentsForDoctorAsync(
                        currentUserId,
                        appointmentId,
                        cancellationToken);

            return Ok(response);
        }

       
        [HttpGet(
            "appointments/{appointmentId:guid}/attachments/{attachmentId:guid}/download")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult>
            DownloadPatientAttachmentForDoctor(
                Guid appointmentId,
                Guid attachmentId,
                CancellationToken cancellationToken)
        {
            var currentUserId =
                GetCurrentUserId();

            var result =
                await _medicalReportAttachmentService
                    .DownloadDoctorAttachmentAsync(
                        currentUserId,
                        appointmentId,
                        attachmentId,
                        cancellationToken);

            return File(
                result.Stream,
                result.ContentType,
                result.FileName,
                enableRangeProcessing: true);
        }

        [HttpDelete(
            "doctor/attachments/{attachmentId:guid}")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult>
            DeleteDoctorAttachment(
                Guid attachmentId,
                CancellationToken cancellationToken)
        {
            var currentUserId =
                GetCurrentUserId();

            var response =
                await _medicalReportAttachmentService
                    .DeleteDoctorAttachmentAsync(
                        currentUserId,
                        attachmentId,
                        cancellationToken);

            return Ok(response);
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub")
                ?? User.FindFirstValue("userId")
                ?? User.FindFirstValue("uid");

            if (string.IsNullOrWhiteSpace(
                    userIdClaim) ||
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
