# AI Chatbot cho ASA Tenant - Hướng dẫn sử dụng

## Tổng quan

AI Chatbot được thiết kế để trả lời các câu hỏi về tình hình kinh doanh của cửa hàng, bao gồm:
- **Doanh thu**: Phân tích doanh thu theo ngày, tuần, tháng
- **Khách hàng**: Thống kê khách hàng, tỷ lệ thành viên, phân tích nhân khẩu học
- **Tồn kho**: Tình hình tồn kho, sản phẩm sắp hết hàng
- **Sản phẩm**: Hiệu suất bán hàng, sản phẩm bán chạy/chậm

## API Endpoints

### 1. Gửi câu hỏi cho chatbot
```
POST /api/chatbot/{shopId}/ask
```

**Request Body:**
```json
{
  "question": "Doanh thu hôm nay của cửa hàng là bao nhiêu?",
  "context": "Cần thông tin chi tiết",
  "dateRangeStart": "2024-01-01",
  "dateRangeEnd": "2024-01-31"
}
```

**Response:**
```json
{
  "question": "Doanh thu hôm nay của cửa hàng là bao nhiêu?",
  "answer": "Doanh thu hôm nay của cửa hàng là 2,500,000 VNĐ từ 15 đơn hàng. Trung bình mỗi đơn hàng có giá trị 166,667 VNĐ.",
  "analysisType": "revenue",
  "data": {
    "todayRevenue": 2500000,
    "todayOrders": 15,
    "averageOrderValue": 166667
  },
  "timestamp": "2024-01-15T10:30:00Z"
}
```

### 2. Lấy thông tin tổng quan cửa hàng
```
GET /api/chatbot/{shopId}/analytics/shop
```

### 3. Lấy phân tích khách hàng
```
GET /api/chatbot/{shopId}/analytics/customers
```

### 4. Lấy phân tích tồn kho
```
GET /api/chatbot/{shopId}/analytics/inventory
```

### 5. Lấy phân tích doanh thu
```
GET /api/chatbot/{shopId}/analytics/revenue?startDate=2024-01-01&endDate=2024-01-31
```

### 6. Lấy phân tích hiệu suất sản phẩm
```
GET /api/chatbot/{shopId}/analytics/products
```

### 7. Lấy câu hỏi mẫu
```
GET /api/chatbot/sample-questions
```

## Các loại câu hỏi được hỗ trợ

### 📊 Doanh thu
- "Doanh thu hôm nay của cửa hàng là bao nhiêu?"
- "Doanh thu tuần này so với tuần trước như thế nào?"
- "Doanh thu tháng này tăng hay giảm so với tháng trước?"
- "Trung bình mỗi đơn hàng có giá trị bao nhiêu?"
- "Phương thức thanh toán nào được sử dụng nhiều nhất?"

### 👥 Khách hàng
- "Bao nhiêu % khách hàng đã tạo tài khoản thành viên?"
- "Khách hàng chủ yếu của cửa hàng là ai?"
- "Có bao nhiêu khách hàng mới trong tháng này?"
- "Khách hàng nào chi tiêu nhiều nhất?"
- "Phân bố khách hàng theo giới tính như thế nào?"

### 📦 Tồn kho
- "Tình hình tồn kho hiện tại ra sao?"
- "Sản phẩm nào sắp hết hàng cần nhập thêm?"
- "Giá trị tồn kho hiện tại là bao nhiêu?"
- "Sản phẩm nào đã hết hàng?"
- "Danh mục nào có nhiều sản phẩm nhất?"

### 🛍️ Sản phẩm
- "Sản phẩm nào bán chạy nhất?"
- "Sản phẩm nào bán chậm cần chú ý?"
- "Sản phẩm nào có lợi nhuận cao nhất?"
- "Những sản phẩm nào cần được chú ý?"
- "Hiệu suất bán hàng theo từng danh mục?"

### 📈 Tổng quan
- "Tổng quan về tình hình kinh doanh của cửa hàng?"
- "Cửa hàng có bao nhiêu sản phẩm và khách hàng?"
- "Thống kê tổng quan về cửa hàng"

## Cấu trúc dữ liệu

### ShopAnalyticsDto
```csharp
{
  "shopId": 1,
  "shopName": "Cửa hàng ABC",
  "createdAt": "2024-01-01T00:00:00Z",
  "totalProducts": 150,
  "totalCustomers": 500,
  "totalOrders": 1200,
  "totalRevenue": 50000000,
  "averageOrderValue": 41667,
  "status": "Active"
}
```

