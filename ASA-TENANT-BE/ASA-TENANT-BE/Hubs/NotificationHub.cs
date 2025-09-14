using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace ASA_TENANT_BE.Hubs
{
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> _logger;
        private static readonly Dictionary<string, string> _userConnections = new();

        public NotificationHub(ILogger<NotificationHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = GetUserId();
            if (!string.IsNullOrEmpty(userId))
            {
                _userConnections[userId] = Context.ConnectionId;
                await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
                _logger.LogInformation($"User {userId} connected with connection ID: {Context.ConnectionId}");
            }
            
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetUserId();
            if (!string.IsNullOrEmpty(userId) && _userConnections.ContainsKey(userId))
            {
                _userConnections.Remove(userId);
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{userId}");
                _logger.LogInformation($"User {userId} disconnected. Connection ID: {Context.ConnectionId}");
            }
            
            await base.OnDisconnectedAsync(exception);
        }

        // Method để gửi thông báo đến user cụ thể
        public async Task SendNotificationToUser(string userId, string message, object? data = null)
        {
            await Clients.Group($"User_{userId}").SendAsync("ReceiveNotification", new
            {
                Message = message,
                Data = data,
                Timestamp = DateTime.UtcNow
            });
        }

        // Method để gửi thông báo đến tất cả users
        public async Task SendNotificationToAll(string message, object? data = null)
        {
            await Clients.All.SendAsync("ReceiveNotification", new
            {
                Message = message,
                Data = data,
                Timestamp = DateTime.UtcNow
            });
        }

        // Method để gửi thông báo đến một group cụ thể
        public async Task SendNotificationToGroup(string groupName, string message, object? data = null)
        {
            await Clients.Group(groupName).SendAsync("ReceiveNotification", new
            {
                Message = message,
                Data = data,
                Timestamp = DateTime.UtcNow
            });
        }

        // Method để join group
        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            _logger.LogInformation($"Connection {Context.ConnectionId} joined group {groupName}");
        }

        // Method để join Shop group
        public async Task JoinShopGroup(long shopId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Shop_{shopId}");
            _logger.LogInformation($"Connection {Context.ConnectionId} joined shop group Shop_{shopId}");
        }

        // Method để join Admin group
        public async Task JoinAdminGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "Admin");
            _logger.LogInformation($"Connection {Context.ConnectionId} joined admin group");
        }

        // Method để leave group
        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            _logger.LogInformation($"Connection {Context.ConnectionId} left group {groupName}");
        }

        // Method để lấy danh sách users đang online
        public async Task GetOnlineUsers()
        {
            var onlineUsers = _userConnections.Keys.ToList();
            await Clients.Caller.SendAsync("OnlineUsers", onlineUsers);
        }

        private string? GetUserId()
        {
            return Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
