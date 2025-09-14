# Order Status và Shop Groups Guide

## Tổng quan
Hệ thống đã được cập nhật để hỗ trợ:
- **Order Status Management**: Cập nhật trạng thái đơn hàng
- **Multi-Shop Support**: Gửi thông báo theo từng cửa hàng
- **Real-time Notifications**: Thông báo real-time cho đúng đối tượng

## Order Status Flow

### 1. Trạng thái đơn hàng
```
0: Chờ xác nhận
1: Chờ thanh toán  
2: Đã thanh toán
3: Đang chuẩn bị
4: Đang giao hàng
5: Đã giao hàng
6: Đã hủy
```

### 2. Luồng thanh toán
1. **Tạo đơn hàng**: Status = 1 (Chờ thanh toán)
2. **Khách hàng thanh toán**: SePay xử lý
3. **SePay webhook**: Gửi payload về hệ thống
4. **Cập nhật status**: Status = 2 (Đã thanh toán)
5. **Gửi thông báo**: Real-time notification

## Shop Groups trong SignalR

### 1. Các Groups có sẵn
- `User_{CustomerId}` - Thông báo cho khách hàng cụ thể
- `Shop_{ShopId}` - Thông báo cho cửa hàng cụ thể
- `Admin` - Thông báo cho tất cả admin

### 2. Cách join Groups

#### Frontend (JavaScript)
```javascript
// Kết nối SignalR
const connection = new HubConnectionBuilder()
    .withUrl("/notificationHub")
    .build();

await connection.start();

// Join User group (khi user đăng nhập)
await connection.invoke("JoinGroup", `User_${userId}`);

// Join Shop group (khi staff đăng nhập)
await connection.invoke("JoinShopGroup", shopId);

// Join Admin group (khi admin đăng nhập)
await connection.invoke("JoinAdminGroup");
```

#### Backend (C#)
```csharp
// Trong controller hoặc service
await _hubContext.Clients.Group($"Shop_{shopId}")
    .SendAsync("ReceiveNotification", notificationData);
```

## Cập nhật Order Status

### 1. API Endpoint
```csharp
// POST /api/orders/{id}/status
[HttpPut("{id}/status")]
public async Task<IActionResult> UpdateOrderStatus(long id, [FromBody] UpdateStatusRequest request)
{
    var result = await _orderService.UpdateStatusAsync(id, request.Status);
    return Ok(result);
}
```

### 2. UpdateStatusRequest Model
```csharp
public class UpdateStatusRequest
{
    public short Status { get; set; }
    public string? Note { get; set; }
}
```

## Thông báo Real-time

### 1. Payment Success Notification
```javascript
{
    message: "Thanh toán thành công cho đơn hàng #123",
    data: {
        orderId: 123,
        shopId: 1,
        customerId: 456,
        amount: 100000,
        status: "PAID",
        transactionId: 789
    },
    timestamp: "2024-01-01T10:00:00Z",
    type: "PaymentSuccess"
}
```

### 2. Các loại thông báo
- **PaymentSuccess**: Thanh toán thành công
- **OrderStatusChanged**: Trạng thái đơn hàng thay đổi
- **NewOrder**: Đơn hàng mới
- **OrderCancelled**: Đơn hàng bị hủy

## Frontend Integration

### 1. Lắng nghe thông báo
```javascript
connection.on("ReceiveNotification", (notification) => {
    switch(notification.type) {
        case "PaymentSuccess":
            handlePaymentSuccess(notification.data);
            break;
        case "OrderStatusChanged":
            handleOrderStatusChanged(notification.data);
            break;
        // ... other cases
    }
});
```

### 2. Xử lý thông báo theo Shop
```javascript
function handlePaymentSuccess(data) {
    // Chỉ xử lý nếu là shop hiện tại
    if (data.shopId === currentShopId) {
        showSuccessMessage(`Đơn hàng #${data.orderId} đã thanh toán thành công`);
        updateOrderStatus(data.orderId, 'PAID');
    }
}
```

## Database Schema

### Order Table
```sql
CREATE TABLE Orders (
    OrderId BIGINT PRIMARY KEY,
    CustomerId BIGINT,
    ShopId BIGINT,
    Status SMALLINT, -- 0-6
    TotalPrice DECIMAL,
    PaymentMethod VARCHAR(50),
    CreatedAt TIMESTAMP,
    Note TEXT
);
```

### Transaction Table
```sql
CREATE TABLE Transactions (
    TransactionId BIGINT PRIMARY KEY,
    OrderId BIGINT,
    UserId BIGINT, -- CustomerId
    PaymentStatus VARCHAR(20),
    AppTransId VARCHAR(100),
    ZpTransId VARCHAR(100),
    ReturnCode INT,
    ReturnMessage TEXT,
    CreatedAt TIMESTAMP
);
```

## Testing

### 1. Test Order Status Update
```bash
curl -X PUT "https://your-domain/api/orders/123/status" \
  -H "Authorization: Bearer your-token" \
  -H "Content-Type: application/json" \
  -d '{"status": 2}'
```

### 2. Test SignalR Groups
```javascript
// Test join shop group
await connection.invoke("JoinShopGroup", 1);

// Test receive notification
connection.on("ReceiveNotification", (notification) => {
    console.log("Received:", notification);
});
```

## Lưu ý quan trọng

### 1. Security
- Chỉ cho phép user join groups của họ
- Validate ShopId trước khi join Shop group
- Admin có thể join tất cả groups

### 2. Performance
- Sử dụng connection pooling cho SignalR
- Limit số lượng connections per user
- Cleanup disconnected connections

### 3. Error Handling
- Handle connection drops
- Retry failed notifications
- Log all notification attempts

### 4. Monitoring
- Track notification delivery rates
- Monitor group membership
- Alert on failed status updates
