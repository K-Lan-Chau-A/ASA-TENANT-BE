namespace ASA_TENANT_SERVICE.Templates
{
    public static class ChatbotPrompts
    {
        public static class Keywords
        {
            public static readonly string[] RevenueKeywords = new[]
            {
                "doanh thu", "revenue", "tiền", "bán", "thu nhập", "lợi nhuận", "profit",
                "tổng thu", "thu về", "kiếm được", "bán được", "doanh số"
            };

            public static readonly string[] CustomerKeywords = new[]
            {
                "khách hàng", "customer", "member", "thành viên", "khách", "client",
                "người mua", "buyer", "người dùng", "user"
            };

            public static readonly string[] InventoryKeywords = new[]
            {
                "tồn kho", "inventory", "hàng", "sản phẩm", "stock", "kho", "tồn",
                "số lượng", "quantity", "còn lại", "còn", "hết", "out of stock"
            };

            public static readonly string[] ProductKeywords = new[]
            {
                "sản phẩm", "product", "mặt hàng", "bán chạy", "ít bán", "item",
                "hàng hóa", "goods", "bán nhiều", "bán ít", "performance"
            };

            public static readonly string[] TimeKeywords = new[]
            {
                "hôm nay", "today", "tuần này", "this week", "tháng này", "this month",
                "tuần trước", "last week", "tháng trước", "last month", "năm nay", "this year"
            };

            public static readonly string[] ComparisonKeywords = new[]
            {
                "so với", "compared to", "tăng", "increase", "giảm", "decrease",
                "nhiều hơn", "more than", "ít hơn", "less than", "bằng", "equal"
            };
        }

        public static class ResponseTemplates
        {
            public static readonly Dictionary<string, string[]> RevenueResponses = new()
            {
                ["today"] = new[]
                {
                    "Doanh thu hôm nay của cửa hàng là {TodayRevenue:N0} VNĐ từ {TodayOrders} đơn hàng.",
                    "Hôm nay cửa hàng đã thu về {TodayRevenue:N0} VNĐ với {TodayOrders} giao dịch.",
                    "Tổng doanh thu trong ngày: {TodayRevenue:N0} VNĐ ({TodayOrders} đơn hàng)."
                },
                ["week"] = new[]
                {
                    "Doanh thu tuần này của cửa hàng là {ThisWeekRevenue:N0} VNĐ từ {ThisWeekOrders} đơn hàng.",
                    "Tuần này cửa hàng đã thu về {ThisWeekRevenue:N0} VNĐ với {ThisWeekOrders} giao dịch.",
                    "Tổng doanh thu trong tuần: {ThisWeekRevenue:N0} VNĐ ({ThisWeekOrders} đơn hàng)."
                },
                ["month"] = new[]
                {
                    "Doanh thu tháng này của cửa hàng là {ThisMonthRevenue:N0} VNĐ từ {ThisMonthOrders} đơn hàng. So với tháng trước, doanh thu {GrowthText}.",
                    "Tháng này cửa hàng đã thu về {ThisMonthRevenue:N0} VNĐ với {ThisMonthOrders} giao dịch. {GrowthText} so với tháng trước.",
                    "Tổng doanh thu trong tháng: {ThisMonthRevenue:N0} VNĐ ({ThisMonthOrders} đơn hàng). {GrowthText}."
                },
                ["total"] = new[]
                {
                    "Tổng doanh thu của cửa hàng là {TotalRevenue:N0} VNĐ từ {TotalOrders} đơn hàng. Trung bình mỗi đơn hàng có giá trị {AverageOrderValue:N0} VNĐ.",
                    "Cửa hàng đã thu về tổng cộng {TotalRevenue:N0} VNĐ với {TotalOrders} đơn hàng. Giá trị trung bình mỗi đơn hàng là {AverageOrderValue:N0} VNĐ.",
                    "Tổng doanh thu: {TotalRevenue:N0} VNĐ ({TotalOrders} đơn hàng). AOV: {AverageOrderValue:N0} VNĐ."
                }
            };

            public static readonly Dictionary<string, string[]> CustomerResponses = new()
            {
                ["membership"] = new[]
                {
                    "Cửa hàng có {TotalCustomers} khách hàng tổng cộng. Trong đó {MemberPercentage:F1}% ({MemberCustomers} khách) là thành viên, còn lại {NonMemberPercentage:F1}% ({NonMemberCustomers} khách) chưa tạo tài khoản thành viên.",
                    "Tỷ lệ thành viên: {MemberPercentage:F1}% ({MemberCustomers}/{TotalCustomers} khách). {NonMemberPercentage:F1}% khách hàng chưa đăng ký thành viên.",
                    "Khách hàng thành viên: {MemberCustomers} người ({MemberPercentage:F1}%), khách vãng lai: {NonMemberCustomers} người ({NonMemberPercentage:F1}%)."
                },
                ["demographics"] = new[]
                {
                    "Khách hàng chủ yếu của cửa hàng: {TopRank} ({TopRankCount} khách), {TopGender} ({TopGenderCount} khách). Trung bình mỗi khách hàng đã chi tiêu {AverageCustomerSpent:N0} VNĐ.",
                    "Phân bố khách hàng: {TopRank} chiếm {TopRankCount} người, {TopGender} chiếm {TopGenderCount} người. Chi tiêu trung bình: {AverageCustomerSpent:N0} VNĐ.",
                    "Nhóm khách hàng chính: {TopRank} và {TopGender}. Mức chi tiêu trung bình: {AverageCustomerSpent:N0} VNĐ."
                },
                ["new_customers"] = new[]
                {
                    "Tháng này có {NewCustomersThisMonth} khách hàng mới đăng ký. Tổng cộng có {ReturningCustomers} khách hàng đã từng mua hàng.",
                    "Khách hàng mới tháng này: {NewCustomersThisMonth} người. Khách hàng quay lại: {ReturningCustomers} người.",
                    "Tăng trưởng khách hàng: +{NewCustomersThisMonth} khách mới, {ReturningCustomers} khách thân thiết."
                }
            };

            public static readonly Dictionary<string, string[]> InventoryResponses = new()
            {
                ["overview"] = new[]
                {
                    "Cửa hàng có {TotalProducts} sản phẩm trong kho với tổng giá trị {TotalInventoryValue:N0} VNĐ. Trong đó {InStockProducts} sản phẩm còn hàng, {LowStockProducts} sản phẩm sắp hết hàng, và {OutOfStockProducts} sản phẩm đã hết hàng.",
                    "Tình hình tồn kho: {TotalProducts} sản phẩm (giá trị {TotalInventoryValue:N0} VNĐ). Còn hàng: {InStockProducts}, sắp hết: {LowStockProducts}, hết hàng: {OutOfStockProducts}.",
                    "Tồn kho hiện tại: {TotalProducts} mặt hàng, giá trị {TotalInventoryValue:N0} VNĐ. {InStockProducts} còn hàng, {LowStockProducts} cần nhập, {OutOfStockProducts} hết hàng."
                },
                ["low_stock"] = new[]
                {
                    "Các sản phẩm sắp hết hàng cần nhập thêm: {LowStockItems}. Tổng cộng có {LowStockProducts} sản phẩm cần chú ý.",
                    "Sản phẩm cần nhập hàng: {LowStockItems}. {LowStockProducts} sản phẩm đang ở mức thấp.",
                    "Cảnh báo tồn kho: {LowStockItems} sắp hết. {LowStockProducts} sản phẩm cần bổ sung."
                }
            };

            public static readonly Dictionary<string, string[]> ProductResponses = new()
            {
                ["top_selling"] = new[]
                {
                    "Sản phẩm bán chạy nhất: {TopProducts}.",
                    "Top sản phẩm bán chạy: {TopProducts}.",
                    "Mặt hàng hot: {TopProducts}."
                },
                ["worst_selling"] = new[]
                {
                    "Sản phẩm bán chậm: {WorstProducts}.",
                    "Mặt hàng ít bán: {WorstProducts}.",
                    "Sản phẩm cần chú ý: {WorstProducts}."
                },
                ["profitable"] = new[]
                {
                    "Sản phẩm có lợi nhuận cao nhất: {ProfitableProducts}.",
                    "Mặt hàng sinh lời tốt: {ProfitableProducts}.",
                    "Top lợi nhuận: {ProfitableProducts}."
                }
            };

            public static readonly Dictionary<string, string[]> GeneralResponses = new()
            {
                ["overview"] = new[]
                {
                    "Tổng quan về cửa hàng {ShopName}: Có {TotalProducts} sản phẩm, {TotalCustomers} khách hàng, đã thực hiện {TotalOrders} đơn hàng với tổng doanh thu {TotalRevenue:N0} VNĐ. Trung bình mỗi đơn hàng có giá trị {AverageOrderValue:N0} VNĐ.",
                    "Báo cáo tổng quan cửa hàng {ShopName}: {TotalProducts} mặt hàng, {TotalCustomers} khách hàng, {TotalOrders} đơn hàng, doanh thu {TotalRevenue:N0} VNĐ, AOV {AverageOrderValue:N0} VNĐ.",
                    "Thống kê cửa hàng {ShopName}: {TotalProducts} sản phẩm, {TotalCustomers} khách, {TotalOrders} giao dịch, thu nhập {TotalRevenue:N0} VNĐ, giá trị TB/đơn {AverageOrderValue:N0} VNĐ."
                }
            };
        }

        public static class ErrorMessages
        {
            public static readonly string[] GeneralErrors = new[]
            {
                "Xin lỗi, tôi chưa hiểu rõ câu hỏi của bạn. Bạn có thể hỏi về doanh thu, khách hàng, tồn kho, hoặc sản phẩm.",
                "Tôi chưa thể trả lời câu hỏi này. Hãy thử hỏi về tình hình kinh doanh của cửa hàng.",
                "Câu hỏi này nằm ngoài khả năng của tôi. Tôi có thể giúp bạn về doanh thu, khách hàng, tồn kho, và sản phẩm.",
                "Xin lỗi, tôi không hiểu. Bạn có thể hỏi cụ thể hơn về doanh thu, khách hàng, tồn kho, hoặc hiệu suất sản phẩm."
            };

            public static readonly string[] DataErrors = new[]
            {
                "Hiện tại chưa có đủ dữ liệu để trả lời câu hỏi này.",
                "Dữ liệu về vấn đề này chưa được cập nhật.",
                "Chưa có thông tin để phân tích câu hỏi của bạn.",
                "Dữ liệu này chưa có sẵn trong hệ thống."
            };

            public static readonly string[] SystemErrors = new[]
            {
                "Xin lỗi, tôi gặp sự cố khi xử lý câu hỏi của bạn. Vui lòng thử lại sau.",
                "Có lỗi xảy ra trong quá trình xử lý. Hãy thử lại trong vài phút.",
                "Hệ thống đang gặp sự cố. Vui lòng thử lại sau.",
                "Tôi không thể trả lời ngay bây giờ. Vui lòng thử lại sau."
            };
        }

        public static class SampleQuestions
        {
            public static readonly Dictionary<string, string[]> Questions = new()
            {
                ["Revenue"] = new[]
                {
                    "Doanh thu hôm nay của cửa hàng là bao nhiêu?",
                    "Doanh thu tuần này so với tuần trước như thế nào?",
                    "Doanh thu tháng này tăng hay giảm so với tháng trước?",
                    "Trung bình mỗi đơn hàng có giá trị bao nhiêu?",
                    "Phương thức thanh toán nào được sử dụng nhiều nhất?",
                    "Doanh thu theo từng danh mục sản phẩm ra sao?"
                },
                ["Customer"] = new[]
                {
                    "Bao nhiêu % khách hàng đã tạo tài khoản thành viên?",
                    "Khách hàng chủ yếu của cửa hàng là ai?",
                    "Có bao nhiêu khách hàng mới trong tháng này?",
                    "Khách hàng nào chi tiêu nhiều nhất?",
                    "Phân bố khách hàng theo giới tính như thế nào?",
                    "Khách hàng nào mua hàng thường xuyên nhất?"
                },
                ["Inventory"] = new[]
                {
                    "Tình hình tồn kho hiện tại ra sao?",
                    "Sản phẩm nào sắp hết hàng cần nhập thêm?",
                    "Giá trị tồn kho hiện tại là bao nhiêu?",
                    "Sản phẩm nào đã hết hàng?",
                    "Danh mục nào có nhiều sản phẩm nhất?",
                    "Có bao nhiêu sản phẩm đang ở mức cảnh báo?"
                },
                ["Product"] = new[]
                {
                    "Sản phẩm nào bán chạy nhất?",
                    "Sản phẩm nào bán chậm cần chú ý?",
                    "Sản phẩm nào có lợi nhuận cao nhất?",
                    "Những sản phẩm nào cần được chú ý?",
                    "Hiệu suất bán hàng theo từng danh mục?",
                    "Sản phẩm nào đang có vấn đề về tồn kho?"
                },
                ["General"] = new[]
                {
                    "Tổng quan về tình hình kinh doanh của cửa hàng?",
                    "Cửa hàng có bao nhiêu sản phẩm và khách hàng?",
                    "Thống kê tổng quan về cửa hàng",
                    "Báo cáo tình hình kinh doanh hiện tại",
                    "Tóm tắt hoạt động kinh doanh của cửa hàng"
                }
            };
        }
    }
}
