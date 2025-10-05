using ASA_TENANT_BE.Hubs;
using ASA_TENANT_SERVICE.Interface;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.Implement
{
    public class RealtimeNotifier : IRealtimeNotifier
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public RealtimeNotifier(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task EmitLowStockAlertToShop(long shopId, object payload)
        {
            await _hubContext.Clients.Group($"Shop_{shopId}").SendAsync("LowStockAlert", payload);
        }
    }
}


