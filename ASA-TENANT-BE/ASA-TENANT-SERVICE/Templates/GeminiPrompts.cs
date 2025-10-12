using ASA_TENANT_SERVICE.DTOs;

namespace ASA_TENANT_SERVICE.Templates
{
    public static class GeminiPrompts
    {
        public static class SystemPrompts
        {
            public const string BaseSystemPrompt = @"
Bạn là một AI Assistant chuyên nghiệp cho hệ thống quản lý cửa hàng ASA Platform. 
Nhiệm vụ của bạn là phân tích dữ liệu kinh doanh và trả lời câu hỏi về tình hình hoạt động của cửa hàng.

QUY TẮC QUAN TRỌNG:
1. Luôn trả lời bằng tiếng Việt, thân thiện và chuyên nghiệp
2. Sử dụng dữ liệu được cung cấp để đưa ra câu trả lời chính xác
3. Định dạng số tiền theo định dạng Việt Nam (VD: 1.000.000 VNĐ)
4. Đưa ra insights và gợi ý cải thiện khi có thể
5. Nếu không có đủ dữ liệu, hãy nói rõ và đưa ra gợi ý
6. Giữ câu trả lời ngắn gọn, dễ hiểu nhưng đầy đủ thông tin
7. Sử dụng emoji phù hợp để làm câu trả lời sinh động hơn

THÔNG TIN CỬA HÀNG:
- Tên cửa hàng: {ShopName}
- ID cửa hàng: {ShopId}
- Loại câu hỏi: {AnalysisType}
";

            public const string RevenueAnalysisPrompt = @"
Bạn đang phân tích dữ liệu DOANH THU của cửa hàng.

DỮ LIỆU DOANH THU:
{RevenueData}

HƯỚNG DẪN PHÂN TÍCH:
1. Đưa ra số liệu cụ thể với định dạng tiền tệ rõ ràng
2. So sánh các khoảng thời gian khi có thể (hôm nay vs hôm qua, tháng này vs tháng trước)
3. Tính toán tỷ lệ tăng trưởng và giải thích ý nghĩa
4. Phân tích xu hướng và đưa ra dự đoán
5. Gợi ý cách cải thiện doanh thu

ĐỊNH DẠNG TRẢ LỜI:
- Số tiền: 1.000.000 VNĐ (không dùng ký hiệu $)
- Tỷ lệ: 15.5% (làm tròn 1 chữ số thập phân)
- So sánh: tăng/giảm X% so với kỳ trước
- Thời gian: hôm nay, tuần này, tháng này, năm nay
";

            public const string CustomerAnalysisPrompt = @"
Bạn đang phân tích dữ liệu KHÁCH HÀNG của cửa hàng.

DỮ LIỆU KHÁCH HÀNG:
{CustomerData}

HƯỚNG DẪN PHÂN TÍCH:
1. Phân tích tỷ lệ thành viên vs khách vãng lai
2. Đưa ra insights về hành vi khách hàng
3. Phân tích nhân khẩu học (giới tính, hạng thành viên)
4. Xác định khách hàng có giá trị cao
5. Gợi ý chiến lược khách hàng

ĐỊNH DẠNG TRẢ LỜI:
- Tỷ lệ: X% (làm tròn 1 chữ số thập phân)
- Số lượng: X khách hàng
- Chi tiêu trung bình: X VNĐ
- Xu hướng: tăng/giảm X% so với kỳ trước
";

            public const string InventoryAnalysisPrompt = @"
Bạn đang phân tích dữ liệu TỒN KHO của cửa hàng.

DỮ LIỆU TỒN KHO:
{InventoryData}

HƯỚNG DẪN PHÂN TÍCH:
1. Đánh giá tình hình tồn kho tổng thể
2. Xác định sản phẩm cần chú ý (sắp hết, hết hàng)
3. Tính toán giá trị tồn kho và hiệu quả
4. Phân tích sản phẩm bán chạy và chậm
5. Đưa ra khuyến nghị về quản lý kho

ĐỊNH DẠNG TRẢ LỜI:
- Số lượng: X sản phẩm
- Giá trị: X VNĐ
- Tình trạng: Còn hàng/Sắp hết/Hết hàng
- Tỷ lệ: X% sản phẩm cần chú ý
";

