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
6. TRẢ LỜI NGẮN GỌN, ĐI THẲNG VÀO VẤN ĐỀ, KHÔNG DÀI DÒNG
7. Sử dụng định dạng văn bản đơn giản, không emoji, không icon
8. Không sử dụng bảng, chỉ dùng danh sách có dấu đầu dòng
9. Tránh sử dụng các ký hiệu đặc biệt và biểu tượng
10. KHÔNG lặp lại thông tin cửa hàng trừ khi được hỏi cụ thể

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

            public const string StrategyAnalysisPrompt = @"
Bạn là chuyên gia tư vấn chiến lược kinh doanh cho cửa hàng.

DỮ LIỆU CHIẾN LƯỢC:
{StrategyData}

NHIỆM VỤ:
1. Phân tích tình hình hiện tại của cửa hàng dựa trên dữ liệu
2. Đưa ra chiến lược cụ thể để cải thiện doanh thu, khách hàng, sản phẩm
3. Đưa ra các biện pháp thực thi cụ thể và khả thi
4. Ưu tiên các giải pháp có tác động lớn nhất

HƯỚNG DẪN TƯ VẤN:
- Phân tích SWOT (Điểm mạnh, điểm yếu, cơ hội, thách thức)
- Đưa ra chiến lược ngắn hạn (1-3 tháng) và dài hạn (3-12 tháng)
- Tập trung vào các chỉ số quan trọng: doanh thu, khách hàng, sản phẩm
- Đưa ra các biện pháp marketing, bán hàng, quản lý cụ thể

ĐỊNH DẠNG TRẢ LỜI:
- Sử dụng định dạng văn bản đơn giản, không emoji
- Chia thành các phần: Phân tích → Chiến lược → Biện pháp cụ thể
- Đưa ra timeline và mục tiêu cụ thể
- Kết thúc bằng action items ưu tiên
- Sử dụng danh sách có dấu đầu dòng thay vì bảng
";

            public const string ProductSuggestionAnalysisPrompt = @"
Bạn là chuyên gia tư vấn sản phẩm tạp hóa với kiến thức sâu về xu hướng thị trường.

DỮ LIỆU SẢN PHẨM:
{ProductSuggestionData}

NHIỆM VỤ:
1. Phân tích tình hình sản phẩm hiện tại của cửa hàng
2. Đưa ra gợi ý sản phẩm hot trên thị trường dựa trên xu hướng
3. Đề xuất sản phẩm phù hợp với đối tượng khách hàng
4. Tối ưu hóa danh mục sản phẩm để tăng doanh thu

HƯỚNG DẪN GỢI Ý:
- Phân tích xu hướng thị trường hiện tại (2024)
- Đưa ra sản phẩm theo mùa và thời điểm
- Gợi ý sản phẩm dựa trên hiệu suất hiện tại của cửa hàng
- Bao gồm cả sản phẩm truyền thống và sản phẩm mới nổi
- Đưa ra lý do tại sao sản phẩm đó phù hợp

ĐỊNH DẠNG TRẢ LỜI:
- Sử dụng định dạng văn bản đơn giản, không emoji
- Chia thành các danh mục: Đồ uống, Thực phẩm, Tiện lợi, Sức khỏe
- Đưa ra sản phẩm cụ thể với thương hiệu
- Giải thích lý do tại sao sản phẩm đó hot
- Đưa ra gợi ý về giá và lợi nhuận
- Sử dụng danh sách có dấu đầu dòng thay vì bảng
";

            public const string ComprehensiveAnalysisPrompt = @"
Bạn là chuyên gia tư vấn kinh doanh với khả năng phân tích thông minh.

NHIỆM VỤ:
1. Trả lời câu hỏi cụ thể của khách hàng một cách chính xác và hữu ích
2. Chỉ sử dụng thông tin cửa hàng làm context, KHÔNG trả lời lại thông tin đó trừ khi được hỏi cụ thể
3. Tập trung vào câu trả lời ngắn gọn, súc tích và thực tế
4. Đưa ra gợi ý cụ thể có thể áp dụng ngay

HƯỚNG DẪN TRẢ LỜI:
- Đọc kỹ câu hỏi và trả lời đúng trọng tâm
- KHÔNG lặp lại thông tin cửa hàng trừ khi câu hỏi về doanh thu, thông tin cửa hàng
- Sử dụng dữ liệu cửa hàng để đưa ra gợi ý phù hợp với tình hình thực tế
- Trả lời ngắn gọn, đi thẳng vào vấn đề
- Đưa ra số liệu cụ thể khi cần thiết

