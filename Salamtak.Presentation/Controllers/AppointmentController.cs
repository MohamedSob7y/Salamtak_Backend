using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Salamtak.services.Abstractions.Interfaces_Services;
using Salamtak.Shared.Constants;
using Salamtak.Shared.DTOs.Appointments;
using Salamtak.Web.Api.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        // Patient books an appointment for himself
        [HttpPost]
        [Authorize(Roles = Roles.Patient)]
        public async Task<IActionResult> BookAppointment(
            [FromBody] BookAppointmentDto dto)
        {
            var patientUserId = GetCurrentUserId();

            var response =
                await _appointmentService.BookAppointmentAsync(
                    patientUserId,
                    dto);

            return Ok(response);
        }

        // Patient or Doctor cancels an appointment belonging to him
        [HttpPut("cancel")]
        [Authorize(Roles = Roles.Patient + "," + Roles.Doctor)]
        public async Task<IActionResult> CancelAppointment(
            [FromBody] CancelAppointmentDto dto)
        {
            var currentUserId = GetCurrentUserId();

            var response =
                await _appointmentService.CancelAppointmentAsync(
                    currentUserId,
                    dto);

            return Ok(response);
        }

        // Doctor completes an appointment belonging to him
        [HttpPut("complete")]
        [Authorize(Roles = Roles.Doctor)]
        public async Task<IActionResult> CompleteAppointment(
            [FromBody] CompleteAppointmentDto dto)
        {
            var doctorUserId = GetCurrentUserId();

            var response =
                await _appointmentService.CompleteAppointmentAsync(
                    doctorUserId,
                    dto);

            return Ok(response);
        }

        // Patient or Doctor views an appointment belonging to him
        [HttpGet("{appointmentId:guid}")]
        [Authorize(Roles = Roles.Patient + "," + Roles.Doctor)]
        public async Task<IActionResult> GetAppointmentById(
            [FromRoute] Guid appointmentId)
        {
            var currentUserId = GetCurrentUserId();

            var response =
                await _appointmentService.GetByIdAsync(
                    currentUserId,
                    appointmentId);

            return Ok(response);
        }

        // Patient views only his appointments
        [HttpGet("patient/me")]
        [Authorize(Roles = Roles.Patient)]
        public async Task<IActionResult> GetMyPatientAppointments()
        {
            var patientUserId = GetCurrentUserId();

            var response =
                await _appointmentService
                    .GetPatientAppointmentsAsync(patientUserId);

            return Ok(response);
        }

        // Doctor views only his appointments
        [HttpGet("doctor/me")]
        [Authorize(Roles = Roles.Doctor)]
        public async Task<IActionResult> GetMyDoctorAppointments()
        {
            var doctorUserId = GetCurrentUserId();

            var response =
                await _appointmentService
                    .GetDoctorAppointmentsAsync(doctorUserId);

            return Ok(response);
        }
    }
}
