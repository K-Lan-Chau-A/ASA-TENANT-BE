# Hướng dẫn tính năng tự động hủy đơn hàng

## Tổng quan
Tính năng này tự động hủy các đơn hàng sau 5 phút nếu chúng không được thanh toán.

## Cách hoạt động

### 1. Tạo đơn hàng
- Khi tạo đơn hàng mới, hệ thống sẽ:
  - Set `Status = 0` (Chờ thanh toán)
  - Set `CreatedAt = DateTime.UtcNow` (thời gian tạo)

### 2. Background Job
- **OrderExpirationJob** chạy mỗi phút để kiểm tra đơn hàng hết hạn
- Tìm các đơn hàng có:
  - `Status = 0` (Chờ thanh toán)
  - `CreatedAt <= DateTime.UtcNow.AddMinutes(-5)` (được tạo trước 5 phút)

### 3. Hủy đơn hàng tự động
- Đơn hàng hết hạn sẽ được hủy với:
  - `Status = 2` (Đã hủy)
  - `Note` được cập nhật với lý do: "Đơn hàng hết hạn thanh toán sau 5 phút"

## API Endpoints

### Hủy đơn hàng thủ công
```
POST /api/orders/{id}/cancel
Content-Type: application/json

{
    "reason": "Lý do hủy đơn hàng (tùy chọn)"
}
```

### Cập nhật trạng thái đơn hàng
```
PUT /api/orders/{id}
Content-Type: application/json

{
    "status": 1  // 1 = Đã thanh toán
}
```

## Trạng thái đơn hàng (OrderStatus)
- `0` = Chờ thanh toán (Pending)
- `1` = Đã thanh toán (Paid)  
- `2` = Đã hủy (Cancelled)

## Logging
Hệ thống sẽ log các hoạt động:
- Số lượng đơn hàng hết hạn được tìm thấy
- Kết quả hủy từng đơn hàng
- Lỗi nếu có

## Cấu hình
- Thời gian hết hạn: 5 phút (có thể thay đổi trong `OrderExpirationJob.cs`)
- Tần suất kiểm tra: mỗi phút (có thể thay đổi trong `Program.cs`)

## Lưu ý
- Chỉ đơn hàng có status = 0 mới có thể bị hủy tự động
- Đơn hàng đã thanh toán (status = 1) sẽ không bị ảnh hưởng
- Background job sử dụng Quartz.NET để đảm bảo độ tin cậy
