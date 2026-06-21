using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Salamtak.services.Abstractions.Interfaces_Services;
using Salamtak.Shared.Constants;
using Salamtak.Shared.DTOs.Doctors;

namespace Salamtak.Web.Api.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class DoctorController : BaseApiController
    {
        private readonly IDoctorService _doctorService;

        public DoctorController(IDoctorService doctorService)
        {
            _doctorService = doctorService;
        }
        [HttpGet("search")]
        [Authorize(Roles = Roles.Patient)]
        public async Task<IActionResult> SearchDoctors([FromQuery] DoctorSearchRequestDto dto)
        {
            var response =
                await _doctorService.SearchDoctorsAsync(dto);

            return Ok(response);
        }
        [HttpGet("{doctorId:guid}")]
        [Authorize(Roles = Roles.Patient)]
        public async Task<IActionResult> GetDoctorDetails([FromRoute] Guid doctorId)
        {
            var response =
                await _doctorService.GetDoctorDetailsAsync(doctorId);

            return Ok(response);
        }
        [HttpGet("me")]
        [Authorize(Roles = Roles.Doctor)]
        public async Task<IActionResult> GetMyProfile()
        {
            var doctorUserId = GetCurrentUserId();

            var response =
                await _doctorService.GetProfileAsync(doctorUserId);

            return Ok(response);
        }
        [HttpPut("me")]
        [Authorize(Roles = Roles.Doctor)]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateDoctorProfileDto dto)
        {
            var doctorUserId = GetCurrentUserId();

            var response =
                await _doctorService.UpdateProfileAsync(
                    doctorUserId,
                    dto);

            return Ok(response);
        }
        [HttpGet("me/appointments")]
        [Authorize(Roles = Roles.Doctor)]
        public async Task<IActionResult> GetMyAppointments()
        {
            var doctorUserId = GetCurrentUserId();

            var response =
                await _doctorService.GetAppointmentsAsync(
                    doctorUserId);

            return Ok(response);
        }
        [HttpGet("me/rating-summary")]
        [Authorize(Roles = Roles.Doctor)]
        public async Task<IActionResult> GetMyRatingSummary()
        {
            var doctorUserId = GetCurrentUserId();

            var response =
                await _doctorService.GetRatingSummaryAsync(
                    doctorUserId);

            return Ok(response);
        }
    }
}
