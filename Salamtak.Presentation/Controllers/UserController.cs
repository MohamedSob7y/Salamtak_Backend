using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Salamtak.services.Abstractions.Interfaces_Services;
using Salamtak.Shared.DTOs.Users;

namespace Salamtak.Web.Api.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : BaseApiController
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = GetCurrentUserId();

            var response = await _userService
                .GetUserByIdAsync(userId);

            return Ok(response);
        }

        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile(
            [FromBody] UpdateUserProfileDto dto)
        {
            var userId = GetCurrentUserId();

            var response = await _userService
                .UpdateUserProfileAsync(userId, dto);

            return Ok(response);
        }
    }
}
