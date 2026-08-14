using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Salamtak.services.Abstractions.Interfaces_Services;
using Salamtak.Shared.Constants;
using Salamtak.Shared.DTOs.AvailabilitySlots;
using Salamtak.Web.Api.Controllers;

namespace Salamtak.Presentation.Controllers
{
    [Route("api/availability-slots")]
    [Authorize]
    public class AvailabilitySlotsController
        : BaseApiController
    {
        private readonly IAvailabilitySlotService
            _availabilitySlotService;

        public AvailabilitySlotsController(
            IAvailabilitySlotService
                availabilitySlotService)
        {
            _availabilitySlotService =
                availabilitySlotService;
        }

        [HttpGet("doctor/{doctorId:guid}")]
        [Authorize(
            Roles =
                Roles.Patient + "," +
                Roles.Doctor)]
        public async Task<IActionResult>
            GetDoctorAvailableSlots(
                Guid doctorId)
        {
            var response =
                await _availabilitySlotService
                    .GetDoctorAvailableSlotsAsync(
                        doctorId);

            return Ok(response);
        }

        [HttpGet("clinic/{clinicId:guid}")]
        [Authorize(
            Roles =
                Roles.Patient + "," +
                Roles.Doctor)]
        public async Task<IActionResult>
            GetClinicAvailableSlots(
                Guid clinicId)
        {
            var response =
                await _availabilitySlotService
                    .GetClinicAvailableSlotsAsync(
                        clinicId);

            return Ok(response);
        }

        [HttpGet("search")]
        [Authorize(
            Roles =
                Roles.Patient + "," +
                Roles.Doctor)]
        public async Task<IActionResult>
            SearchAvailableSlots(
                [FromQuery]
                AvailableSlotSearchDto dto)
        {
            var response =
                await _availabilitySlotService
                    .SearchAvailableSlotsAsync(dto);

            return Ok(response);
        }

        [HttpGet("{slotId:guid}")]
        [Authorize(
            Roles =
                Roles.Patient + "," +
                Roles.Doctor)]
        public async Task<IActionResult>
            GetById(
                Guid slotId)
        {
            var response =
                await _availabilitySlotService
                    .GetByIdAsync(slotId);

            return Ok(response);
        }

        [HttpPost]
        [Authorize(Roles = Roles.Doctor)]
        public async Task<IActionResult>
            Create(
                [FromBody]
                CreateAvailabilitySlotDto dto)
        {
            var doctorUserId =
                GetCurrentUserId();

            var response =
                await _availabilitySlotService
                    .CreateAsync(
                        doctorUserId,
                        dto);

            return Ok(response);
        }

        [HttpPut("{slotId:guid}")]
        [Authorize(Roles = Roles.Doctor)]
        public async Task<IActionResult>
            Update(
                Guid slotId,
                [FromBody]
                UpdateAvailabilitySlotDto dto)
        {
            var doctorUserId =
                GetCurrentUserId();

            var response =
                await _availabilitySlotService
                    .UpdateAsync(
                        doctorUserId,
                        slotId,
                        dto);

            return Ok(response);
        }

        [HttpDelete("{slotId:guid}")]
        [Authorize(Roles = Roles.Doctor)]
        public async Task<IActionResult>
            Delete(
                Guid slotId)
        {
            var doctorUserId =
                GetCurrentUserId();

            var response =
                await _availabilitySlotService
                    .DeleteAsync(
                        doctorUserId,
                        slotId);

            return Ok(response);
        }

        [HttpPut("{slotId:guid}/available")]
        [Authorize(Roles = Roles.Doctor)]
        public async Task<IActionResult>
            MarkAsAvailable(
                Guid slotId)
        {
            var doctorUserId =
                GetCurrentUserId();

            var response =
                await _availabilitySlotService
                    .MarkAsAvailableAsync(
                        doctorUserId,
                        slotId);

            return Ok(response);
        }

        [HttpPut("{slotId:guid}/unavailable")]
        [Authorize(Roles = Roles.Doctor)]
        public async Task<IActionResult>
            MarkAsUnavailable(
                Guid slotId)
        {
            var doctorUserId =
                GetCurrentUserId();

            var response =
                await _availabilitySlotService
                    .MarkAsUnavailableAsync(
                        doctorUserId,
                        slotId);

            return Ok(response);
        }
    }
}