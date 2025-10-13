# Test Chatbot với Gemini AI

## 1. Kiểm tra Configuration

### Đảm bảo API Key được set đúng:
```bash
# Kiểm tra environment variable
echo $GEMINI_API_KEY

# Hoặc kiểm tra trong appsettings.Development.json
# "ApiKey": "AIzaSyD82T4ovcnr0Sxcx2CXSyUNiHVHEK9gSXQ"
```

## 2. Test API Endpoints

### Test Health Check
```bash
curl -X GET "https://localhost:5001/api/chatbot/health"
```

### Test Sample Questions
```bash
curl -X GET "https://localhost:5001/api/chatbot/sample-questions"
```

### Test Chatbot Question (Doanh thu)
```bash
curl -X POST "https://localhost:5001/api/chatbot/1/ask" \
  -H "Content-Type: application/json" \
  -d '{
    "question": "Doanh thu hôm nay của cửa hàng là bao nhiêu?"
  }'
```

### Test Chatbot Question (Khách hàng)
```bash
curl -X POST "https://localhost:5001/api/chatbot/1/ask" \
  -H "Content-Type: application/json" \
  -d '{
    "question": "Bao nhiêu % khách hàng đã tạo tài khoản thành viên?"
  }'
```

### Test Chatbot Question (Tồn kho)
```bash
curl -X POST "https://localhost:5001/api/chatbot/1/ask" \
  -H "Content-Type: application/json" \
  -d '{
    "question": "Tình hình tồn kho hiện tại ra sao?"
  }'
```

### Test Chatbot Question (Sản phẩm)
```bash
curl -X POST "https://localhost:5001/api/chatbot/1/ask" \
  -H "Content-Type: application/json" \
  -d '{
    "question": "Sản phẩm nào bán chạy nhất?"
  }'
```

## 3. Test Analytics Endpoints

### Test Shop Analytics
```bash
curl -X GET "https://localhost:5001/api/chatbot/1/analytics/shop"
```

### Test Customer Analytics
```bash
curl -X GET "https://localhost:5001/api/chatbot/1/analytics/customers"
```

### Test Revenue Analytics
```bash
curl -X GET "https://localhost:5001/api/chatbot/1/analytics/revenue"
```

### Test Inventory Analytics
```bash
curl -X GET "https://localhost:5001/api/chatbot/1/analytics/inventory"
```

### Test Product Performance
```bash
curl -X GET "https://localhost:5001/api/chatbot/1/analytics/products"
```

## 4. Expected Responses

### Successful Chatbot Response
```json
{
  "question": "Doanh thu hôm nay của cửa hàng là bao nhiêu?",
  "answer": "Dựa trên dữ liệu hiện tại, doanh thu hôm nay của cửa hàng là 2,500,000 VNĐ từ 15 đơn hàng. Trung bình mỗi đơn hàng có giá trị 166,667 VNĐ. 📊",
  "analysisType": "revenue",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

### Error Response (Fallback)
```json
{
  "question": "Doanh thu hôm nay của cửa hàng là bao nhiêu?",
  "answer": "Doanh thu hôm nay của cửa hàng là 2,500,000 VNĐ từ 15 đơn hàng. Trung bình mỗi đơn hàng có giá trị 166,667 VNĐ.",
  "analysisType": "revenue",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

## 5. Troubleshooting

### Lỗi API Key
```
Error: Gemini API key not found
```
**Giải pháp:** Kiểm tra API key trong appsettings.Development.json

### Lỗi Model
```
Error: 400 Bad Request - Model not found
```
**Giải pháp:** Sử dụng model đúng: `gemini-2.0-flash-exp`

### Lỗi Database
```
Error: Shop with ID 1 not found
```
**Giải pháp:** Đảm bảo có dữ liệu trong database

### Lỗi Network
```
Error: Request timeout
```
**Giải pháp:** Kiểm tra internet connection và API key validity

## 6. Debug Mode

### Enable Detailed Logging
```json
{
  "Logging": {
    "LogLevel": {
      "ASA_TENANT_SERVICE": "Debug",
      "ASA_TENANT_SERVICE.Implenment.GeminiService": "Trace"
    }
  }
}
```

### Check Logs
```bash
# Xem logs trong console khi chạy application
dotnet run --verbosity normal
```

## 7. Performance Test

### Test Multiple Questions
```bash
# Test với nhiều câu hỏi cùng lúc
for i in {1..5}; do
  curl -X POST "https://localhost:5001/api/chatbot/1/ask" \
    -H "Content-Type: application/json" \
    -d '{"question": "Doanh thu hôm nay của cửa hàng là bao nhiêu?"}' &
done
wait
```

### Monitor Response Time
- Expected response time: < 3 seconds
- With caching: < 1 second
- Fallback mode: < 0.5 seconds

## 8. Validation Checklist

- [ ] API key is valid and accessible
- [ ] Database connection is working
- [ ] All repositories are properly registered
- [ ] Gemini service is responding
- [ ] Fallback mechanism is working
- [ ] Response format is correct
- [ ] Error handling is proper
- [ ] Logging is working
- [ ] Performance is acceptable
