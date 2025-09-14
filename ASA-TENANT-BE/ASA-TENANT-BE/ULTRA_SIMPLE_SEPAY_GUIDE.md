# Ultra Simple Sepay API Key Guide

## Tổng quan
Giải pháp siêu đơn giản: chỉ thêm 1 cột `SepayApiKey` vào table Shop và sử dụng ShopController có sẵn.

## Database Schema

### Chỉ cần thêm 1 cột vào Shop table
```sql
ALTER TABLE Shops ADD COLUMN SepayApiKey VARCHAR(500);
```

## Model Update

### Shop.cs - Thêm trực tiếp vào model
```csharp
public partial class Shop
{
    // ... existing properties ...
    public string QrcodeUrl { get; set; }
    
    public string? SepayApiKey { get; set; }  // ← Chỉ thêm dòng này
    
    // ... navigation properties ...
}
```

## API Endpoints

### Sử dụng ShopController có sẵn

#### PUT `/api/shops/{id}/sepay-api-key`
Cập nhật Sepay API key cho shop
```json
{
  "apiKey": "sk_live_shop123_sepay"
}
```

#### POST `/api/shops/test-sepay-api-key`
Test Sepay API key
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
  "data": {
    "shopId": 123,
    "shopName": "Cửa hàng ABC",
    "sepayApiKey": "sk_live_shop123_sepay",
    "status": 1
  }
}
```

## Webhook Processing

### SepayWebhookController - Không thay đổi
```csharp
// Tìm Shop theo SepayApiKey
var shop = await _shopRepo.GetAllAsync()
    .FirstOrDefaultAsync(s => s.SepayApiKey == apiKey);

if (shop == null || shop.Status != 1)
{
    return Unauthorized("Invalid API key or shop inactive");
}

// Validate Order thuộc đúng Shop
if (order.ShopId != shop.ShopId)
{
    return BadRequest("Order does not belong to this shop");
}
```

## Frontend Integration

### 1. Cập nhật Sepay API key
```javascript
const updateSepayApiKey = async (shopId, apiKey) => {
    const response = await fetch(`/api/shops/${shopId}/sepay-api-key`, {
        method: 'PUT',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify({ apiKey })
    });
    return response.json();
};
```

### 2. Test API key
```javascript
const testSepayApiKey = async (apiKey) => {
    const response = await fetch('/api/shops/test-sepay-api-key', {
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

### 3. UI Component đơn giản
```jsx
const SepayApiKeyManager = ({ shopId }) => {
    const [apiKey, setApiKey] = useState('');

    const handleUpdate = async () => {
        const result = await updateSepayApiKey(shopId, apiKey);
        if (result.success) {
            alert('API key updated successfully');
        } else {
            alert('Failed to update API key');
        }
    };

    const handleTest = async () => {
        const result = await testSepayApiKey(apiKey);
        if (result.success) {
            alert(`API key is valid for Shop: ${result.data.shopName}`);
        } else {
            alert('Invalid API key');
        }
    };

    return (
        <div>
            <h3>Sepay API Key</h3>
            <input
                type="text"
                placeholder="Enter Sepay API Key"
                value={apiKey}
                onChange={(e) => setApiKey(e.target.value)}
            />
            <button onClick={handleUpdate}>Update</button>
            <button onClick={handleTest}>Test</button>
        </div>
    );
};
```

## Migration

### 1. Database Migration
```sql
-- Thêm cột SepayApiKey
ALTER TABLE Shops ADD COLUMN SepayApiKey VARCHAR(500);

-- Cập nhật API key hiện tại cho tất cả shops
UPDATE Shops 
SET SepayApiKey = 'your-current-sepay-api-key'
WHERE SepayApiKey IS NULL;
```

### 2. Code Changes
- ✅ Thêm `SepayApiKey` vào Shop model
- ✅ Thêm 2 methods vào ShopController
- ✅ Thêm 2 methods vào IShopService và ShopService
- ✅ SepayWebhookController sử dụng `s.SepayApiKey`

## Testing

### 1. Test API Key
```bash
curl -X POST "https://your-domain/api/shops/test-sepay-api-key" \
  -H "Authorization: Bearer your-token" \
  -H "Content-Type: application/json" \
  -d '{"apiKey": "sk_live_shop123_sepay"}'
```

### 2. Update API Key
```bash
curl -X PUT "https://your-domain/api/shops/123/sepay-api-key" \
  -H "Authorization: Bearer your-token" \
  -H "Content-Type: application/json" \
  -d '{"apiKey": "sk_live_shop123_sepay"}'
```

### 3. Test Webhook
```bash
curl -X POST "https://your-domain/api/sepay/webhook" \
  -H "Authorization: Apikey sk_live_shop123_sepay" \
  -H "Content-Type: application/json" \
  -d '{
    "id": "test-transaction-123",
    "referenceCode": "1",
    "transferAmount": 100000
  }'
```

## Lợi ích

### 1. Siêu đơn giản
- Chỉ 1 cột trong database
- Sử dụng controller có sẵn
- Ít code phức tạp nhất

### 2. Không cần tạo file mới
- Không cần ShopExtensions.cs
- Không cần ShopPaymentController.cs
- Sử dụng ShopController có sẵn

### 3. Dễ maintain
- Tất cả logic shop ở 1 chỗ
- Consistent với existing code
- Dễ hiểu và debug

### 4. Performance cao
- Không cần JOIN queries
- Fast lookup
- Đơn giản cho caching

## Kết luận

Đây là giải pháp đơn giản nhất có thể:
- ✅ Chỉ 1 cột `SepayApiKey` trong Shop table
- ✅ Sử dụng ShopController có sẵn
- ✅ Không cần tạo file mới
- ✅ Dễ migrate và maintain
- ✅ Đầy đủ tính năng multi-tenant
