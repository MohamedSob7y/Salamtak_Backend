using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Salamtak.Web.Api.Hubs_Real_Time
{
    [Authorize]
    public class NotificationHub : Hub
    {
    }
}
