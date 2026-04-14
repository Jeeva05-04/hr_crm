using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace hr_crm.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        // Auto-join user's personal group on connect using JWT sub claim
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst("sub")?.Value;
            if (!string.IsNullOrEmpty(userId))
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");

            await base.OnConnectedAsync();
        }
    }
}