            public const string ProductAnalysisPrompt = @"
Bạn đang phân tích dữ liệu SẢN PHẨM của cửa hàng.

DỮ LIỆU SẢN PHẨM:
{ProductData}

HƯỚNG DẪN PHÂN TÍCH:
1. Xác định sản phẩm bán chạy nhất
2. Phân tích sản phẩm có lợi nhuận cao
3. Cảnh báo sản phẩm bán chậm
4. So sánh hiệu suất theo danh mục
5. Đưa ra chiến lược sản phẩm

ĐỊNH DẠNG TRẢ LỜI:
- Hiệu suất: X đơn vị đã bán
- Lợi nhuận: X% (tỷ lệ lợi nhuận)
- Ranking: Top 1, Top 2, Top 3...
- Cảnh báo: Cần chú ý, Cần xem xét
";

            public const string GeneralAnalysisPrompt = @"
Bạn đang phân tích dữ liệu TỔNG QUAN của cửa hàng.

DỮ LIỆU TỔNG QUAN:
{GeneralData}

HƯỚNG DẪN PHÂN TÍCH:
1. Đưa ra bức tranh tổng thể về cửa hàng
2. So sánh các chỉ số quan trọng
3. Xác định điểm mạnh và điểm cần cải thiện
4. Đưa ra đánh giá tổng quan về tình hình kinh doanh
5. Gợi ý các hành động ưu tiên

ĐỊNH DẠNG TRẢ LỜI:
- Tổng quan: X sản phẩm, X khách hàng, X đơn hàng
- Doanh thu: X VNĐ
- Hiệu suất: Trung bình X VNĐ/đơn hàng
- Đánh giá: Tốt/Khá/Cần cải thiện
";
        }

        public static class PromptTemplates
        {
            public static string GetRevenuePrompt(string shopName, long shopId, RevenueAnalyticsDto data)
            {
                return SystemPrompts.BaseSystemPrompt.Replace("{ShopName}", shopName)
                    .Replace("{ShopId}", shopId.ToString())
                    .Replace("{AnalysisType}", "Doanh thu")
                    + SystemPrompts.RevenueAnalysisPrompt.Replace("{RevenueData}", DataFormatters.FormatRevenueData(data));
            }

            public static string GetCustomerPrompt(string shopName, long shopId, CustomerAnalyticsDto data)
            {
                return SystemPrompts.BaseSystemPrompt.Replace("{ShopName}", shopName)
                    .Replace("{ShopId}", shopId.ToString())
                    .Replace("{AnalysisType}", "Khách hàng")
                    + SystemPrompts.CustomerAnalysisPrompt.Replace("{CustomerData}", DataFormatters.FormatCustomerData(data));
            }

            public static string GetInventoryPrompt(string shopName, long shopId, InventoryAnalyticsDto data)
            {
                return SystemPrompts.BaseSystemPrompt.Replace("{ShopName}", shopName)
                    .Replace("{ShopId}", shopId.ToString())
                    .Replace("{AnalysisType}", "Tồn kho")
                    + SystemPrompts.InventoryAnalysisPrompt.Replace("{InventoryData}", DataFormatters.FormatInventoryData(data));
            }

            public static string GetProductPrompt(string shopName, long shopId, ProductPerformanceDto data)
            {
                return SystemPrompts.BaseSystemPrompt.Replace("{ShopName}", shopName)
                    .Replace("{ShopId}", shopId.ToString())
                    .Replace("{AnalysisType}", "Sản phẩm")
                    + SystemPrompts.ProductAnalysisPrompt.Replace("{ProductData}", DataFormatters.FormatProductData(data));
            }

            public static string GetGeneralPrompt(string shopName, long shopId, ShopAnalyticsDto data)
            {
                return SystemPrompts.BaseSystemPrompt.Replace("{ShopName}", shopName)
                    .Replace("{ShopId}", shopId.ToString())
                    .Replace("{AnalysisType}", "Tổng quan")
                    + SystemPrompts.GeneralAnalysisPrompt.Replace("{GeneralData}", DataFormatters.FormatGeneralData(data));
            }
        }

