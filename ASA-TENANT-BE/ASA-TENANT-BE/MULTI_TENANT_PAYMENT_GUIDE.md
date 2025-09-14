# Multi-Tenant Payment API Key Management Guide

## Tổng quan
Hệ thống đã được cập nhật để hỗ trợ quản lý API key riêng cho từng chủ cửa hàng với các nhà cung cấp thanh toán khác nhau.

## Kiến trúc Multi-Tenant

### 1. Database Schema

#### ShopPaymentConfig Table
```sql
CREATE TABLE ShopPaymentConfigs (
    Id BIGINT PRIMARY KEY,
    ShopId BIGINT NOT NULL,
    PaymentProvider VARCHAR(100) NOT NULL, -- "Sepay", "Zalopay", "Momo"
    ApiKey VARCHAR(500) NOT NULL,
    SecretKey VARCHAR(200),
    BankCode VARCHAR(100),
    MerchantId VARCHAR(100),
    WebhookUrl VARCHAR(500),
    AdditionalConfig TEXT, -- JSON config
    IsActive BIT DEFAULT 1,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP,
    FOREIGN KEY (ShopId) REFERENCES Shops(ShopId)
);
```

### 2. Luồng xử lý Webhook

```
1. SePay gửi webhook với API key
2. Hệ thống tìm ShopPaymentConfig theo API key
3. Xác thực API key và kiểm tra Shop
4. Tìm Order và validate Order thuộc đúng Shop
5. Xử lý thanh toán và cập nhật status
6. Gửi thông báo real-time cho đúng Shop
```

## API Endpoints

### 1. Quản lý Payment Config

#### GET `/api/shop-payment-config/shop/{shopId}`
Lấy danh sách cấu hình thanh toán của shop
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "shopId": 123,
      "paymentProvider": "Sepay",
      "apiKey": "sk_***",
      "bankCode": "VCB",
      "isActive": true
    }
  ]
}
```

#### POST `/api/shop-payment-config`
Tạo cấu hình thanh toán mới
```json
{
  "shopId": 123,
  "paymentProvider": "Sepay",
  "apiKey": "sk_test_123456789",
  "secretKey": "secret_123456789",
  "bankCode": "VCB",
  "merchantId": "MERCHANT_123",
  "webhookUrl": "https://your-domain.com/api/sepay/webhook",
  "isActive": true
}
```

#### PUT `/api/shop-payment-config/{id}`
Cập nhật cấu hình thanh toán

#### DELETE `/api/shop-payment-config/{id}`
Xóa cấu hình thanh toán

### 2. Test API Key

#### POST `/api/shop-payment-config/test-api-key`
Test API key (không trả về thông tin nhạy cảm)
```json
{
  "apiKey": "sk_test_123456789"
}
```

Response:
```json
{
  "success": true,
  "message": "API key is valid",
  "shopId": 123,
  "paymentProvider": "Sepay",
  "isActive": true
}
```

## Cấu hình cho từng Shop

### 1. Sepay Configuration
```json
{
  "shopId": 1,
  "paymentProvider": "Sepay",
  "apiKey": "sk_live_shop1_sepay_key",
  "secretKey": "secret_shop1_sepay",
  "bankCode": "VCB",
  "merchantId": "MERCHANT_SHOP1",
  "webhookUrl": "https://your-domain.com/api/sepay/webhook",
  "additionalConfig": "{\"bankAccount\":\"1234567890\",\"branchCode\":\"001\"}"
}
```

### 2. Zalopay Configuration
```json
{
  "shopId": 1,
  "paymentProvider": "Zalopay",
  "apiKey": "zpk_live_shop1_zalopay_key",
  "secretKey": "secret_shop1_zalopay",
  "merchantId": "MERCHANT_SHOP1_ZALOPAY",
  "webhookUrl": "https://your-domain.com/api/zalopay/webhook",
  "additionalConfig": "{\"appId\":\"1234567890\",\"key1\":\"key1_value\",\"key2\":\"key2_value\"}"
}
```

## Webhook Security

### 1. API Key Validation
```csharp
// Tìm ShopPaymentConfig theo API key
var paymentConfigResult = await _shopPaymentConfigService.GetByApiKeyAsync(apiKey);
if (!paymentConfigResult.Success || paymentConfigResult.Data == null)
{
    return Unauthorized(new { success = false, message = "Invalid Apikey" });
}
```

### 2. Shop Validation
```csharp
// Kiểm tra Order thuộc về đúng Shop
if (order.ShopId != paymentConfig.ShopId)
{
    return BadRequest(new { success = false, message = "Order does not belong to this shop" });
}
```

## Frontend Integration

### 1. Quản lý Payment Config
```javascript
// Lấy danh sách cấu hình thanh toán
const getPaymentConfigs = async (shopId) => {
    const response = await fetch(`/api/shop-payment-config/shop/${shopId}`, {
        headers: {
            'Authorization': `Bearer ${token}`
        }
    });
    return response.json();
};

