using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Salamtak.Web.Api.Controllers
{
    [ApiController]
    public abstract class BaseApiController : ControllerBase
    {
        protected Guid GetCurrentUserId()
        {
            var userIdValue =
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue("UserId");

            if (string.IsNullOrWhiteSpace(userIdValue))
                throw new UnauthorizedAccessException("UserId claim not found.");

            if (!Guid.TryParse(userIdValue, out var userId))
                throw new UnauthorizedAccessException("Invalid UserId claim.");

            return userId;
        }

        protected string GetCurrentUserRole()
        {
            var role =
                User.FindFirstValue(ClaimTypes.Role) ??
                User.FindFirstValue("Role");

            if (string.IsNullOrWhiteSpace(role))
                throw new UnauthorizedAccessException("Role claim not found.");

            return role;
        }
    }
}
