using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Salamtak.Domain.Models;
using Salamtak.services.Abstractions.Interfaces_Services;
using Salamtak.Shared.DTOs.DoctorDocuments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.Presentation.Controllers
{
    [Route("api/doctor-documents")]
    [ApiController]
    [Authorize]
    public class DoctorDocumentController : ControllerBase
    {
        private readonly IDoctorDocumentService
            _doctorDocumentService;

        public DoctorDocumentController(
            IDoctorDocumentService doctorDocumentService)
        {
            _doctorDocumentService =
                doctorDocumentService;
        }

        // =========================================================
        // Doctor Endpoints
        // =========================================================

        /// <summary>
        /// Doctor uploads a private document such as CV,
        /// license, certificate or syndicate card.
        /// </summary>
        [HttpPost("me")]
        [Authorize(Roles = "Doctor")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadMyDocument(
            [FromForm] UploadDoctorDocumentDto dto,
            CancellationToken cancellationToken)
        {
            var currentUserId =
                GetCurrentUserId();

            var response =
                await _doctorDocumentService
                    .UploadMyDocumentAsync(
                        currentUserId,
                        dto,
                        cancellationToken);

            return Ok(response);
        }

        /// <summary>
        /// Doctor gets all his uploaded documents.
        /// </summary>
        [HttpGet("me")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> GetMyDocuments(
            CancellationToken cancellationToken)
        {
            var currentUserId =
                GetCurrentUserId();

            var response =
                await _doctorDocumentService
                    .GetMyDocumentsAsync(
                        currentUserId,
                        cancellationToken);

            return Ok(response);
        }

        /// <summary>
        /// Doctor downloads one of his own documents.
        /// </summary>
        [HttpGet("me/{documentId:guid}/download")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> DownloadMyDocument(
            Guid documentId,
            CancellationToken cancellationToken)
        {
            var currentUserId =
                GetCurrentUserId();

            var result =
                await _doctorDocumentService
                    .DownloadMyDocumentAsync(
                        currentUserId,
                        documentId,
                        cancellationToken);

            return File(
                result.Stream,
                result.ContentType,
                result.FileName,
                enableRangeProcessing: true);
        }

        /// <summary>
        /// Doctor deletes one of his unverified documents.
        /// </summary>
        [HttpDelete("me/{documentId:guid}")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> DeleteMyDocument(
            Guid documentId,
            CancellationToken cancellationToken)
        {
            var currentUserId =
                GetCurrentUserId();

            var response =
                await _doctorDocumentService
                    .DeleteMyDocumentAsync(
                        currentUserId,
                        documentId,
                        cancellationToken);

            return Ok(response);
        }

        // =========================================================
        // Admin Endpoints
        // =========================================================

        /// <summary>
        /// Admin gets all documents for a specific doctor.
        /// </summary>
        [HttpGet("admin/doctors/{doctorId:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult>
            GetDoctorDocumentsForAdmin(
                Guid doctorId,
                CancellationToken cancellationToken)
        {
            var currentUserId =
                GetCurrentUserId();

            var response =
                await _doctorDocumentService
                    .GetDoctorDocumentsForAdminAsync(
                        currentUserId,
                        doctorId,
                        cancellationToken);

            return Ok(response);
        }

        /// <summary>
        /// Admin downloads a doctor document.
        /// </summary>
        [HttpGet(
            "admin/{documentId:guid}/download")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult>
            DownloadDocumentForAdmin(
                Guid documentId,
                CancellationToken cancellationToken)
        {
            var currentUserId =
                GetCurrentUserId();

            var result =
                await _doctorDocumentService
                    .DownloadDocumentForAdminAsync(
                        currentUserId,
                        documentId,
                        cancellationToken);

            return File(
                result.Stream,
                result.ContentType,
                result.FileName,
                enableRangeProcessing: true);
        }

        /// <summary>
        /// Admin approves a doctor document.
        /// </summary>
        [HttpPut(
            "admin/{documentId:guid}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveDocument(
            Guid documentId,
            CancellationToken cancellationToken)
        {
            var currentUserId =
                GetCurrentUserId();

            var response =
                await _doctorDocumentService
                    .ApproveDocumentAsync(
                        currentUserId,
                        documentId,
                        cancellationToken);

            return Ok(response);
        }

        /// <summary>
        /// Admin rejects a doctor document.
        /// </summary>
        [HttpPut(
            "admin/{documentId:guid}/reject")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RejectDocument(
            Guid documentId,
            [FromBody] RejectDoctorDocumentDto dto,
            CancellationToken cancellationToken)
        {
            var currentUserId =
                GetCurrentUserId();

            var response =
                await _doctorDocumentService
                    .RejectDocumentAsync(
                        currentUserId,
                        documentId,
                        dto,
                        cancellationToken);

            return Ok(response);
        }

        // =========================================================
        // Current User
        // =========================================================

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