        public static class DataFormatters
        {
            public static string FormatRevenueData(RevenueAnalyticsDto data)
            {
                var growthText = data.RevenueGrowth > 0 ? $"tăng {data.RevenueGrowth:F1}%" : 
                               data.RevenueGrowth < 0 ? $"giảm {Math.Abs(data.RevenueGrowth):F1}%" : "không thay đổi";

                return $@"
TỔNG DOANH THU: {data.TotalRevenue:N0} VNĐ
DOANH THU HÔM NAY: {data.TodayRevenue:N0} VNĐ ({data.TodayOrders} đơn hàng)
DOANH THU TUẦN NÀY: {data.ThisWeekRevenue:N0} VNĐ ({data.ThisWeekOrders} đơn hàng)
DOANH THU THÁNG NÀY: {data.ThisMonthRevenue:N0} VNĐ ({data.ThisMonthOrders} đơn hàng)
DOANH THU THÁNG TRƯỚC: {data.LastMonthRevenue:N0} VNĐ
TĂNG TRƯỞNG: {growthText}
TRUNG BÌNH ĐƠN HÀNG: {data.AverageOrderValue:N0} VNĐ
TỔNG ĐƠN HÀNG: {data.TotalOrders}

PHÂN BỐ THEO PHƯƠNG THỨC THANH TOÁN:
{string.Join("\n", data.RevenueByPaymentMethod.Select(kvp => $"- {kvp.Key}: {kvp.Value:N0} VNĐ"))}

PHÂN BỐ THEO DANH MỤC:
{string.Join("\n", data.RevenueByCategory.Select(kvp => $"- {kvp.Key}: {kvp.Value:N0} VNĐ"))}
";
            }

            public static string FormatCustomerData(CustomerAnalyticsDto data)
            {
                var topRank = data.CustomersByRank.OrderByDescending(x => x.Value).FirstOrDefault();
                var topGender = data.CustomersByGender.OrderByDescending(x => x.Value).FirstOrDefault();

                return $@"
TỔNG KHÁCH HÀNG: {data.TotalCustomers}
THÀNH VIÊN: {data.MemberCustomers} ({data.MemberPercentage:F1}%)
KHÁCH VÃNG LAI: {data.NonMemberCustomers} ({data.NonMemberPercentage:F1}%)
KHÁCH HÀNG MỚI THÁNG NÀY: {data.NewCustomersThisMonth}
KHÁCH HÀNG QUAY LẠI: {data.ReturningCustomers}
CHI TIÊU TRUNG BÌNH: {data.AverageCustomerSpent:N0} VNĐ

PHÂN BỐ THEO HẠNG THÀNH VIÊN:
{string.Join("\n", data.CustomersByRank.Select(kvp => $"- {kvp.Key}: {kvp.Value} khách"))}

PHÂN BỐ THEO GIỚI TÍNH:
{string.Join("\n", data.CustomersByGender.Select(kvp => $"- {kvp.Key}: {kvp.Value} khách"))}

TOP KHÁCH HÀNG CHI TIÊU NHIỀU NHẤT:
{string.Join("\n", data.TopCustomers.TopSpenders.Take(3).Select((c, i) => $"{i + 1}. {c.FullName} - {c.TotalSpent:N0} VNĐ ({c.TotalOrders} đơn hàng)"))}

KHÁCH HÀNG MUA HÀNG THƯỜNG XUYÊN:
{string.Join("\n", data.TopCustomers.MostFrequent.Take(3).Select((c, i) => $"{i + 1}. {c.FullName} - {c.TotalOrders} đơn hàng"))}
";
            }

            public static string FormatInventoryData(InventoryAnalyticsDto data)
            {
                return $@"
TỔNG SẢN PHẨM: {data.TotalProducts}
CÒN HÀNG: {data.InStockProducts}
SẮP HẾT HÀNG: {data.LowStockProducts}
HẾT HÀNG: {data.OutOfStockProducts}
GIÁ TRỊ TỒN KHO: {data.TotalInventoryValue:N0} VNĐ

SẢN PHẨM SẮP HẾT HÀNG:
{string.Join("\n", data.LowStockItems.Take(5).Select(p => $"- {p.ProductName}: còn {p.CurrentStock} (Danh mục: {p.CategoryName})"))}

SẢN PHẨM BÁN CHẠY NHẤT:
{string.Join("\n", data.TopSellingProducts.Take(5).Select((p, i) => $"{i + 1}. {p.ProductName}: {p.TotalSold} đơn vị - {p.TotalRevenue:N0} VNĐ"))}

SẢN PHẨM BÁN CHẬM:
{string.Join("\n", data.SlowMovingProducts.Take(5).Select(p => $"- {p.ProductName}: {p.TotalSold} đơn vị"))}

PHÂN BỐ THEO DANH MỤC:
{string.Join("\n", data.ProductsByCategory.Select(kvp => $"- {kvp.Key}: {kvp.Value} sản phẩm"))}
";
            }

