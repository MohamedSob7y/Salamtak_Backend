using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Salamtak.services.Abstractions.Interfaces_Services;
using Salamtak.Shared.DTOs.Auth;
using System.Security.Claims;

namespace Salamtak.Web.Api.Controllers
{
    [Route("api/[controller]")]
    public class AuthController : BaseApiController
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            var response = await _authService.LoginAsync(dto);

            return Ok(response);
        }

        [HttpPost("register-patient")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterPatient([FromBody] RegisterPatientRequestDto dto)
        {
            var response = await _authService.RegisterPatientAsync(dto);

            return Ok(response);
        }



        [HttpPost("register-doctor")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterDoctor([FromBody] RegisterDoctorRequestDto dto)
        {
            var response = await _authService.RegisterDoctorAsync(dto);

            return Ok(response);
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = GetCurrentUserId();

            var response = await _authService.GetCurrentUserAsync(userId);

            return Ok(response);
        }
    }
}