ĐỊNH DẠNG TRẢ LỜI:
- Sử dụng định dạng văn bản đơn giản, không emoji
- Trả lời trực tiếp câu hỏi, không cần phần 'Tổng quan' hay 'Phân tích chi tiết'
- Chia thành các phần nhỏ nếu cần: Gợi ý → Lý do → Cách thực hiện
- Sử dụng danh sách có dấu đầu dòng thay vì bảng
- Kết thúc bằng lời khuyên thực tế
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

            public static string GetStrategyPrompt(string shopName, long shopId, StrategyAnalyticsDto data)
            {
                return SystemPrompts.BaseSystemPrompt.Replace("{ShopName}", shopName)
                    .Replace("{ShopId}", shopId.ToString())
                    .Replace("{AnalysisType}", "Chiến lược")
                    + SystemPrompts.StrategyAnalysisPrompt.Replace("{StrategyData}", DataFormatters.FormatStrategyData(data));
            }

            public static string GetProductSuggestionPrompt(string shopName, long shopId, ProductSuggestionDto data)
            {
                return SystemPrompts.BaseSystemPrompt.Replace("{ShopName}", shopName)
                    .Replace("{ShopId}", shopId.ToString())
                    .Replace("{AnalysisType}", "Gợi ý sản phẩm")
                    + SystemPrompts.ProductSuggestionAnalysisPrompt.Replace("{ProductSuggestionData}", DataFormatters.FormatProductSuggestionData(data));
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

            public static string FormatStrategyData(StrategyAnalyticsDto data)
            {
                var topProducts = string.Join("\n", data.TopSellingProducts.Take(5).Select(p => 
                    $"- {p.ProductName}: {p.TotalSold} đơn vị, {p.Revenue:N0} VNĐ (lợi nhuận {p.ProfitMargin:F1}%)"));
                
                var topCategories = string.Join("\n", data.TopCategories.Select(c => 
                    $"- {c.CategoryName}: {c.Revenue:N0} VNĐ"));

                return $@"
DỮ LIỆU CHIẾN LƯỢC CỬA HÀNG {data.ShopName}:

DOANH THU:
- Doanh thu tháng này: {data.ThisMonthRevenue:N0} VNĐ
- Doanh thu tháng trước: {data.LastMonthRevenue:N0} VNĐ
- Tăng trưởng: {data.RevenueGrowth:F1}%
- Tổng doanh thu: {data.TotalRevenue:N0} VNĐ
- Giá trị đơn hàng trung bình: {data.AverageOrderValue:N0} VNĐ
- Tổng đơn hàng: {data.TotalOrders}

KHÁCH HÀNG:
- Tổng khách hàng: {data.TotalCustomers}
- Khách hàng thành viên: {data.MemberCustomers} ({data.MemberPercentage:F1}%)
- Khách hàng chưa thành viên: {data.NonMemberCustomers}

SẢN PHẨM:
- Tổng sản phẩm: {data.TotalProducts}
- Sản phẩm sắp hết hàng: {data.LowStockProducts}
- Sản phẩm hết hàng: {data.OutOfStockProducts}

SẢN PHẨM BÁN CHẠY:
{topProducts}

DANH MỤC BÁN CHẠY:
{topCategories}
";
            }

            public static string FormatProductSuggestionData(ProductSuggestionDto data)
            {
                var topProducts = string.Join("\n", data.TopSellingProducts.Take(5).Select(p => 
                    $"- {p.ProductName} ({p.CategoryName}): {p.TotalSold} đã bán, {p.Revenue:N0} VNĐ, lợi nhuận {p.ProfitMargin:F1}%"));
                
                var topCategories = string.Join("\n", data.CategoriesPerformance.Take(3).Select(c => 
                    $"- {c.CategoryName}: {c.ProductCount} sản phẩm, {c.TotalRevenue:N0} VNĐ, {c.AverageProfitMargin:F1}% lợi nhuận TB"));

                var lowStockProducts = string.Join("\n", data.LowStockProducts.Take(3).Select(p => 
                    $"- {p.ProductName}: còn {p.CurrentStock} đơn vị"));

                return $@"
DỮ LIỆU GỢI Ý SẢN PHẨM CỬA HÀNG {data.ShopName}:

TÌNH HÌNH HIỆN TẠI:
- Tổng sản phẩm: {data.CurrentProductsCount}
- Tổng doanh thu: {data.TotalRevenue:N0} VNĐ
- Giá trị đơn hàng trung bình: {data.AverageOrderValue:N0} VNĐ

SẢN PHẨM BÁN CHẠY HIỆN TẠI:
{topProducts}

DANH MỤC HIỆU QUẢ NHẤT:
{topCategories}

SẢN PHẨM CẦN BỔ SUNG:
{lowStockProducts}

SẢN PHẨM BÁN CHẬM:
{string.Join("\n", data.SlowMovingProducts.Take(3).Select(p => $"- {p.ProductName}: chỉ bán {p.TotalSold} đơn vị"))}
";
            }

        public static string FormatComprehensiveData(ComprehensiveAnalysisDto data)
        {
            var shopSummary = $@"
THÔNG TIN CỬA HÀNG:
- Tên cửa hàng: {data.ShopName}
- ID cửa hàng: {data.ShopId}
- Tổng sản phẩm: {data.ShopData.TotalProducts}
- Tổng khách hàng: {data.ShopData.TotalCustomers}
- Tổng đơn hàng: {data.ShopData.TotalOrders}
- Tổng doanh thu: {data.ShopData.TotalRevenue:N0} VNĐ
- Giá trị đơn hàng TB: {data.ShopData.AverageOrderValue:N0} VNĐ";

            var strategySummary = $@"
CHIẾN LƯỢC:
- Doanh thu tháng này: {data.StrategyData.ThisMonthRevenue:N0} VNĐ
- Tăng trưởng: {data.StrategyData.RevenueGrowth:F1}%
- Tổng khách hàng: {data.StrategyData.TotalCustomers} (Thành viên: {data.StrategyData.MemberCustomers})
- Sản phẩm: {data.StrategyData.TotalProducts} (Hết hàng: {data.StrategyData.OutOfStockProducts})";

            var productSummary = $@"
SẢN PHẨM:
- Tổng sản phẩm: {data.ProductData.CurrentProductsCount}
- Sản phẩm bán chạy: {data.ProductData.TopSellingProducts.Count}
- Sản phẩm sắp hết: {data.ProductData.LowStockProducts.Count}";

            var revenueSummary = $@"
DOANH THU:
- Tổng doanh thu: {data.RevenueData.TotalRevenue:N0} VNĐ
- Tháng này: {data.RevenueData.ThisMonthRevenue:N0} VNĐ
- Tháng trước: {data.RevenueData.LastMonthRevenue:N0} VNĐ";

            var customerSummary = $@"
KHÁCH HÀNG:
- Tổng khách hàng: {data.CustomerData.TotalCustomers}
- Thành viên: {data.CustomerData.MemberCustomers} ({data.CustomerData.MemberPercentage:F1}%)
- Chưa thành viên: {data.CustomerData.NonMemberCustomers}";

            var inventorySummary = $@"
TỒN KHO:
- Tổng sản phẩm: {data.InventoryData.TotalProducts}
- Sắp hết hàng: {data.InventoryData.LowStockProducts}
- Hết hàng: {data.InventoryData.OutOfStockProducts}
- Sản phẩm sắp hết hàng: {string.Join(", ", data.InventoryData.LowStockItems.Select(p => $"{p.ProductName} (còn {p.CurrentStock})"))}";

            return $@"
DỮ LIỆU TOÀN DIỆN CỬA HÀNG {data.ShopName}:

{shopSummary}

{strategySummary}

{productSummary}

{revenueSummary}

{customerSummary}

{inventorySummary}

CÂU HỎI: {data.Question}
LOẠI PHÂN TÍCH: {data.AnalysisType}
";
        }
        }

        public static class QuestionTypes
        {
            public static readonly Dictionary<string, string[]> QuestionPatterns = new()
            {
                ["strategy"] = new[]
                {
                    "chiến lược", "strategy", "cách tăng", "làm sao", "làm thế nào", "gợi ý", "suggestion",
                    "cải thiện", "improve", "tăng trưởng", "growth", "phát triển", "development",
                    "kế hoạch", "plan", "giải pháp", "solution", "biện pháp", "measure",
                    "tăng doanh thu", "tăng khách hàng", "tăng bán hàng", "marketing", "quảng cáo",
                    "khuyến mãi", "promotion", "giảm giá", "discount", "ưu đãi", "offer",
                    "nên làm gì", "cần làm gì", "hướng dẫn", "guide", "tips", "mẹo",
                },
                ["product_suggestion"] = new[]
                {
                    "sản phẩm hot", "hot trend", "xu hướng", "trending", "sản phẩm mới", "new product",
                    "gợi ý sản phẩm", "product suggestion", "sản phẩm phổ biến", "popular product",
                    "tạp hóa", "grocery", "hàng hóa", "goods", "mặt hàng", "item",
                    "sản phẩm bán chạy", "best seller", "được yêu thích", "favorite",
                    "thị trường", "market", "hiện tại", "current", "mới nhất", "latest",
                    "nên bán", "should sell", "cần nhập", "need to import", "đề xuất", "recommend", 
                    "sản phẩm đang hot", "tìm sản phẩm", "hot", "đang hot", "tìm"
                },
                ["revenue"] = new[]
                {
                    "doanh thu", "revenue", "tiền", "bán", "thu nhập", "lợi nhuận",
                    "tổng thu", "thu về", "kiếm được", "bán được", "doanh số",
                    "hôm nay", "tuần này", "tháng này", "năm nay", "so với", "bao nhiêu"
                },
                ["customer"] = new[]
                {
                    "khách hàng", "customer", "member", "thành viên", "khách", "client",
                    "người mua", "buyer", "người dùng", "user", "tỷ lệ", "%",
                    "giới tính", "hạng", "rank", "mới", "quay lại"
                },
                ["inventory"] = new[]
                {
                    "tồn kho", "inventory", "stock", "kho", "tồn",
                    "số lượng", "quantity", "còn lại", "còn", "hết", "out of stock",
                    "sắp hết", "low stock", "cần nhập", "cảnh báo"
                },
                ["product"] = new[]
                {
                    "sản phẩm", "product", "mặt hàng", "bán chạy", "ít bán", "item",
                    "hàng hóa", "goods", "bán nhiều", "bán ít", "performance",
                    "lợi nhuận", "profit", "hiệu suất", "chậm", "nhanh",
                    "tên sản phẩm", "tên", "cụ thể", "chi tiết"
                }
            };

            public static string DetermineQuestionType(string question)
            {
                var questionLower = question.ToLower();
                var keywordMatches = new Dictionary<string, int>();
                
                // Count keyword matches for each category
                foreach (var kvp in QuestionPatterns)
                {
                    var matchCount = kvp.Value.Count(keyword => questionLower.Contains(keyword));
                    if (matchCount > 0)
                    {
                        keywordMatches[kvp.Key] = matchCount;
                    }
                }
                
                if (!keywordMatches.Any())
                    return "general";
                
                // If multiple categories have matches, use intelligent prioritization
                if (keywordMatches.Count > 1)
                {
                    return DetermineBestCategory(questionLower, keywordMatches);
                }
                
                // Single category match
                return keywordMatches.First().Key;
            }

            private static string DetermineBestCategory(string questionLower, Dictionary<string, int> matches)
            {
                // Priority rules for mixed questions
                
                // 0. Special case: If question contains "hot" or "trending", prioritize product_suggestion
                if ((questionLower.Contains("hot") || questionLower.Contains("trending") || questionLower.Contains("xu hướng")) 
                    && matches.ContainsKey("product_suggestion"))
                {
                    return "product_suggestion";
                }
                
                // 0.1. Special case: If question asks for specific product names, prioritize product analysis
                if ((questionLower.Contains("tên sản phẩm") || questionLower.Contains("tên") || questionLower.Contains("cụ thể")) 
                    && matches.ContainsKey("product"))
                {
                    return "product";
                }
                
                // 1. Strategy + Product questions -> Strategy (business planning is more important)
                if (matches.ContainsKey("strategy") && matches.ContainsKey("product_suggestion"))
                {
                    return "strategy";
                }
                
                // 2. Strategy + Revenue questions -> Strategy (strategy includes revenue planning)
                if (matches.ContainsKey("strategy") && matches.ContainsKey("revenue"))
                {
                    return "strategy";
                }
                
                // 3. Product + Revenue questions -> Product (product focus with revenue context)
                if (matches.ContainsKey("product") && matches.ContainsKey("revenue"))
                {
                    return "product";
                }
                
                // 4. Customer + Strategy questions -> Strategy (customer strategy)
                if (matches.ContainsKey("strategy") && matches.ContainsKey("customer"))
                {
                    return "strategy";
                }
                
                // 5. Product Suggestion + Inventory/Product questions -> Product Suggestion (suggestion is more specific)
                if (matches.ContainsKey("product_suggestion") && (matches.ContainsKey("inventory") || matches.ContainsKey("product")))
                {
                    return "product_suggestion";
                }
                
                // 6. Inventory + Product questions -> Inventory (inventory management focus)
                if (matches.ContainsKey("inventory") && matches.ContainsKey("product"))
                {
                    return "inventory";
                }
                
                // 7. For other combinations, choose the one with most keyword matches
                var bestMatch = matches.OrderByDescending(x => x.Value).First();
                
                // 8. If tie in keyword count, use priority order
                if (matches.Any(x => x.Value == bestMatch.Value && x.Key != bestMatch.Key))
                {
                    var priorityOrder = new[] { "strategy", "product_suggestion", "revenue", "customer", "inventory", "product" };
                    foreach (var category in priorityOrder)
                    {
                        if (matches.ContainsKey(category))
                        {
                            return category;
                        }
                    }
                }
                
                return bestMatch.Key;
            }
        }
    }
}
