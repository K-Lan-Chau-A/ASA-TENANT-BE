using ASA_TENANT_REPO.DBContext;
using ASA_TENANT_SERVICE.Interface;
using Microsoft.EntityFrameworkCore;
using Quartz;
using System;

namespace ASA_TENANT_SERVICE.CronJobs
{
    public class OrderExpirationJob : IJob
    {
        private readonly ASATENANTDBContext _context;
        private readonly IOrderService _orderService;

        public OrderExpirationJob(ASATENANTDBContext context, IOrderService orderService)
        {
            _context = context;
            _orderService = orderService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] OrderExpirationJob: Bắt đầu kiểm tra đơn hàng hết hạn...");

                // Lấy thời điểm 5 phút trước
                var expirationTime = DateTime.UtcNow.AddMinutes(-5);

                // Tìm các đơn hàng có status = 0 (chờ thanh toán) và được tạo trước 5 phút
                // Include OrderDetails để có thể hoàn lại tồn kho
                var expiredOrders = await _context.Orders
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Product)
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.ProductUnit)
                    .Where(o => o.Status == 0 && // Chờ thanh toán
                               o.CreatedAt.HasValue && 
                               o.CreatedAt.Value <= expirationTime)
                    .ToListAsync();

                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] OrderExpirationJob: Tìm thấy {expiredOrders.Count} đơn hàng hết hạn");

                int cancelledCount = 0;
                foreach (var order in expiredOrders)
                {
                    try
                    {
                        // Sử dụng CancelOrderAsync để hủy đơn hàng với lý do cụ thể
                        var result = await _orderService.CancelOrderAsync(order.OrderId, "Đơn hàng hết hạn thanh toán sau 5 phút");
                        
                        if (result.Success)
                        {
                            cancelledCount++;
                            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] OrderExpirationJob: Đã hủy đơn hàng ID {order.OrderId}");
                        }
                        else
                        {
                            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] OrderExpirationJob: Lỗi khi hủy đơn hàng ID {order.OrderId}: {result.Message}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] OrderExpirationJob: Exception khi hủy đơn hàng ID {order.OrderId}: {ex.Message}");
                    }
                }

                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] OrderExpirationJob: Hoàn thành. Đã hủy {cancelledCount}/{expiredOrders.Count} đơn hàng hết hạn");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] OrderExpirationJob: Lỗi tổng quát: {ex.Message}");
            }
        }
    }
}
