# Hướng dẫn Setup Gemini API cho ASA Tenant Chatbot

## 1. Lấy Gemini API Key

### Bước 1: Truy cập Google AI Studio
1. Đi tới [Google AI Studio](https://aistudio.google.com/)
2. Đăng nhập bằng tài khoản Google của bạn
3. Chọn "Get API Key" hoặc "Create API Key"

### Bước 2: Tạo API Key mới
1. Click "Create API Key"
2. Chọn project hoặc tạo project mới
3. Copy API key được tạo

## 2. Cấu hình API Key

### Cách 1: Environment Variable (Khuyến nghị)
```bash
# Windows (PowerShell)
$env:GEMINI_API_KEY="your-api-key-here"

# Windows (CMD)
set GEMINI_API_KEY=your-api-key-here

# Linux/Mac
export GEMINI_API_KEY="your-api-key-here"
```

### Cách 2: appsettings.json
```json
{
  "GeminiSettings": {
    "ApiKey": "your-api-key-here",
    "Model": "gemini-1.5-flash-latest",
    "MaxTokens": 2048,
    "Temperature": 0.7,
    "TimeoutSeconds": 30
  }
}
```

### Cách 3: User Secrets (Development)
```bash
dotnet user-secrets set "GeminiSettings:ApiKey" "your-api-key-here"
```

## 3. Cấu hình Project

### Thêm NuGet Package
```bash
dotnet add package Google.Cloud.AI.GenerativeLanguage.V1Beta
```

### Đăng ký Services trong Program.cs
```csharp
using ASA_TENANT_SERVICE.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Thêm chatbot services với Gemini AI
builder.Services.AddChatbotWithFallback(builder.Configuration);

var app = builder.Build();
```

### Cấu hình trong appsettings.json
```json
{
  "GeminiSettings": {
    "ApiKey": "",
    "Model": "gemini-1.5-flash-latest",
    "MaxTokens": 2048,
    "Temperature": 0.7,
    "TimeoutSeconds": 30
  },
  "ChatbotSettings": {
    "EnableGeminiAI": true,
    "FallbackToHardcoded": true,
    "CacheResponses": true,
    "CacheExpiryMinutes": 5,
    "MaxRetries": 3,
    "RetryDelaySeconds": 2
  }
}
```

## 4. Test API Connection

### Test Endpoint
```bash
curl -X POST "https://localhost:5001/api/chatbot/1/ask" \
  -H "Content-Type: application/json" \
  -d '{
    "question": "Doanh thu hôm nay của cửa hàng là bao nhiêu?"
  }'
```

### Expected Response
```json
{
  "question": "Doanh thu hôm nay của cửa hàng là bao nhiêu?",
  "answer": "Dựa trên dữ liệu hiện tại, doanh thu hôm nay của cửa hàng là 2,500,000 VNĐ từ 15 đơn hàng. Trung bình mỗi đơn hàng có giá trị 166,667 VNĐ. 📊",
  "analysisType": "revenue",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

## 5. Troubleshooting

### Lỗi thường gặp

#### 1. API Key không hợp lệ
```
Error: Gemini API key not found
```
**Giải pháp:**
- Kiểm tra API key đã được set đúng chưa
- Verify API key trong Google AI Studio
- Kiểm tra environment variable hoặc configuration

#### 2. Quota exceeded
```
Error: 429 Too Many Requests
```
**Giải pháp:**
- Kiểm tra quota trong Google AI Studio
- Implement rate limiting
- Sử dụng cache để giảm API calls

#### 3. Model not found
```
Error: 400 Bad Request - Model not found
```
**Giải pháp:**
- Kiểm tra model name trong configuration
- Sử dụng model có sẵn: `gemini-1.5-flash-latest`

#### 4. Timeout
```
Error: Request timeout
```
**Giải pháp:**
- Tăng `TimeoutSeconds` trong configuration
- Optimize prompt length
- Implement retry logic

### Debug Mode

#### Enable Detailed Logging
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

#### Check API Status
```bash
curl -X GET "https://localhost:5001/api/chatbot/health"
```

## 6. Performance Optimization

### Caching Strategy
```csharp
// Enable response caching
"ChatbotSettings": {
  "CacheResponses": true,
  "CacheExpiryMinutes": 5
}
```

### Rate Limiting
```csharp
// Implement rate limiting
"ChatbotSettings": {
  "MaxRetries": 3,
  "RetryDelaySeconds": 2
}
```

### Prompt Optimization
- Sử dụng prompt templates có sẵn
- Giữ prompt ngắn gọn nhưng đầy đủ
- Sử dụng structured data format

## 7. Security Best Practices

### API Key Security
- Không commit API key vào source code
- Sử dụng environment variables
- Rotate API keys định kỳ
- Monitor API usage

### Input Validation
- Validate user input trước khi gửi đến Gemini
- Sanitize sensitive data
- Implement request size limits

### Error Handling
- Không expose internal errors
- Log errors securely
- Implement graceful fallbacks

## 8. Monitoring & Analytics

### Logging
```csharp
_logger.LogInformation("Gemini API call for shop {ShopId}, tokens used: {Tokens}", shopId, tokensUsed);
```

### Metrics
- Track API response times
- Monitor success/failure rates
- Track token usage
- Monitor costs

### Health Checks
```csharp
[HttpGet("health")]
public ActionResult<object> HealthCheck()
{
    return Ok(new { 
        status = "healthy", 
        geminiEnabled = _chatbotConfig.EnableGeminiAI,
        timestamp = DateTime.UtcNow
    });
}
```

## 9. Advanced Configuration

### Custom Models
```json
{
  "GeminiSettings": {
    "Model": "gemini-1.5-pro-latest",  // For more complex analysis
    "Temperature": 0.3,                // More deterministic
    "MaxTokens": 4096                  // Longer responses
  }
}
```

### Fallback Strategy
```json
{
  "ChatbotSettings": {
    "EnableGeminiAI": true,
    "FallbackToHardcoded": true,      // Fallback nếu Gemini fail
    "CacheResponses": true
  }
}
```

## 10. Cost Management

### Token Usage
- Monitor token consumption
- Optimize prompts để giảm tokens
- Implement caching để giảm API calls
- Set usage limits

### Pricing (approximate)
- Gemini 1.5 Flash: ~$0.075 per 1M input tokens, ~$0.30 per 1M output tokens
- Estimate cost: ~$0.001-0.005 per query

### Cost Optimization
- Cache frequent queries
- Batch similar requests
- Use shorter prompts
- Implement smart caching strategies
