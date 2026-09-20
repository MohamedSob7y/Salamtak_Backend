using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Salamtak.services.Abstractions.Interfaces_Services;
using Salamtak.Shared.Constants;
using Salamtak.Shared.DTOs.Appointments;
using Salamtak.Shared.Pagination;
using Salamtak.Web.Api.Controllers;

namespace Salamtak.Presentation.Controllers
{
    [Route("api/appointments")]
    [ApiController]
    [Authorize]
    public class AppointmentController : BaseApiController
    {
        private readonly IAppointmentService _appointmentService;

        public AppointmentController(
            IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        [HttpPost]
        [Authorize(Roles = Roles.Patient)]
        public async Task<IActionResult> BookAppointment(
            [FromBody] BookAppointmentDto dto)
        {
            var patientUserId =
                GetCurrentUserId();

            var response =
                await _appointmentService
                    .BookAppointmentAsync(
                        patientUserId,
                        dto);

            return Ok(response);
        }

        [HttpPut("cancel")]
        [Authorize(Roles = Roles.Patient + "," + Roles.Doctor)]
        public async Task<IActionResult> CancelAppointment(
            [FromBody] CancelAppointmentDto dto)
        {
            var currentUserId =
                GetCurrentUserId();

            var response =
                await _appointmentService
                    .CancelAppointmentAsync(
                        currentUserId,
                        dto);

            return Ok(response);
        }

        [HttpPut("complete")]
        [Authorize(Roles = Roles.Doctor)]
        public async Task<IActionResult> CompleteAppointment(
            [FromBody] CompleteAppointmentDto dto)
        {
            var doctorUserId =
                GetCurrentUserId();

            var response =
                await _appointmentService
                    .CompleteAppointmentAsync(
                        doctorUserId,
                        dto);

            return Ok(response);
        }

        [HttpGet("{appointmentId:guid}")]
        [Authorize(Roles = Roles.Patient + "," + Roles.Doctor)]
        public async Task<IActionResult> GetAppointmentById(
            Guid appointmentId)
        {
            var currentUserId =
                GetCurrentUserId();

            var response =
                await _appointmentService
                    .GetByIdAsync(
                        currentUserId,
                        appointmentId);

            return Ok(response);
        }

        [HttpGet("patient/me")]
        [Authorize(Roles = Roles.Patient)]
        public async Task<IActionResult> GetMyPatientAppointments()
        {
            var patientUserId =
                GetCurrentUserId();

            var response =
                await _appointmentService
                    .GetPatientAppointmentsAsync(
                        patientUserId);

            return Ok(response);
        }

        [HttpGet("doctor/me")]
        [Authorize(Roles = Roles.Doctor)]
        public async Task<IActionResult> GetMyDoctorAppointments(
            [FromQuery] PaginationParameters pagination)
        {
            var doctorUserId =
                GetCurrentUserId();

            var response =
                await _appointmentService
                    .GetDoctorAppointmentsAsync(
                        doctorUserId,
                        pagination);

            return Ok(response);
        }

        [HttpPut("{appointmentId:guid}/confirm")]
        [Authorize(Roles = Roles.Doctor)]
        public async Task<IActionResult> ConfirmAppointment(
            Guid appointmentId)
        {
            var doctorUserId =
                GetCurrentUserId();

            var response =
                await _appointmentService
                    .ConfirmAppointmentAsync(
                        doctorUserId,
                        appointmentId);

            return Ok(response);
        }

        [HttpPut("{appointmentId:guid}/no-show")]
        [Authorize(Roles = Roles.Doctor)]
        public async Task<IActionResult> MarkAsNoShow(
            Guid appointmentId)
        {
            var doctorUserId =
                GetCurrentUserId();

            var response =
                await _appointmentService
                    .MarkAsNoShowAsync(
                        doctorUserId,
                        appointmentId);

            return Ok(response);
        }
    }
}