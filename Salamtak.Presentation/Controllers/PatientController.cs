using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Salamtak.services.Abstractions.Interfaces_Services;
using Salamtak.Shared.DTOs.Patients;

namespace Salamtak.Web.Api.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class PatientController : BaseApiController
    {
        private readonly IPatientService _patientService;

        public PatientController(IPatientService patientService)
        {
            _patientService = patientService;
        }

        [HttpGet("{id:guid}")]
        [Authorize(Roles = "Patient,Doctor,Admin")]
        public async Task<IActionResult> GetProfile(Guid id)
        {
            var response = await _patientService.GetProfileAsync(id);
            return Ok(response);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> UpdateProfile(Guid id, [FromBody] UpdatePatientProfileDto dto)
        {
            var response = await _patientService.UpdateProfileAsync(id, dto);
            return Ok(response);
        }

        [HttpGet("{id:guid}/appointments")]
        [Authorize(Roles = "Patient,Doctor,Admin")]
        public async Task<IActionResult> GetAppointments(Guid id)
        {
            var response = await _patientService.GetAppointmentsAsync(id);
            return Ok(response);
        }

        [HttpGet("{id:guid}/medical-history")]
        [Authorize(Roles = "Patient,Doctor")]
        public async Task<IActionResult> GetMedicalHistory(Guid id)
        {
            var response = await _patientService.GetMedicalHistoryAsync(id);
            return Ok(response);
        }
    }
}