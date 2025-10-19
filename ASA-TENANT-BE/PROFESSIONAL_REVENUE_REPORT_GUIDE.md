# Hướng dẫn sử dụng API Báo cáo Doanh thu Chuyên nghiệp

## Tổng quan
API này tạo ra một báo cáo doanh thu chuyên nghiệp với nhiều sheet phân tích chi tiết về doanh thu của cửa hàng.

## Endpoint
```
POST /api/Report/professional-revenue
```

## Request Body
```json
{
  "startDate": "2024-01-01T00:00:00",
  "endDate": "2024-12-31T23:59:59",
  "shopId": 1
}
```

## Response
- **Content-Type**: `application/vnd.openxmlformats-officedocument.spreadsheetml.sheet`
- **File Name**: `Professional_Revenue_Report_{startDate}_{endDate}.xlsx`

## Cấu trúc File Excel

### 1. Sheet "TỔNG QUAN"
- **Tiêu đề báo cáo**: Thông tin cửa hàng và thời gian báo cáo
- **Tổng quan doanh thu**: 
  - Tổng doanh thu
  - Số đơn hàng
  - Số sản phẩm bán
  - Giá trị đơn hàng trung bình
- **Doanh thu theo phương thức thanh toán**: Phân tích tỷ lệ các phương thức thanh toán

### 2. Sheet "DOANH THU THEO NGÀY"
- **Cột**: Ngày, Số đơn hàng, Doanh thu, Trung bình/đơn, Số sản phẩm
- **Dữ liệu**: Doanh thu được nhóm theo từng ngày
- **Tổng cộng**: Tổng kết cuối sheet

### 3. Sheet "DOANH THU THEO SẢN PHẨM"
- **Cột**: STT, Tên sản phẩm, SKU, Danh mục, Số lượng bán, Doanh thu, Giá trung bình
- **Dữ liệu**: Top 20 sản phẩm bán chạy nhất
- **Sắp xếp**: Theo doanh thu giảm dần

### 4. Sheet "DOANH THU THEO DANH MỤC"
- **Cột**: STT, Tên danh mục, Số sản phẩm, Số lượng bán, Doanh thu, Tỷ lệ (%)
- **Dữ liệu**: Doanh thu được nhóm theo danh mục sản phẩm
- **Sắp xếp**: Theo doanh thu giảm dần

### 5. Sheet "PHÂN TÍCH KHÁCH HÀNG"
- **Cột**: STT, Mã khách hàng, Tên khách hàng, Số đơn hàng, Doanh thu, Giá trị TB/đơn
- **Dữ liệu**: Top 20 khách hàng có doanh thu cao nhất
- **Sắp xếp**: Theo doanh thu giảm dần

## Cách sử dụng

### 1. Test với Postman/Insomnia
1. Tạo request POST đến `https://localhost:7001/api/reports/professional-revenue`
2. Set Content-Type: `application/json`
3. Body:
```json
{
  "startDate": "2024-01-01T00:00:00",
  "endDate": "2024-12-31T23:59:59",
  "shopId": 1
}
```
4. Send request
5. File Excel sẽ được tải về tự động

### 2. Test với file .http
Sử dụng file `Test_Professional_Revenue_Report.http` đã tạo sẵn

### 3. Frontend Integration
```javascript
// JavaScript để gọi API và tải file
async function downloadProfessionalRevenueReport(startDate, endDate, shopId) {
  try {
    const response = await fetch('/api/reports/professional-revenue', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        startDate: startDate,
        endDate: endDate,
        shopId: shopId
      })
    });

    if (!response.ok) {
      throw new Error('Network response was not ok');
    }

    // Tạo blob từ response
    const blob = await response.blob();
    
    // Tạo URL tạm thời cho blob
    const url = window.URL.createObjectURL(blob);
    
    // Tạo link tải về
    const a = document.createElement('a');
    a.href = url;
    a.download = `Professional_Revenue_Report_${startDate}_${endDate}.xlsx`;
    
    // Trigger download
    document.body.appendChild(a);
    a.click();
    
    // Cleanup
    document.body.removeChild(a);
    window.URL.revokeObjectURL(url);
    
    console.log('File downloaded successfully');
  } catch (error) {
    console.error('Error downloading file:', error);
  }
}

// Sử dụng
downloadProfessionalRevenueReport('2024-01-01T00:00:00', '2024-12-31T23:59:59', 1);
```

## Lưu ý quan trọng

1. **Dữ liệu**: Chỉ lấy đơn hàng có status >= 2 (đã thanh toán)
2. **Thời gian**: `startDate` và `endDate` phải hợp lệ
3. **ShopId**: Phải tồn tại trong hệ thống
4. **Performance**: Với dữ liệu lớn, API có thể mất vài giây để xử lý
5. **File size**: File Excel có thể khá lớn tùy thuộc vào số lượng dữ liệu

## Xử lý lỗi

### Lỗi thường gặp:
- **400 Bad Request**: Dữ liệu đầu vào không hợp lệ
- **404 Not Found**: ShopId không tồn tại
- **500 Internal Server Error**: Lỗi server hoặc database

### Troubleshooting:
1. Kiểm tra format ngày tháng
2. Đảm bảo ShopId tồn tại
3. Kiểm tra kết nối database
4. Xem log để biết chi tiết lỗi

## Tính năng nổi bật

- **Tạo động**: Không cần template file, tạo Excel hoàn toàn mới
- **5 sheets phân tích**: Tổng quan, theo ngày, sản phẩm, danh mục, khách hàng
- **Format chuyên nghiệp**: Định dạng số, tỷ lệ phần trăm, biểu đồ
- **Dữ liệu tổng hợp**: Phân tích và thống kê thay vì dữ liệu thô

## Kết luận
API Professional Revenue Report cung cấp một báo cáo doanh thu toàn diện và chuyên nghiệp, phù hợp cho việc phân tích và đưa ra quyết định kinh doanh.
