using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Salamtak.services.Abstractions.Interfaces_Services;
using Salamtak.Shared.Constants;
using Salamtak.Shared.DTOs.Admin;

namespace Salamtak.Web.Api.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Roles = Roles.Admin)]
    public class AdminController : BaseApiController
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        // GET: api/Admin/pending-doctors
        [HttpGet("pending-doctors")]
        public async Task<IActionResult> GetPendingDoctors()
        {
            var response =
                await _adminService.GetPendingDoctorsAsync();

            return Ok(response);
        }

        // PUT: api/Admin/doctors/verify
        [HttpPut("doctors/verify")]
        public async Task<IActionResult> VerifyDoctor(
            [FromBody] DoctorVerificationResultDto dto)
        {
            var adminUserId = GetCurrentUserId();

            // لا نعتمد على القيمة المرسلة من Client
            dto.IsApproved = true;

            var response =
                await _adminService.VerifyDoctorAsync(
                    adminUserId,
                    dto);

            return Ok(response);
        }

        // PUT: api/Admin/doctors/reject
        [HttpPut("doctors/reject")]
        public async Task<IActionResult> RejectDoctor(
            [FromBody] DoctorVerificationResultDto dto)
        {
            var adminUserId = GetCurrentUserId();

            // لا نعتمد على القيمة المرسلة من Client
            dto.IsApproved = false;

            var response =
                await _adminService.RejectDoctorAsync(
                    adminUserId,
                    dto);

            return Ok(response);
        }

        // GET: api/Admin/users
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var response =
                await _adminService.GetUsersAsync();

            return Ok(response);
        }

        // PUT: api/Admin/users/status
        [HttpPut("users/status")]
        public async Task<IActionResult> UpdateUserStatus(
            [FromBody] UpdateUserStatusDto dto)
        {
            var adminUserId = GetCurrentUserId();

            var response =
                await _adminService.UpdateUserStatusAsync(
                    adminUserId,
                    dto);

            return Ok(response);
        }

        // GET: api/Admin/dashboard-stats
        [HttpGet("dashboard-stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var response =
                await _adminService.GetDashboardStatsAsync();

            return Ok(response);
        }
    }
}
