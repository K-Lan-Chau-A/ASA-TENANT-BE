using System;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.Interface
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string to, string subject, string body);
        Task<bool> SendOrderConfirmationEmailAsync(string toEmail, string customerName, long orderId, string shopName, string orderDetails, decimal totalPrice, decimal? totalDiscount, decimal finalPrice, DateTime orderDate, string note = null);
        Task<bool> SendLowStockAlertEmailAsync(string toEmail, string productName, int currentQuantity, int threshold, string shopName);
        Task<bool> TestEmailAsync(string toEmail);
    }
}
