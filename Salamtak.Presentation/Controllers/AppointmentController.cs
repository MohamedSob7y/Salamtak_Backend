using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Salamtak.Domain.Interfaces.UnitOfWork;
using Salamtak.Domain.Models;
using Salamtak.services.Abstractions.Interfaces_Services;
using Salamtak.Shared.DTOs.Appointments;
using System.Security.Claims;

namespace Salamtak.Presentation.Controllers
{
    [Route("api/appointments")]
    [ApiController]
    [Authorize]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IUnitOfWork _unitOfWork;

        public AppointmentController(
            IAppointmentService appointmentService,
            IUnitOfWork unitOfWork)
        {
            _appointmentService = appointmentService;
            _unitOfWork = unitOfWork;
        }

        [HttpPost]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> BookAppointment([FromBody] BookAppointmentDto dto)
        {
            var userId = GetCurrentUserId();

            var patient = await _unitOfWork.Repository<Patient>()
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (patient is null)
                return Forbid();

            var response = await _appointmentService.BookAppointmentAsync(patient.Id, dto);
            return Ok(response);
        }

        [HttpPut("{appointmentId:guid}/cancel")]
        [Authorize(Roles = "Patient,Doctor")]
        public async Task<IActionResult> CancelAppointment(Guid appointmentId, [FromBody] CancelAppointmentDto dto)
        {
            dto.AppointmentId = appointmentId;

            var userId = GetCurrentUserId();

            var response = await _appointmentService.CancelAppointmentAsync(userId, dto);
            return Ok(response);
        }

        [HttpPut("{appointmentId:guid}/complete")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> CompleteAppointment(Guid appointmentId, [FromBody] CompleteAppointmentDto dto)
        {
            dto.AppointmentId = appointmentId;

            var userId = GetCurrentUserId();

            var doctor = await _unitOfWork.Repository<Doctor>()
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctor is null)
                return Forbid();

            var response = await _appointmentService.CompleteAppointmentAsync(doctor.Id, dto);
            return Ok(response);
        }

        [HttpGet("{appointmentId:guid}")]
        [Authorize(Roles = "Patient,Doctor,Admin")]
        public async Task<IActionResult> GetAppointmentById(Guid appointmentId)
        {
            var appointment = await _unitOfWork.Repository<Appointment>().GetByIdAsync(appointmentId);

            if (appointment is null)
                return NotFound();

            if (!await CanAccessAppointmentAsync(appointment))
                return Forbid();

            var response = await _appointmentService.GetByIdAsync(appointmentId);
            return Ok(response);
        }

        [HttpGet("me/patient")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> GetMyPatientAppointments()
        {
            var userId = GetCurrentUserId();

            var patient = await _unitOfWork.Repository<Patient>()
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (patient is null)
                return Forbid();

            var response = await _appointmentService.GetPatientAppointmentsAsync(patient.Id);
            return Ok(response);
        }

        [HttpGet("me/doctor")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> GetMyDoctorAppointments()
        {
            var userId = GetCurrentUserId();

            var doctor = await _unitOfWork.Repository<Doctor>()
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctor is null)
                return Forbid();

            var response = await _appointmentService.GetDoctorAppointmentsAsync(doctor.Id);
            return Ok(response);
        }

        [HttpGet("patients/{patientId:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPatientAppointments(Guid patientId)
        {
            var response = await _appointmentService.GetPatientAppointmentsAsync(patientId);
            return Ok(response);
        }

        [HttpGet("doctors/{doctorId:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetDoctorAppointments(Guid doctorId)
        {
            var response = await _appointmentService.GetDoctorAppointmentsAsync(doctorId);
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

        private async Task<bool> CanAccessAppointmentAsync(Appointment appointment)
        {
            if (User.IsInRole("Admin"))
                return true;

            var userId = GetCurrentUserId();

            if (User.IsInRole("Patient"))
            {
                var patient = await _unitOfWork.Repository<Patient>()
                    .FirstOrDefaultAsync(p => p.UserId == userId);

                return patient is not null && appointment.PatientId == patient.Id;
            }

            if (User.IsInRole("Doctor"))
            {
                var doctor = await _unitOfWork.Repository<Doctor>()
                    .FirstOrDefaultAsync(d => d.UserId == userId);

                return doctor is not null && appointment.DoctorId == doctor.Id;
            }

            return false;
        }
    }
}
