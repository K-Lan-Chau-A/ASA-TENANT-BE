# Simple Sepay API Key Management Guide

## Tổng quan
Hệ thống đơn giản: mỗi Shop chỉ có 1 Sepay API key được lưu trực tiếp trong table Shop.

## Database Schema

### Shop Table - Thêm 1 cột
```sql
-- Chỉ cần thêm 1 cột vào bảng Shop
ALTER TABLE Shops ADD COLUMN SepayApiKey VARCHAR(500);
```

## API Endpoints

### 1. Quản lý Sepay API Key

#### GET `/api/shop-payment/{shopId}`
Lấy thông tin Sepay API key của shop
```json
{
  "success": true,
  "data": {
    "shopId": 123,
    "shopName": "Cửa hàng ABC",
    "sepayApiKey": "sk_live_shop123_sepay",
    "hasApiKey": true
  }
}
```

#### PUT `/api/shop-payment/{shopId}`
Cập nhật Sepay API key cho shop
```json
{
  "apiKey": "sk_live_shop123_sepay"
}
```

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

#### GET `/api/shop-payment/all`
Lấy danh sách tất cả shops với Sepay API key (admin only)
```json
{
  "success": true,
  "data": [
    {
      "shopId": 123,
      "shopName": "Cửa hàng ABC",
      "status": 1,
      "hasSepayApiKey": true,
      "createdAt": "2024-01-01T10:00:00Z"
    }
  ]
}
```

## Webhook Processing

### 1. Sepay Webhook Flow
```
1. SePay gửi webhook với API key
2. Tìm Shop: SELECT * FROM Shops WHERE SepayApiKey = 'api_key'
3. Kiểm tra Shop status (active)
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

### 1. Quản lý Sepay API Key
```javascript
// Lấy Sepay API key của shop
const getSepayApiKey = async (shopId) => {
    const response = await fetch(`/api/shop-payment/${shopId}`, {
        headers: {
            'Authorization': `Bearer ${token}`
        }
    });
    return response.json();
};

// Cập nhật Sepay API key
const updateSepayApiKey = async (shopId, apiKey) => {
    const response = await fetch(`/api/shop-payment/${shopId}`, {
        method: 'PUT',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify({ apiKey })
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

### 2. UI Component
```jsx
// Sepay API Key Management
const SepayApiKeyManager = ({ shopId }) => {
    const [apiKey, setApiKey] = useState('');
    const [loading, setLoading] = useState(false);

    const handleUpdate = async () => {
        setLoading(true);
        try {
            const result = await updateSepayApiKey(shopId, apiKey);
            if (result.success) {
                alert('API key updated successfully');
            } else {
                alert('Failed to update API key');
            }
        } catch (error) {
            alert('Error updating API key');
        } finally {
            setLoading(false);
        }
    };

    const handleTest = async () => {
        try {
            const result = await testApiKey(apiKey);
            if (result.success) {
                alert(`API key is valid for Shop: ${result.shopName}`);
            } else {
                alert('Invalid API key');
            }
        } catch (error) {
            alert('Error testing API key');
        }
    };

    return (
        <div>
            <h3>Sepay API Key Management</h3>
            <input
                type="text"
                placeholder="Enter Sepay API Key"
                value={apiKey}
                onChange={(e) => setApiKey(e.target.value)}
            />
            <button onClick={handleUpdate} disabled={loading}>
                {loading ? 'Updating...' : 'Update API Key'}
            </button>
            <button onClick={handleTest}>
                Test API Key
            </button>
        </div>
    );
};
```

## Migration từ Single API Key

### 1. Database Migration
```sql
-- Thêm cột SepayApiKey vào bảng Shop
ALTER TABLE Shops ADD COLUMN SepayApiKey VARCHAR(500);

-- Cập nhật API key hiện tại cho tất cả shops
UPDATE Shops 
SET SepayApiKey = 'your-current-sepay-api-key'
WHERE SepayApiKey IS NULL;
```

### 2. Fallback Logic
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

## Security

### 1. API Key Validation
- Kiểm tra API key có tồn tại trong database
- Kiểm tra Shop status (active)
- Validate Order ownership

### 2. Access Control
- Chỉ shop owner/admin mới có thể quản lý API key
- Validate quyền truy cập trước khi cho phép thao tác

### 3. Logging
```csharp
_logger.LogInformation("Webhook received for Shop {ShopId}, Order {OrderId}", 
    shop.ShopId, order.OrderId);

_logger.LogWarning("Invalid API key attempt: {ApiKey}, IP: {IP}", 
    apiKey, HttpContext.Connection.RemoteIpAddress);
```

## Testing

### 1. Test API Key
```bash
curl -X POST "https://your-domain/api/shop-payment/test-api-key" \
  -H "Authorization: Bearer your-token" \
  -H "Content-Type: application/json" \
  -d '{"apiKey": "sk_live_shop123_sepay"}'
```

### 2. Test Webhook
```bash
curl -X POST "https://your-domain/api/sepay/webhook" \
  -H "Authorization: Apikey sk_live_shop123_sepay" \
  -H "Content-Type: application/json" \
  -d '{
    "id": "test-transaction-123",
    "referenceCode": "1",
    "transferAmount": 100000,
    "transactionDate": "2024-01-01T10:00:00Z"
  }'
```

## Lợi ích

### 1. Đơn giản
- Chỉ 1 cột trong database
- Ít code phức tạp
- Dễ hiểu và maintain

### 2. Performance
- Không cần JOIN queries
- Fast lookup
- Đơn giản cho caching

### 3. Scalability
- Dễ dàng thêm shop mới
- Không cần config phức tạp
- Dễ migrate và backup

### 4. Security
- Centralized trong Shop table
- Dễ audit và monitor
- Consistent với business logic
