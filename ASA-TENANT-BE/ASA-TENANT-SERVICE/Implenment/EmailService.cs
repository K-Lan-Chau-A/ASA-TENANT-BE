using ASA_TENANT_REPO.Models;
using ASA_TENANT_REPO.Repository;
using ASA_TENANT_SERVICE.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.Implenment
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;
        private readonly HttpClient _httpClient;
        private readonly string _fromEmail;
        private readonly string _fromName;
        private readonly string _apiKey;

        public EmailService(IConfiguration config, ILogger<EmailService> logger, HttpClient httpClient)
        {
            _config = config;
            _logger = logger;
            _httpClient = httpClient;

            // Lấy API key & thông tin sender từ ENV trước, sau đó mới tới appsettings
            _apiKey = Environment.GetEnvironmentVariable("BREVO_SETTINGS__APIKEY")
                            ?? _config["BrevoSettings:ApiKey"];

            _fromEmail = Environment.GetEnvironmentVariable("BREVO_SETTINGS__FROMEMAIL")
                            ?? _config["BrevoSettings:FromEmail"];

            _fromName = Environment.GetEnvironmentVariable("BREVO_SETTINGS__FROMNAME")
                            ?? _config["BrevoSettings:FromName"];

            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                _logger.LogError("Brevo API key chưa được cấu hình (BREVO_SETTINGS__APIKEY hoặc BrevoSettings:ApiKey).");
            }
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                _logger.LogError("Brevo API key chưa được cấu hình.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(_fromEmail))
            {
                _logger.LogError("Brevo FromEmail chưa cấu hình (BREVO_SETTINGS__FROMEMAIL hoặc BrevoSettings:FromEmail).");
                return false;
            }

            try
            {
                // Debug logging
                _logger.LogInformation("🔑 Using API Key: {ApiKey}", _apiKey?.Substring(0, Math.Min(20, _apiKey?.Length ?? 0)) + "...");
                _logger.LogInformation("📧 From Email: {FromEmail}", _fromEmail);
                _logger.LogInformation("📧 From Name: {FromName}", _fromName);

                // Tạo request payload cho Brevo REST API
                var requestPayload = new
                {
                    sender = new { name = _fromName, email = _fromEmail },
                    to = new[] { new { email = to } },
                    subject = subject,
                    htmlContent = body
                };

                var json = JsonSerializer.Serialize(requestPayload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Cấu hình headers
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("api-key", _apiKey);

                // Gửi request đến Brevo REST API
                var response = await _httpClient.PostAsync("https://api.brevo.com/v3/smtp/email", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    
                    if (result.TryGetProperty("messageId", out var messageId))
                    {
                        _logger.LogInformation("✅ Email sent to {To}. MessageId: {Mid}", to, messageId.GetString());
                        return true;
                    }
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("❌ Gửi email tới {To} thất bại. Status: {Status}, Response: {Response}", 
                    to, response.StatusCode, errorContent);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception khi gửi email tới {To}", to);
                return false;
            }
        }

        public async Task<bool> SendOrderConfirmationEmailAsync(
            string toEmail, string customerName, long orderId, string shopName,
            string orderDetails, decimal totalPrice, decimal? totalDiscount,
            decimal finalPrice, DateTime orderDate, string note = null)
        {
            var subject = $"Xác nhận đơn hàng #{orderId} - {shopName}";
            var body = $@"
<html>
<body style='font-family:Arial,Helvetica,sans-serif;background:#f6f9fc;padding:24px;'>
  <div style='max-width:640px;margin:0 auto;background:#ffffff;border-radius:12px;box-shadow:0 8px 24px rgba(0,0,0,0.08);overflow:hidden;'>
    <div style='background:linear-gradient(135deg,#4f46e5,#06b6d4);padding:24px 28px;color:#ffffff;'>
      <h2 style='margin:0;font-size:22px;'>Xác nhận đơn hàng thành công!</h2>
      <p style='margin:6px 0 0;opacity:0.95;'>Xin chào {customerName}, cảm ơn bạn đã đặt hàng!</p>
    </div>

    <div style='padding:24px 28px;color:#0f172a;'>
      <p style='margin:0 0 12px;'>Cảm ơn bạn đã đặt hàng tại <strong>{shopName}</strong>!</p>

      <div style='background:#f8fafc;border:1px solid #e2e8f0;border-radius:10px;padding:16px 18px;margin:18px 0;'>
        <h3 style='margin:0 0 10px;font-size:16px;color:#334155;'>Thông tin đơn hàng</h3>
        <div style='display:flex;gap:12px;flex-wrap:wrap;'>
          <div style='flex:1 1 240px;background:#ffffff;border:1px solid #e2e8f0;border-radius:8px;padding:10px 12px;'>
            <div style='font-size:12px;color:#64748b;text-transform:uppercase;letter-spacing:0.4px;'>Mã đơn hàng</div>
            <div style='font-weight:600;color:#0f172a;margin-top:4px;'>#{orderId}</div>
          </div>
          <div style='flex:1 1 240px;background:#ffffff;border:1px solid #e2e8f0;border-radius:8px;padding:10px 12px;'>
            <div style='font-size:12px;color:#64748b;text-transform:uppercase;letter-spacing:0.4px;'>Ngày đặt hàng</div>
            <div style='font-weight:600;color:#0f172a;margin-top:4px;'>{orderDate:dd/MM/yyyy HH:mm}</div>
          </div>
          <div style='flex:1 1 240px;background:#ffffff;border:1px solid #e2e8f0;border-radius:8px;padding:10px 12px;'>
            <div style='font-size:12px;color:#64748b;text-transform:uppercase;letter-spacing:0.4px;'>Phương thức thanh toán</div>
            <div style='font-weight:600;color:#0f172a;margin-top:4px;'>Tiền mặt</div>
          </div>
          <div style='flex:1 1 240px;background:#ffffff;border:1px solid #e2e8f0;border-radius:8px;padding:10px 12px;'>
            <div style='font-size:12px;color:#64748b;text-transform:uppercase;letter-spacing:0.4px;'>Trạng thái</div>
            <div style='font-weight:600;color:#22c55e;margin-top:4px;'>Đã thanh toán</div>
          </div>
        </div>
      </div>

      <div style='background:#f8fafc;border:1px solid #e2e8f0;border-radius:10px;padding:16px 18px;margin:18px 0;'>
        <h3 style='margin:0 0 10px;font-size:16px;color:#334155;'>Chi tiết sản phẩm</h3>
        <div style='background:#ffffff;border:1px solid #e2e8f0;border-radius:8px;overflow:hidden;'>
          {orderDetails}
        </div>
      </div>

      <div style='background:#f8fafc;border:1px solid #e2e8f0;border-radius:10px;padding:16px 18px;margin:18px 0;'>
        <h3 style='margin:0 0 10px;font-size:16px;color:#334155;'>Tổng kết đơn hàng</h3>
        <div style='display:flex;gap:12px;flex-wrap:wrap;'>
          <div style='flex:1 1 240px;background:#ffffff;border:1px solid #e2e8f0;border-radius:8px;padding:10px 12px;'>
            <div style='font-size:12px;color:#64748b;text-transform:uppercase;letter-spacing:0.4px;'>Tổng tiền sản phẩm</div>
            <div style='font-weight:600;color:#0f172a;margin-top:4px;'>{totalPrice:N0} đ</div>
          </div>";

            if (totalDiscount.HasValue && totalDiscount.Value > 0)
            {
                body += $@"
          <div style='flex:1 1 240px;background:#ffffff;border:1px solid #e2e8f0;border-radius:8px;padding:10px 12px;'>
            <div style='font-size:12px;color:#64748b;text-transform:uppercase;letter-spacing:0.4px;'>Giảm giá</div>
            <div style='font-weight:600;color:#ef4444;margin-top:4px;'>-{(totalDiscount ?? 0):N0} đ</div>
          </div>";
            }

            body += $@"
          <div style='flex:1 1 240px;background:#ffffff;border:1px solid #e2e8f0;border-radius:8px;padding:10px 12px;'>
            <div style='font-size:12px;color:#64748b;text-transform:uppercase;letter-spacing:0.4px;'>Tổng cộng</div>
            <div style='font-weight:600;color:#4f46e5;margin-top:4px;'>{finalPrice:N0} đ</div>
          </div>
        </div>
      </div>";

            if (!string.IsNullOrEmpty(note))
            {
                body += $@"
      <div style='background:#fef3c7;border:1px solid #f59e0b;border-radius:10px;padding:16px 18px;margin:18px 0;'>
        <h3 style='margin:0 0 10px;font-size:16px;color:#92400e;'>Ghi chú</h3>
        <p style='margin:0;color:#92400e;'>{note}</p>
      </div>";
            }

            body += $@"
      <p style='margin:18px 0 0;color:#475569;font-size:14px;'>Cảm ơn bạn đã tin tưởng và ủng hộ {shopName}!</p>
    </div>

    <div style='background:#0f172a;color:#94a3b8;padding:16px 28px;font-size:12px;'>
      © {DateTime.Now.Year} {shopName}. Tất cả các quyền được bảo lưu.<br/>
      <a href='#' style='color:#94a3b8;text-decoration:none;'>Hủy đăng ký</a> |
      <a href='#' style='color:#94a3b8;text-decoration:none;'>Chính sách bảo mật</a>
    </div>
  </div>
</body>
</html>";

            return await SendEmailAsync(toEmail, subject, body);
        }

        public async Task<bool> SendLowStockAlertEmailAsync(string toEmail, string productName, int currentQuantity, int threshold, string shopName)
        {
            var subject = $"Cảnh báo sắp hết hàng - {productName}";
            var body = $@"
<html>
<body style='font-family:Arial,Helvetica,sans-serif;background:#f6f9fc;padding:24px;'>
  <div style='max-width:640px;margin:0 auto;background:#ffffff;border-radius:12px;box-shadow:0 8px 24px rgba(0,0,0,0.08);overflow:hidden;'>
    <div style='background:linear-gradient(135deg,#ef4444,#f59e0b);padding:24px 28px;color:#ffffff;'>
      <h2 style='margin:0;font-size:22px;'>⚠️ Cảnh báo sắp hết hàng</h2>
      <p style='margin:6px 0 0;opacity:0.95;'>Sản phẩm {productName} sắp hết hàng!</p>
    </div>

    <div style='padding:24px 28px;color:#0f172a;'>
      <div style='background:#fef2f2;border:1px solid #fca5a5;border-radius:10px;padding:16px 18px;margin:18px 0;'>
        <h3 style='margin:0 0 10px;font-size:16px;color:#dc2626;'>Chi tiết cảnh báo</h3>
        <div style='display:flex;gap:12px;flex-wrap:wrap;'>
          <div style='flex:1 1 240px;background:#ffffff;border:1px solid #fca5a5;border-radius:8px;padding:10px 12px;'>
            <div style='font-size:12px;color:#64748b;text-transform:uppercase;letter-spacing:0.4px;'>Sản phẩm</div>
            <div style='font-weight:600;color:#0f172a;margin-top:4px;'>{productName}</div>
          </div>
          <div style='flex:1 1 240px;background:#ffffff;border:1px solid #fca5a5;border-radius:8px;padding:10px 12px;'>
            <div style='font-size:12px;color:#64748b;text-transform:uppercase;letter-spacing:0.4px;'>Tồn kho hiện tại</div>
            <div style='font-weight:600;color:#ef4444;margin-top:4px;'>{currentQuantity}</div>
          </div>
          <div style='flex:1 1 240px;background:#ffffff;border:1px solid #fca5a5;border-radius:8px;padding:10px 12px;'>
            <div style='font-size:12px;color:#64748b;text-transform:uppercase;letter-spacing:0.4px;'>Mức cảnh báo</div>
            <div style='font-weight:600;color:#f59e0b;margin-top:4px;'>{threshold}</div>
          </div>
        </div>
        <p style='margin:10px 0 0;color:#dc2626;font-size:12px;'>Vui lòng nhập thêm hàng để đảm bảo hoạt động kinh doanh không bị gián đoạn.</p>
      </div>

      <p style='margin:18px 0 0;color:#475569;font-size:14px;'>Đây là email tự động từ hệ thống quản lý {shopName}.</p>
    </div>

    <div style='background:#0f172a;color:#94a3b8;padding:16px 28px;font-size:12px;'>
      © {DateTime.Now.Year} {shopName}. Tất cả các quyền được bảo lưu.
    </div>
  </div>
</body>
</html>";

            return await SendEmailAsync(toEmail, subject, body);
        }

        public async Task<bool> TestEmailAsync(string toEmail)
        {
            var subject = "Test Email từ ASA Platform (Brevo)";
            var body = $@"
<html>
<body style='font-family:Arial,Helvetica,sans-serif;background:#f6f9fc;padding:24px;'>
  <div style='max-width:640px;margin:0 auto;background:#ffffff;border-radius:12px;box-shadow:0 8px 24px rgba(0,0,0,0.08);overflow:hidden;'>
    <div style='background:linear-gradient(135deg,#4f46e5,#06b6d4);padding:24px 28px;color:#ffffff;'>
      <h2 style='margin:0;font-size:22px;'>✅ Test Email qua Brevo</h2>
      <p style='margin:6px 0 0;opacity:0.95;'>Dịch vụ email đã hoạt động!</p>
    </div>

    <div style='padding:24px 28px;color:#0f172a;'>
      <p style='margin:0 0 12px;'>Chúc mừng! Email service đã được cấu hình thành công.</p>
      <div style='background:#f0f9ff;border:1px solid #0ea5e9;border-radius:10px;padding:16px 18px;margin:18px 0;'>
        <h3 style='margin:0 0 10px;font-size:16px;color:#0c4a6e;'>Thông tin</h3>
        <ul style='margin:0;color:#0c4a6e;'>
          <li>✅ Brevo API key hợp lệ</li>
          <li>✅ Sender đã xác thực</li>
          <li>✅ Kết nối Render → Brevo OK</li>
        </ul>
      </div>
    </div>

    <div style='background:#0f172a;color:#94a3b8;padding:16px 28px;font-size:12px;'>
      © {DateTime.Now.Year} ASA Platform.
    </div>
  </div>
</body>
</html>";

            return await SendEmailAsync(toEmail, subject, body);
        }
    }
}
