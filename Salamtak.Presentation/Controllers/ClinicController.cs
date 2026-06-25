using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Salamtak.services.Abstractions.Interfaces_Services;
using Salamtak.Shared.DTOs.Clinics;

namespace Salamtak.Web.Api.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class ClinicController : BaseApiController
    {
        private readonly IClinicService _clinicService;

        public ClinicController(IClinicService clinicService)
        {
            _clinicService = clinicService;
        }

        [HttpGet("doctor/{doctorId:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetDoctorClinics(Guid doctorId)
        {
            var response = await _clinicService.GetDoctorClinicsAsync(doctorId);
            return Ok(response);
        }

        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(Guid id)
        {
            var response = await _clinicService.GetByIdAsync(id);
            return Ok(response);
        }

        [HttpPost]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> Create([FromBody] CreateClinicDto dto)
        {
            var doctorId = GetCurrentUserId();
            var response = await _clinicService.CreateAsync(doctorId, dto);
            return Ok(response);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateClinicDto dto)
        {
            var doctorId = GetCurrentUserId();
            var response = await _clinicService.UpdateAsync(doctorId, id, dto);
            return Ok(response);
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var doctorId = GetCurrentUserId();
            var response = await _clinicService.DeleteAsync(doctorId, id);
            return Ok(response);
        }
    }
}