// Tạo cấu hình mới
const createPaymentConfig = async (config) => {
    const response = await fetch('/api/shop-payment-config', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify(config)
    });
    return response.json();
};
```

### 2. Test API Key
```javascript
const testApiKey = async (apiKey) => {
    const response = await fetch('/api/shop-payment-config/test-api-key', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify({ apiKey })
    });
    return response.json();
};
```

## Migration từ Single API Key

### 1. Backup cấu hình hiện tại
```sql
-- Lưu API key hiện tại vào ShopPaymentConfig
INSERT INTO ShopPaymentConfigs (ShopId, PaymentProvider, ApiKey, IsActive, CreatedAt)
SELECT ShopId, 'Sepay', 'your-current-api-key', 1, NOW()
FROM Shops;
```

### 2. Cập nhật appsettings.json
```json
{
  "Sepay": {
    "ApiKey": "legacy-api-key" // Chỉ dùng cho fallback
  }
}
```

### 3. Fallback Logic
```csharp
// Nếu không tìm thấy trong DB, sử dụng config cũ
if (paymentConfigResult.Data == null)
{
    var legacyApiKey = _config["Sepay:ApiKey"];
    if (apiKey == legacyApiKey)
    {
        // Xử lý với legacy API key
        // Tìm Shop theo cách khác hoặc sử dụng ShopId mặc định
    }
}
```

## Monitoring & Logging

### 1. Log Webhook Calls
```csharp
_logger.LogInformation("Webhook received for Shop {ShopId}, Provider {Provider}, Order {OrderId}", 
    paymentConfig.ShopId, paymentConfig.PaymentProvider, order.OrderId);
```

### 2. Track API Key Usage
```csharp
// Log mỗi lần sử dụng API key
_logger.LogInformation("API key {ApiKey} used for Shop {ShopId}", 
    apiKey, paymentConfig.ShopId);
```

### 3. Monitor Failed Validations
```csharp
_logger.LogWarning("Invalid API key attempt: {ApiKey}, IP: {IP}", 
    apiKey, HttpContext.Connection.RemoteIpAddress);
```

## Security Best Practices

### 1. API Key Storage
- Mã hóa API key trong database
- Sử dụng environment variables cho sensitive data
- Rotate API keys định kỳ

### 2. Access Control
- Chỉ admin/shop owner mới có thể quản lý config
- Validate quyền truy cập trước khi cho phép thao tác

### 3. Rate Limiting
- Giới hạn số lần gọi API per shop
- Block IP nếu có quá nhiều request không hợp lệ

### 4. Audit Trail
- Log tất cả thay đổi config
- Track webhook calls và responses
- Monitor suspicious activities

## Troubleshooting

### 1. API Key không hoạt động
- Kiểm tra API key có đúng không
- Kiểm tra IsActive = true
- Kiểm tra PaymentProvider có đúng không

### 2. Order không tìm thấy
- Kiểm tra Order có thuộc đúng Shop không
- Kiểm tra referenceCode/code có đúng format không

### 3. Webhook không nhận được
- Kiểm tra WebhookUrl có đúng không
- Kiểm tra firewall/network settings
- Test với webhook testing tools
