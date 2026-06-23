using Microsoft.AspNetCore.Mvc;
using Salamtak.services.Abstractions.Interfaces_Services;
using Salamtak.Shared.DTOs.MedicalReports;

namespace Salamtak.Web.Api.Controllers
{
    [Route("api/medical-reports")]
    [ApiController]
    public class MedicalController : ControllerBase
    {
        private readonly IMedicalReportService _medicalReportService;

        public MedicalController(IMedicalReportService medicalReportService)
        {
            _medicalReportService = medicalReportService;
        }

        [HttpGet("patients/{patientId:guid}")]
        public async Task<IActionResult> GetPatientReport(Guid patientId)
        {
            var response = await _medicalReportService.GetPatientReportAsync(patientId);
            return Ok(response);
        }

        [HttpGet("doctors/{doctorId:guid}/patients/{patientId:guid}/appointments/{appointmentId:guid}")]
        public async Task<IActionResult> GetPatientReportForDoctor(Guid doctorId, Guid patientId, Guid appointmentId)
        {
            var response = await _medicalReportService.GetPatientReportForDoctorAsync(doctorId, patientId, appointmentId);
            return Ok(response);
        }

        [HttpPost("doctors/{doctorId:guid}/entries")]
        public async Task<IActionResult> AddEntry(Guid doctorId, [FromBody] CreateMedicalReportEntryDto dto)
        {
            var response = await _medicalReportService.AddEntryAsync(doctorId, dto);
            return Ok(response);
        }

        [HttpPut("doctors/{doctorId:guid}/entries")]
        public async Task<IActionResult> UpdateEntry(Guid doctorId, [FromBody] UpdateMedicalReportEntryDto dto)
        {
            var response = await _medicalReportService.UpdateEntryAsync(doctorId, dto);
            return Ok(response);
        }
    }
}
