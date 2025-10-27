# Hướng dẫn cấu hình Brevo Email Service

## Tổng quan
Dự án đã được chuyển đổi từ SendGrid sang Brevo để gửi email. Brevo (trước đây là Sendinblue) cung cấp dịch vụ email marketing và transactional email với giá cả cạnh tranh.

## Các thay đổi đã thực hiện

### 1. Package Dependencies
- ✅ Đã xóa `SendGrid` package khỏi `ASA-TENANT-BE.csproj`
- ✅ Không cần cài đặt package Brevo SDK riêng - sử dụng HttpClient trực tiếp

### 2. EmailService Implementation
- ✅ Thay thế SendGrid client bằng HttpClient để gọi Brevo REST API
- ✅ Cập nhật phương thức `SendEmailAsync()` để sử dụng Brevo API REST
- ✅ Cập nhật constructor để nhận HttpClient dependency
- ✅ Giữ nguyên tất cả các phương thức email khác (Order Confirmation, Low Stock Alert, Test Email)

### 3. Configuration
- ✅ Cập nhật `appsettings.json` và `appsettings.Development.json`
- ✅ Thay thế `SendGridSettings` bằng `BrevoSettings`

### 4. Dependency Injection
- ✅ Cập nhật `Program.cs` để đăng ký EmailService với HttpClient
- ✅ Thay đổi từ `AddScoped<IEmailService, EmailService>()` thành `AddHttpClient<IEmailService, EmailService>()`

## Cấu hình Brevo

### Bước 1: Tạo tài khoản Brevo
1. Truy cập [https://www.brevo.com](https://www.brevo.com)
2. Đăng ký tài khoản miễn phí
3. Xác thực email

### Bước 2: Lấy API Key
1. Đăng nhập vào Brevo dashboard
2. Vào **Settings** > **API Keys**
3. Tạo API key mới cho **Transactional emails**
4. Copy API key

### Bước 3: Cấu hình trong ứng dụng

#### Option 1: Environment Variables (Khuyến nghị)
```bash
# Windows
set BREVO_SETTINGS__APIKEY=your_brevo_api_key_here
set BREVO_SETTINGS__FROMEMAIL=your_email@domain.com
set BREVO_SETTINGS__FROMNAME=Your App Name

# Linux/Mac
export BREVO_SETTINGS__APIKEY=your_brevo_api_key_here
export BREVO_SETTINGS__FROMEMAIL=your_email@domain.com
export BREVO_SETTINGS__FROMNAME=Your App Name
```

#### Option 2: appsettings.json
```json
{
  "BrevoSettings": {
    "ApiKey": "your_brevo_api_key_here",
    "FromEmail": "your_email@domain.com",
    "FromName": "Your App Name"
  }
}
```

### Bước 4: Xác thực Domain (Tùy chọn)
1. Vào **Senders & IP** > **Domains**
2. Thêm domain của bạn
3. Cấu hình DNS records theo hướng dẫn
4. Xác thực domain để tăng deliverability

## Test Email Service

### Sử dụng TestEmailAsync
```csharp
// Trong controller hoặc service
var emailService = new EmailService(configuration, logger);
var result = await emailService.TestEmailAsync("test@example.com");
```

### Kiểm tra logs
- ✅ Thành công: `Email sent successfully to {email}. Message ID: {messageId}`
- ❌ Lỗi: `Brevo API not initialized` hoặc `Brevo configuration missing`

## So sánh SendGrid vs Brevo

| Tính năng | SendGrid | Brevo |
|-----------|----------|-------|
| Free Tier | 100 emails/day | 300 emails/day |
| Pricing | $19.95/month (40K emails) | $25/month (20K emails) |
| API | RESTful | RESTful |
| Documentation | Tốt | Tốt |
| Support | 24/7 | Business hours |

## Troubleshooting

### Lỗi thường gặp

1. **"Brevo API not initialized"**
   - Kiểm tra API key có được cấu hình đúng không
   - Kiểm tra environment variables hoặc appsettings

2. **"Brevo configuration missing"**
   - Kiểm tra FromEmail có được cấu hình không
   - Kiểm tra format email có hợp lệ không

3. **Email không được gửi**
   - Kiểm tra API key có quyền gửi email không
   - Kiểm tra domain có được xác thực không
   - Kiểm tra logs để xem chi tiết lỗi

### Debug Steps
1. Kiểm tra logs trong console
2. Test với API key khác
3. Kiểm tra network connectivity
4. Verify email format và content

## Migration Checklist

- [x] Cài đặt sib_api_v3_sdk package (official Brevo SDK)
- [x] Cập nhật EmailService implementation
- [x] Cập nhật configuration files
- [x] Test email functionality
- [x] Tạo documentation
- [x] Cấu hình development API key
- [ ] Test với real email addresses
- [ ] Monitor email delivery rates

## Liên hệ hỗ trợ

- Brevo Documentation: [https://developers.brevo.com](https://developers.brevo.com)
- Brevo Support: [https://help.brevo.com](https://help.brevo.com)
- GitHub Issues: Tạo issue trong repository nếu cần hỗ trợ
