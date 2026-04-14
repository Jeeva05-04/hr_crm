using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace hr_crm.Hubs
{
    [Authorize]
    public class LocationHub : Hub
    {
        // HR managers call this to start receiving live location updates
        public async Task JoinManagerGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "managers");
        }

        public async Task LeaveManagerGroup()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "managers");
        }
    }
}