            public static string FormatProductData(ProductPerformanceDto data)
            {
                return $@"
TOP SẢN PHẨM BÁN CHẠY:
{string.Join("\n", data.TopSellingProducts.Take(5).Select((p, i) => $"{i + 1}. {p.ProductName}: {p.TotalSold} đơn vị - {p.TotalRevenue:N0} VNĐ"))}

SẢN PHẨM BÁN CHẬM:
{string.Join("\n", data.WorstSellingProducts.Take(5).Select(p => $"- {p.ProductName}: {p.TotalSold} đơn vị"))}

SẢN PHẨM CÓ LỢI NHUẬN CAO NHẤT:
{string.Join("\n", data.MostProfitableProducts.Take(5).Select((p, i) => $"{i + 1}. {p.ProductName}: {p.ProfitMargin:F1}% lợi nhuận"))}

SẢN PHẨM CẦN CHÚ Ý:
{string.Join("\n", data.ProductsNeedAttention.Take(5).Select(p => $"- {p.ProductName}: {p.StockStatus} - {p.TotalSold} đã bán"))}

HIỆU SUẤT THEO DANH MỤC:
{string.Join("\n", data.CategoryPerformance.Select(kvp => $"- {kvp.Key}: {kvp.Value.ProductCount} sản phẩm, {kvp.Value.TotalRevenue:N0} VNĐ, {kvp.Value.AverageProfitMargin:F1}% lợi nhuận TB"))}
";
            }

            public static string FormatGeneralData(ShopAnalyticsDto data)
            {
                return $@"
TÊN CỬA HÀNG: {data.ShopName}
NGÀY THÀNH LẬP: {data.CreatedAt:dd/MM/yyyy}
TỔNG SẢN PHẨM: {data.TotalProducts}
TỔNG KHÁCH HÀNG: {data.TotalCustomers}
TỔNG ĐƠN HÀNG: {data.TotalOrders}
TỔNG DOANH THU: {data.TotalRevenue:N0} VNĐ
TRUNG BÌNH ĐƠN HÀNG: {data.AverageOrderValue:N0} VNĐ
TRẠNG THÁI: {data.Status}
";
            }
        }

        public static class QuestionTypes
        {
            public static readonly Dictionary<string, string[]> QuestionPatterns = new()
            {
                ["revenue"] = new[]
                {
                    "doanh thu", "revenue", "tiền", "bán", "thu nhập", "lợi nhuận",
                    "tổng thu", "thu về", "kiếm được", "bán được", "doanh số",
                    "hôm nay", "tuần này", "tháng này", "năm nay", "so với"
                },
                ["customer"] = new[]
                {
                    "khách hàng", "customer", "member", "thành viên", "khách", "client",
                    "người mua", "buyer", "người dùng", "user", "tỷ lệ", "%",
                    "giới tính", "hạng", "rank", "mới", "quay lại"
                },
                ["inventory"] = new[]
                {
                    "tồn kho", "inventory", "hàng", "sản phẩm", "stock", "kho", "tồn",
                    "số lượng", "quantity", "còn lại", "còn", "hết", "out of stock",
                    "sắp hết", "low stock", "cần nhập", "cảnh báo"
                },
                ["product"] = new[]
                {
                    "sản phẩm", "product", "mặt hàng", "bán chạy", "ít bán", "item",
                    "hàng hóa", "goods", "bán nhiều", "bán ít", "performance",
                    "lợi nhuận", "profit", "hiệu suất", "chậm", "nhanh"
                }
            };

            public static string DetermineQuestionType(string question)
            {
                var questionLower = question.ToLower();
                
                foreach (var kvp in QuestionPatterns)
                {
                    if (kvp.Value.Any(keyword => questionLower.Contains(keyword)))
                    {
                        return kvp.Key;
                    }
                }
                
                return "general";
            }
        }
    }
}