### CustomerAnalyticsDto
```csharp
{
  "shopId": 1,
  "totalCustomers": 500,
  "memberCustomers": 300,
  "nonMemberCustomers": 200,
  "memberPercentage": 60.0,
  "nonMemberPercentage": 40.0,
  "newCustomersThisMonth": 25,
  "returningCustomers": 475,
  "customersByRank": {
    "Gold": 50,
    "Silver": 100,
    "Bronze": 150
  },
  "customersByGender": {
    "Nam": 250,
    "Nữ": 200,
    "Khác": 50
  },
  "averageCustomerSpent": 100000,
  "topCustomers": {
    "topSpenders": [...],
    "mostFrequent": [...],
    "recentCustomers": [...]
  }
}
```

### RevenueAnalyticsDto
```csharp
{
  "shopId": 1,
  "totalRevenue": 50000000,
  "todayRevenue": 2500000,
  "thisWeekRevenue": 15000000,
  "thisMonthRevenue": 50000000,
  "lastMonthRevenue": 45000000,
  "revenueGrowth": 11.1,
  "averageOrderValue": 41667,
  "totalOrders": 1200,
  "todayOrders": 15,
  "thisWeekOrders": 90,
  "thisMonthOrders": 300,
  "revenueByPaymentMethod": {
    "Cash": 20000000,
    "Card": 25000000,
    "ZaloPay": 5000000
  },
  "revenueByCategory": {
    "Thực phẩm": 30000000,
    "Đồ uống": 20000000
  },
  "dailyRevenue": [...],
  "monthlyRevenue": [...]
}
```

## Lưu ý kỹ thuật

### 1. Xử lý lỗi
- Tất cả API đều có error handling
- Trả về HTTP status codes phù hợp
- Log chi tiết các lỗi để debug

### 2. Performance
- Sử dụng async/await cho tất cả database operations
- Cache dữ liệu thường xuyên truy cập (nếu cần)
- Optimize queries để giảm thời gian response

### 3. Security
- Validate shopId để đảm bảo user chỉ truy cập dữ liệu của shop mình
- Sanitize input để tránh injection attacks
- Rate limiting cho API calls

### 4. Scalability
- Service có thể scale horizontal
- Database queries được optimize
- Có thể thêm caching layer nếu cần

## Cách sử dụng trong Frontend

### JavaScript/TypeScript
```javascript
// Gửi câu hỏi
const askQuestion = async (shopId, question) => {
  const response = await fetch(`/api/chatbot/${shopId}/ask`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({ question })
  });
  
  return await response.json();
};

// Lấy thông tin doanh thu
const getRevenue = async (shopId) => {
  const response = await fetch(`/api/chatbot/${shopId}/analytics/revenue`);
  return await response.json();
};
```

### React Component Example
```jsx
const ChatbotComponent = ({ shopId }) => {
  const [question, setQuestion] = useState('');
  const [answer, setAnswer] = useState('');
  
  const handleAsk = async () => {
    const response = await askQuestion(shopId, question);
    setAnswer(response.answer);
  };
  
  return (
    <div>
      <input 
        value={question}
        onChange={(e) => setQuestion(e.target.value)}
        placeholder="Hỏi về tình hình kinh doanh..."
      />
      <button onClick={handleAsk}>Hỏi</button>
      {answer && <div>{answer}</div>}
    </div>
  );
};
```

## Mở rộng

### Thêm loại câu hỏi mới
1. Thêm keywords vào `ChatbotPrompts.Keywords`
2. Thêm response templates vào `ChatbotPrompts.ResponseTemplates`
3. Implement logic xử lý trong `ChatbotService`
4. Thêm sample questions vào `ChatbotPrompts.SampleQuestions`

### Thêm analytics mới
1. Tạo DTO mới trong `ChatbotDtos.cs`
2. Thêm method vào `IChatbotService`
3. Implement trong `ChatbotService`
4. Thêm endpoint vào `ChatbotController`

## Troubleshooting

### Lỗi thường gặp
1. **404 Not Found**: Kiểm tra shopId có tồn tại không
2. **500 Internal Server Error**: Kiểm tra logs để xem chi tiết lỗi
3. **Empty response**: Kiểm tra dữ liệu trong database
4. **Slow response**: Optimize database queries hoặc thêm caching

### Debug
- Enable detailed logging
- Kiểm tra database connections
- Monitor performance metrics
- Test với dữ liệu mẫu
