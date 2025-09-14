# Simple Shop Payment Configuration Guide

## Tổng quan
Hệ thống sử dụng cách tiếp cận đơn giản: lưu trữ API key và cấu hình thanh toán trực tiếp trong table Shop.

## Database Schema

### Shop Table Extensions
```sql
-- Thêm các cột mới vào bảng Shop
ALTER TABLE Shops ADD COLUMN SepayApiKey VARCHAR(500);
ALTER TABLE Shops ADD COLUMN SepaySecretKey VARCHAR(200);
ALTER TABLE Shops ADD COLUMN SepayBankCode VARCHAR(100);
ALTER TABLE Shops ADD COLUMN SepayMerchantId VARCHAR(100);

ALTER TABLE Shops ADD COLUMN ZalopayApiKey VARCHAR(500);
ALTER TABLE Shops ADD COLUMN ZalopaySecretKey VARCHAR(200);
ALTER TABLE Shops ADD COLUMN ZalopayMerchantId VARCHAR(100);

ALTER TABLE Shops ADD COLUMN MomoApiKey VARCHAR(500);
ALTER TABLE Shops ADD COLUMN MomoSecretKey VARCHAR(200);
ALTER TABLE Shops ADD COLUMN MomoMerchantId VARCHAR(100);

ALTER TABLE Shops ADD COLUMN WebhookUrl VARCHAR(500);
ALTER TABLE Shops ADD COLUMN PaymentConfig TEXT; -- JSON config
```

## API Endpoints

### 1. Quản lý Payment Config

#### GET `/api/shop-payment/{shopId}`
Lấy thông tin cấu hình thanh toán của shop
```json
{
  "success": true,
  "data": {
    "shopId": 123,
    "shopName": "Cửa hàng ABC",
    "sepayApiKey": "sk_live_shop123_sepay",
    "sepayBankCode": "VCB",
    "sepayMerchantId": "MERCHANT_123",
    "zalopayApiKey": "zpk_live_shop123_zalopay",
    "momoApiKey": "momo_live_shop123",
    "webhookUrl": "https://your-domain.com/api/sepay/webhook",
    "paymentConfig": "{\"bankAccount\":\"1234567890\"}"
  }
}
```

#### PUT `/api/shop-payment/{shopId}/sepay`
Cập nhật cấu hình Sepay
```json
{
  "apiKey": "sk_live_shop123_sepay",
  "secretKey": "secret_shop123",
  "bankCode": "VCB",
  "merchantId": "MERCHANT_123",
  "webhookUrl": "https://your-domain.com/api/sepay/webhook"
}
```

#### PUT `/api/shop-payment/{shopId}/zalopay`
Cập nhật cấu hình Zalopay
```json
{
  "apiKey": "zpk_live_shop123_zalopay",
  "secretKey": "secret_shop123_zalopay",
  "merchantId": "MERCHANT_123_ZALOPAY"
}
```

### 2. Test API Key

#### POST `/api/shop-payment/test-api-key`
Test API key
```json
{
  "apiKey": "sk_live_shop123_sepay"
}
```

Response:
```json
{
  "success": true,
  "message": "API key is valid",
  "shopId": 123,
  "shopName": "Cửa hàng ABC",
  "isActive": true
}
```

### 3. Admin Functions

#### GET `/api/shop-payment/all`
Lấy danh sách tất cả shops với payment config (chỉ admin)
```json
{
  "success": true,
  "data": [
    {
      "shopId": 123,
      "shopName": "Cửa hàng ABC",
      "status": 1,
      "hasSepayApiKey": true,
      "hasZalopayApiKey": false,
      "hasMomoApiKey": true,
      "createdAt": "2024-01-01T10:00:00Z"
    }
  ]
}
```

## Webhook Processing

### 1. Sepay Webhook Flow
```
1. SePay gửi webhook với API key
2. Hệ thống tìm Shop theo SepayApiKey
3. Xác thực Shop status (active)
4. Tìm Order và validate Order thuộc đúng Shop
5. Xử lý thanh toán và cập nhật status
6. Gửi thông báo real-time cho đúng Shop
```

### 2. Code Example
```csharp
// Tìm Shop theo SepayApiKey
var shop = await _shopRepo.GetAllAsync()
    .FirstOrDefaultAsync(s => s.SepayApiKey == apiKey);

if (shop == null)
{
    return Unauthorized("Invalid API key");
}

if (shop.Status != 1) // 1 = active
{
    return Unauthorized("Shop is not active");
}

// Validate Order thuộc đúng Shop
if (order.ShopId != shop.ShopId)
{
    return BadRequest("Order does not belong to this shop");
}
```

## Frontend Integration

