using Microsoft.AspNetCore.Mvc;
using Salamtak.services.Abstractions.Interfaces_Services;
using Salamtak.Shared.DTOs.Appointments;

namespace Salamtak.Web.Api.Controllers
{
    [Route("api/appointments")]
    [ApiController]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;

        public AppointmentController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        [HttpPost("patients/{patientId:guid}")]
        public async Task<IActionResult> BookAppointment(Guid patientId, [FromBody] BookAppointmentDto dto)
        {
            var response = await _appointmentService.BookAppointmentAsync(patientId, dto);
            return Ok(response);
        }

        [HttpPut("cancel/users/{userId:guid}")]
        public async Task<IActionResult> CancelAppointment(Guid userId, [FromBody] CancelAppointmentDto dto)
        {
            var response = await _appointmentService.CancelAppointmentAsync(userId, dto);
            return Ok(response);
        }

        [HttpPut("complete/doctors/{doctorId:guid}")]
        public async Task<IActionResult> CompleteAppointment(Guid doctorId, [FromBody] CompleteAppointmentDto dto)
        {
            var response = await _appointmentService.CompleteAppointmentAsync(doctorId, dto);
            return Ok(response);
        }

        [HttpGet("{appointmentId:guid}")]
        public async Task<IActionResult> GetAppointmentById(Guid appointmentId)
        {
            var response = await _appointmentService.GetByIdAsync(appointmentId);
            return Ok(response);
        }

        [HttpGet("patients/{patientId:guid}")]
        public async Task<IActionResult> GetPatientAppointments(Guid patientId)
        {
            var response = await _appointmentService.GetPatientAppointmentsAsync(patientId);
            return Ok(response);
        }

        [HttpGet("doctors/{doctorId:guid}")]
        public async Task<IActionResult> GetDoctorAppointments(Guid doctorId)
        {
            var response = await _appointmentService.GetDoctorAppointmentsAsync(doctorId);
            return Ok(response);
        }
    }
}
