using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Salamtak.Domain.Models;
using Salamtak.services.Abstractions.Interfaces_Services;
using Salamtak.Shared.DTOs.Patients;
using Salamtak.Shared.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Salamtak.Presentation.Controllers
{
    [ApiController]
    [Route("api/patients")]
    [Authorize(Roles = "Patient")]
    [ResponseCache(
       NoStore = true,
       Location = ResponseCacheLocation.None)]
    public class PatientController : ControllerBase
    {
        private readonly IPatientService _patientService;

        public PatientController(
            IPatientService patientService)
        {
            _patientService = patientService
                ?? throw new ArgumentNullException(
                    nameof(patientService));
        }


        [HttpGet("me")]
        [ProducesResponseType(
            typeof(ApiResponse<PatientProfileDto>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ErrorResponse),
            StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(
            typeof(ErrorResponse),
            StatusCodes.Status403Forbidden)]
        [ProducesResponseType(
            typeof(ErrorResponse),
            StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProfile()
        {
            if (!TryGetCurrentUserId(
                    out var patientUserId))
            {
                return Unauthorized(
                    CreateUnauthorizedResponse());
            }

            var response =
                await _patientService.GetProfileAsync(
                    patientUserId);

            return Ok(response);
        }

        [HttpPut("me")]
        [ProducesResponseType(
            typeof(ApiResponse<PatientProfileDto>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ErrorResponse),
            StatusCodes.Status400BadRequest)]
        [ProducesResponseType(
            typeof(ErrorResponse),
            StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(
            typeof(ErrorResponse),
            StatusCodes.Status403Forbidden)]
        [ProducesResponseType(
            typeof(ErrorResponse),
            StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateProfile(
            [FromBody] UpdatePatientProfileDto dto)
        {
            if (!TryGetCurrentUserId(
                    out var patientUserId))
            {
                return Unauthorized(
                    CreateUnauthorizedResponse());
            }

            var response =
                await _patientService.UpdateProfileAsync(
                    patientUserId,
                    dto);

            return Ok(response);
        }


        [HttpGet("me/appointments")]
        [ProducesResponseType(
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ErrorResponse),
            StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(
            typeof(ErrorResponse),
            StatusCodes.Status403Forbidden)]
        [ProducesResponseType(
            typeof(ErrorResponse),
            StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAppointments()
        {
            if (!TryGetCurrentUserId(
                    out var patientUserId))
            {
                return Unauthorized(
                    CreateUnauthorizedResponse());
            }

            var response =
                await _patientService.GetAppointmentsAsync(
                    patientUserId);

            return Ok(response);
        }

        [HttpGet("me/medical-history")]
        [ProducesResponseType(
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ErrorResponse),
            StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(
            typeof(ErrorResponse),
            StatusCodes.Status403Forbidden)]
        [ProducesResponseType(
            typeof(ErrorResponse),
            StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMedicalHistory()
        {
            if (!TryGetCurrentUserId(
                    out var patientUserId))
            {
                return Unauthorized(
                    CreateUnauthorizedResponse());
            }

            var response =
                await _patientService.GetMedicalHistoryAsync(
                    patientUserId);

            return Ok(response);
        }


        private bool TryGetCurrentUserId(
            out Guid currentUserId)
        {
            var userIdClaim =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub")
                ?? User.FindFirstValue("userId")
                ?? User.FindFirstValue("uid");

            return Guid.TryParse(
                userIdClaim,
                out currentUserId);
        }

        private static ErrorResponse
            CreateUnauthorizedResponse()
        {
            return new ErrorResponse
            {
                Success = false,

                StatusCode =
                    StatusCodes.Status401Unauthorized,

                Message =
                    "Invalid or missing user id in token.",

                Errors =
                    new List<string>()
            };
        }
    }
}