### 1. Quản lý Payment Config
```javascript
// Lấy cấu hình thanh toán
const getPaymentConfig = async (shopId) => {
    const response = await fetch(`/api/shop-payment/${shopId}`, {
        headers: {
            'Authorization': `Bearer ${token}`
        }
    });
    return response.json();
};

// Cập nhật cấu hình Sepay
const updateSepayConfig = async (shopId, config) => {
    const response = await fetch(`/api/shop-payment/${shopId}/sepay`, {
        method: 'PUT',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify(config)
    });
    return response.json();
};

// Test API key
const testApiKey = async (apiKey) => {
    const response = await fetch('/api/shop-payment/test-api-key', {
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

### 2. UI Components
```jsx
// Sepay Configuration Form
const SepayConfigForm = ({ shopId, config, onUpdate }) => {
    const [formData, setFormData] = useState({
        apiKey: config?.sepayApiKey || '',
        secretKey: config?.sepaySecretKey || '',
        bankCode: config?.sepayBankCode || '',
        merchantId: config?.sepayMerchantId || '',
        webhookUrl: config?.webhookUrl || ''
    });

    const handleSubmit = async (e) => {
        e.preventDefault();
        const result = await updateSepayConfig(shopId, formData);
        if (result.success) {
            onUpdate(result.data);
        }
    };

    return (
        <form onSubmit={handleSubmit}>
            <input 
                type="text" 
                placeholder="API Key"
                value={formData.apiKey}
                onChange={(e) => setFormData({...formData, apiKey: e.target.value})}
            />
            {/* Other fields... */}
            <button type="submit">Cập nhật</button>
        </form>
    );
};
```

## Migration từ Single API Key

### 1. Backup và Migration
```sql
-- Backup API key hiện tại
UPDATE Shops 
SET SepayApiKey = 'your-current-api-key'
WHERE SepayApiKey IS NULL;
```

### 2. Cập nhật appsettings.json
```json
{
  "Sepay": {
    "ApiKey": "legacy-fallback-key" // Chỉ dùng cho fallback
  }
}
```

### 3. Fallback Logic trong Code
```csharp
// Nếu không tìm thấy trong DB, sử dụng config cũ
if (shop == null)
{
    var legacyApiKey = _config["Sepay:ApiKey"];
    if (apiKey == legacyApiKey)
    {
        // Xử lý với legacy API key
        // Có thể sử dụng ShopId mặc định hoặc tìm theo cách khác
    }
}
```

## Security Best Practices

### 1. API Key Storage
- Mã hóa API key trong database
- Sử dụng environment variables cho sensitive data
- Rotate API keys định kỳ

### 2. Access Control
- Chỉ shop owner/admin mới có thể quản lý config
- Validate quyền truy cập trước khi cho phép thao tác

### 3. Validation
- Validate API key format
- Kiểm tra Shop status trước khi xử lý
- Validate Order ownership

### 4. Logging
```csharp
_logger.LogInformation("Webhook received for Shop {ShopId}, Order {OrderId}", 
    shop.ShopId, order.OrderId);

_logger.LogWarning("Invalid API key attempt: {ApiKey}, IP: {IP}", 
    apiKey, HttpContext.Connection.RemoteIpAddress);
```

## Monitoring & Troubleshooting

### 1. Health Check
```csharp
[HttpGet("health")]
public async Task<IActionResult> HealthCheck()
{
    var activeShops = await _shopRepo.GetAllAsync()
        .CountAsync(s => s.Status == 1 && !string.IsNullOrEmpty(s.SepayApiKey));
    
    return Ok(new { 
        success = true, 
        activeShopsWithSepay = activeShops 
    });
}
```

### 2. Common Issues
- **API key không hoạt động**: Kiểm tra Shop status và API key format
- **Order không tìm thấy**: Kiểm tra Order có thuộc đúng Shop không
- **Webhook không nhận được**: Kiểm tra WebhookUrl và network settings

### 3. Debug Tools
```javascript
// Test API key từ frontend
const testApiKey = async (apiKey) => {
    try {
        const result = await fetch('/api/shop-payment/test-api-key', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ apiKey })
        });
        const data = await result.json();
        console.log('API Key test result:', data);
        return data;
    } catch (error) {
        console.error('Error testing API key:', error);
    }
};
```

## Lợi ích của cách tiếp cận này

### 1. Đơn giản
- Không cần table riêng cho payment config
- Dễ quản lý và maintain
- Ít complexity hơn

### 2. Performance
- Ít JOIN queries
- Faster lookup
- Đơn giản hơn cho caching

### 3. Scalability
- Dễ dàng thêm payment provider mới
- Flexible configuration
- Dễ migrate và backup

### 4. Security
- Centralized trong Shop table
- Dễ audit và monitor
- Consistent với business logic
