using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Salamtak.Web.Api.Realtime
{
    public class SignalRUserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            return connection.User?.FindFirstValue(ClaimTypes.NameIdentifier)??connection.User?.FindFirstValue("UserId");
        }
    }
}
