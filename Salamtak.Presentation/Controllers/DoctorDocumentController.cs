using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Salamtak.services.Abstractions.Interfaces_Services;
using Salamtak.Shared.DTOs.DoctorDocuments;

namespace Salamtak.Web.Api.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class DoctorDocumentController : BaseApiController
    {
        private readonly IDoctorDocumentService _doctorDocumentService;

        public DoctorDocumentController(IDoctorDocumentService doctorDocumentService)
        {
            _doctorDocumentService = doctorDocumentService;
        }

        [HttpPost("upload/{doctorId:guid}")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> Upload(Guid doctorId, [FromBody] UploadDoctorDocumentDto dto)
        {
            var response = await _doctorDocumentService.UploadAsync(doctorId, dto);
            return Ok(response);
        }

        [HttpGet("doctor/{doctorId:guid}")]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<IActionResult> GetDoctorDocuments(Guid doctorId)
        {
            var response = await _doctorDocumentService.GetDoctorDocumentsAsync(doctorId);
            return Ok(response);
        }

        [HttpPut("verify")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Verify([FromBody] VerifyDoctorDocumentDto dto)
        {
            var response = await _doctorDocumentService.VerifyAsync(dto);
            return Ok(response);
        }

        [HttpPut("reject")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reject([FromBody] RejectDoctorDocumentDto dto)
        {
            var response = await _doctorDocumentService.RejectAsync(dto);
            return Ok(response);
        }
    }
}