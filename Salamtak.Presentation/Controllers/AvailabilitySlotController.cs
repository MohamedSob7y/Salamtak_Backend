using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Salamtak.services.Abstractions.Interfaces_Services;
using Salamtak.Shared.DTOs.AvailabilitySlots;

namespace Salamtak.Web.Api.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class AvailabilitySlotController : BaseApiController
    {
        private readonly IAvailabilitySlotService _availabilitySlotService;

        public AvailabilitySlotController(IAvailabilitySlotService availabilitySlotService)
        {
            _availabilitySlotService = availabilitySlotService;
        }

        [HttpGet("doctor/{doctorId:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetDoctorAvailableSlots(Guid doctorId)
        {
            var response = await _availabilitySlotService.GetDoctorAvailableSlotsAsync(doctorId);
            return Ok(response);
        }

        [HttpGet("clinic/{clinicId:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetClinicAvailableSlots(Guid clinicId)
        {
            var response = await _availabilitySlotService.GetClinicAvailableSlotsAsync(clinicId);
            return Ok(response);
        }

        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(Guid id)
        {
            var response = await _availabilitySlotService.GetByIdAsync(id);
            return Ok(response);
        }

        [HttpPost("search")]
        [AllowAnonymous]
        public async Task<IActionResult> Search([FromBody] AvailableSlotSearchDto dto)
        {
            var response = await _availabilitySlotService.SearchAvailableSlotsAsync(dto);
            return Ok(response);
        }

        [HttpPost]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> Create([FromBody] CreateAvailabilitySlotDto dto)
        {
            var doctorId = GetCurrentUserId();
            var response = await _availabilitySlotService.CreateAsync(doctorId, dto);
            return Ok(response);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAvailabilitySlotDto dto)
        {
            var doctorId = GetCurrentUserId();
            var response = await _availabilitySlotService.UpdateAsync(doctorId, id, dto);
            return Ok(response);
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var doctorId = GetCurrentUserId();
            var response = await _availabilitySlotService.DeleteAsync(doctorId, id);
            return Ok(response);
        }

        [HttpPatch("{id:guid}/available")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> MarkAsAvailable(Guid id)
        {
            var doctorId = GetCurrentUserId();
            var response = await _availabilitySlotService.MarkAsAvailableAsync(doctorId, id);
            return Ok(response);
        }

        [HttpPatch("{id:guid}/unavailable")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> MarkAsUnavailable(Guid id)
        {
            var doctorId = GetCurrentUserId();
            var response = await _availabilitySlotService.MarkAsUnavailableAsync(doctorId, id);
            return Ok(response);
        }
    }
}