using ASA_TENANT_REPO.DBContext;
using ASA_TENANT_REPO.Models;
using ASA_TENANT_REPO.Repository;
using ASA_TENANT_SERVICE.DTOs.Common;
using ASA_TENANT_SERVICE.DTOs.Request;
using ASA_TENANT_SERVICE.DTOs.Response;
using ASA_TENANT_SERVICE.Interface;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using System.IO;

namespace ASA_TENANT_SERVICE.Implenment
{
    public class ReportService : IReportService
    {
        private readonly ReportRepo _reportRepo;
        private readonly InventoryTransactionRepo _inventoryTransactionRepo;
        private readonly OrderRepo _orderRepo;
        private readonly CategoryRepo _categoryRepo;
        private readonly IMapper _mapper;
        private readonly ASATENANTDBContext _context;
        
        public ReportService(ReportRepo reportRepo, 
                           InventoryTransactionRepo inventoryTransactionRepo,
                           OrderRepo orderRepo,
                           CategoryRepo categoryRepo,
                           IMapper mapper, 
                           ASATENANTDBContext context)
        {
            _reportRepo = reportRepo;
            _inventoryTransactionRepo = inventoryTransactionRepo;
            _orderRepo = orderRepo;
            _categoryRepo = categoryRepo;
            _mapper = mapper;
            _context = context;
        }

        public async Task GenerateWeeklyReportAsync()
        {
            await _reportRepo.GenerateWeeklyReportAsync();

        }

        public async Task GenerateMonthlyReportAsync()
        {
            await _reportRepo.GenerateMonthlyReportAsync();
        }

        public async Task<PagedResponse<ReportResponse>> GetFilteredReportAsync(ReportGetRequest Filter, int page, int pageSize)
        {
            var filter = _mapper.Map<Report>(Filter);
            var query = _reportRepo.GetFiltered(filter);

            var totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResponse<ReportResponse>
            {
                Items = _mapper.Map<IEnumerable<ReportResponse>>(items),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        private string GetPaymentMethodText(string paymentMethod)
        {
            return paymentMethod switch
            {
                "1" => "Tiền mặt",
                "2" => "Chuyển khoản",
                "3" => "NFC", 
                "4" => "ATM",
                null => "Không xác định",
                "" => "Không xác định",
                _ => $"Phương thức {paymentMethod}"
            };
        }

        public async Task<byte[]> GenerateProfessionalRevenueReportAsync(ExcelReportRequest request)
        {
            try
            {
                // Lấy dữ liệu từ database
                var orders = await _context.Orders
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Product)
                            .ThenInclude(p => p.Category)
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.ProductUnit)
                    .Include(o => o.Customer)
                    .Include(o => o.Shift)
                        .ThenInclude(s => s.User)
                    .Include(o => o.Voucher)
                    .Include(o => o.Shop)
                    .Where(o => o.ShopId == request.ShopId && 
                               o.CreatedAt >= request.StartDate && 
                               o.CreatedAt <= request.EndDate &&
                               o.Status == 1) // Chỉ lấy đơn hàng chờ thanh toán
                    .ToListAsync();

                // Đường dẫn đến file template
                var templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "Revenue_Report_Template.xlsx");
                
                if (!File.Exists(templatePath))
                {
                    throw new FileNotFoundException($"Template file not found at: {templatePath}");
                }

                // Đọc file template
                using (var workbook = new XLWorkbook(templatePath))
                {
                    // Fill data vào các sheet có sẵn trong template
                    FillDataToTemplate(workbook, orders, request);

                    // Convert workbook to byte array
                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        stream.Position = 0;
                        return stream.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error generating professional revenue report: {ex.Message}", ex);
            }
        }

        private void FillDataToTemplate(XLWorkbook workbook, List<Order> orders, ExcelReportRequest request)
        {
            // Fill data vào sheet TỔNG QUAN (nếu có)
            var overviewSheet = workbook.Worksheets.FirstOrDefault(ws => ws.Name.Contains("TỔNG QUAN") || ws.Name.Contains("OVERVIEW"));
            if (overviewSheet != null)
            {
                FillOverviewData(overviewSheet, orders, request);
            }

            // Fill data vào sheet DOANH THU THEO NGÀY (nếu có)
            var dailySheet = workbook.Worksheets.FirstOrDefault(ws => ws.Name.Contains("DOANH THU THEO NGÀY") || ws.Name.Contains("DAILY"));
            if (dailySheet != null)
            {
                FillDailyRevenueData(dailySheet, orders);
            }

            // Fill data vào sheet DOANH THU THEO SẢN PHẨM (nếu có)
            var productSheet = workbook.Worksheets.FirstOrDefault(ws => ws.Name.Contains("DOANH THU THEO SẢN PHẨM") || ws.Name.Contains("PRODUCT"));
            if (productSheet != null)
            {
                FillProductRevenueData(productSheet, orders);
            }

            // Fill data vào sheet DOANH THU THEO DANH MỤC (nếu có)
            var categorySheet = workbook.Worksheets.FirstOrDefault(ws => ws.Name.Contains("DOANH THU THEO DANH MỤC") || ws.Name.Contains("CATEGORY"));
            if (categorySheet != null)
            {
                FillCategoryRevenueData(categorySheet, orders);
            }

            // Fill data vào sheet PHÂN TÍCH KHÁCH HÀNG (nếu có)
            var customerSheet = workbook.Worksheets.FirstOrDefault(ws => ws.Name.Contains("PHÂN TÍCH KHÁCH HÀNG") || ws.Name.Contains("CUSTOMER"));
            if (customerSheet != null)
            {
                FillCustomerAnalysisData(customerSheet, orders);
            }
        }

        private void FillOverviewData(IXLWorksheet sheet, List<Order> orders, ExcelReportRequest request)
        {
            // Tính toán tổng quan
            var totalRevenue = orders.Sum(o => o.TotalPrice ?? 0);
            var totalOrders = orders.Count;
            var totalProducts = orders.SelectMany(o => o.OrderDetails).Sum(od => od.Quantity ?? 0);
            var averageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

            // Fill thông tin cửa hàng và thời gian
            sheet.Cell("B3").Value = orders.FirstOrDefault()?.Shop?.ShopName ?? "N/A"; // Cửa hàng
            sheet.Cell("B4").Value = $"{request.StartDate:dd/MM/yyyy} - {request.EndDate:dd/MM/yyyy}"; // Thời gian báo cáo
            sheet.Cell("B5").Value = DateTime.Now.ToString("dd/MM/yyyy HH:mm"); // Ngày tạo báo cáo

            // Fill tổng quan doanh thu - điền trực tiếp vào các cell cụ thể
            sheet.Cell("B9").Value = totalRevenue; // Tổng doanh thu
            sheet.Cell("B9").Style.NumberFormat.Format = "#,##0";
            
            sheet.Cell("B10").Value = totalOrders; // Số đơn hàng
            
            sheet.Cell("B11").Value = totalProducts; // Số sản phẩm bán
            
            sheet.Cell("B12").Value = averageOrderValue; // Giá trị đơn hàng trung bình
            sheet.Cell("B12").Style.NumberFormat.Format = "#,##0";

            // Fill phương thức thanh toán
            var paymentMethods = orders
                .GroupBy(o => GetPaymentMethodText(o.PaymentMethod))
                .Select(g => new { 
                    Method = g.Key, 
                    Revenue = g.Sum(o => o.TotalPrice ?? 0)
                })
                .OrderByDescending(x => x.Revenue)
                .ToList();

            // Điền data vào bảng phương thức thanh toán (bắt đầu từ dòng 17)
            for (int i = 0; i < paymentMethods.Count && i < 5; i++)
            {
                var row = 17 + i;
                var percentage = totalRevenue > 0 ? (paymentMethods[i].Revenue / totalRevenue) : 0;
                
                // Định dạng dòng data
                var dataRowRange = sheet.Range($"A{row}:C{row}");
                dataRowRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                dataRowRange.Style.Fill.BackgroundColor = row % 2 == 1 ? XLColor.White : XLColor.LightGray;

                sheet.Cell($"A{row}").Value = paymentMethods[i].Method;
                sheet.Cell($"B{row}").Value = paymentMethods[i].Revenue;
                sheet.Cell($"B{row}").Style.NumberFormat.Format = "#,##0";
                sheet.Cell($"C{row}").Value = percentage;
                sheet.Cell($"C{row}").Style.NumberFormat.Format = "0.00%";
            }
        }

        private void FillDailyRevenueData(IXLWorksheet sheet, List<Order> orders)
        {
            var startRow = FindDataStartRow(sheet, "Ngày");
            if (startRow == -1) return;

            var dailyRevenue = orders
                .Where(o => o.CreatedAt.HasValue)
                .GroupBy(o => o.CreatedAt.Value.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    OrderCount = g.Count(),
                    Revenue = g.Sum(o => o.TotalPrice ?? 0),
                    ProductCount = g.SelectMany(o => o.OrderDetails).Sum(od => od.Quantity ?? 0),
                    AveragePerOrder = g.Count() > 0 ? g.Sum(o => o.TotalPrice ?? 0) / g.Count() : 0
                })
                .OrderBy(x => x.Date)
                .ToList();

            int row = startRow;
            foreach (var daily in dailyRevenue)
            {
                // Định dạng dòng data
                var dataRowRange = sheet.Range($"A{row}:E{row}");
                dataRowRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                dataRowRange.Style.Fill.BackgroundColor = row % 2 == 1 ? XLColor.White : XLColor.LightBlue;

                sheet.Cell($"A{row}").Value = daily.Date.ToString("dd/MM/yyyy");
                sheet.Cell($"B{row}").Value = daily.OrderCount;
                sheet.Cell($"C{row}").Value = daily.Revenue;
                sheet.Cell($"C{row}").Style.NumberFormat.Format = "#,##0";
                sheet.Cell($"D{row}").Value = daily.AveragePerOrder;
                sheet.Cell($"D{row}").Style.NumberFormat.Format = "#,##0";
                sheet.Cell($"E{row}").Value = daily.ProductCount;
                row++;
            }

            // Tính tổng cộng
            var totalRow = row;
            var totalOrders = dailyRevenue.Sum(d => d.OrderCount);
            var totalRevenue = dailyRevenue.Sum(d => d.Revenue);
            var totalProducts = dailyRevenue.Sum(d => d.ProductCount);
            var totalAverage = totalOrders > 0 ? totalRevenue / totalOrders : 0;

            // Định dạng dòng tổng cộng
            var totalRowRange = sheet.Range($"A{totalRow}:E{totalRow}");
            totalRowRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            totalRowRange.Style.Fill.BackgroundColor = XLColor.FromArgb(0xffffe0);
            totalRowRange.Style.Font.Bold = true;

            sheet.Cell($"A{totalRow}").Value = "TỔNG CỘNG";
            sheet.Cell($"B{totalRow}").Value = totalOrders;
            sheet.Cell($"C{totalRow}").Value = totalRevenue;
            sheet.Cell($"C{totalRow}").Style.NumberFormat.Format = "#,##0";
            sheet.Cell($"D{totalRow}").Value = totalAverage;
            sheet.Cell($"D{totalRow}").Style.NumberFormat.Format = "#,##0";
            sheet.Cell($"E{totalRow}").Value = totalProducts;
        }

        private void FillProductRevenueData(IXLWorksheet sheet, List<Order> orders)
        {
            var startRow = FindDataStartRow(sheet, "Tên sản phẩm");
            if (startRow == -1) return;

            var productRevenue = orders
                .SelectMany(o => o.OrderDetails)
                .Where(od => od.Product != null)
                .GroupBy(od => od.Product.ProductId)
                .Select(g => new
                {
                    Product = g.First().Product,
                    Quantity = g.Sum(od => od.Quantity ?? 0),
                    Revenue = g.Sum(od => od.TotalPrice ?? 0),
                    AveragePrice = g.Sum(od => od.Quantity ?? 0) > 0 ? g.Sum(od => od.TotalPrice ?? 0) / g.Sum(od => od.Quantity ?? 0) : 0
                })
                .OrderByDescending(x => x.Revenue)
                .ToList();

            int row = startRow;
            int stt = 1;
            foreach (var product in productRevenue)
            {
                // Định dạng dòng data
                var dataRowRange = sheet.Range($"A{row}:G{row}");
                dataRowRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                dataRowRange.Style.Fill.BackgroundColor = row % 2 == 1 ? XLColor.White : XLColor.LightYellow;

                sheet.Cell($"A{row}").Value = stt++; // STT
                sheet.Cell($"A{row}").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                sheet.Cell($"B{row}").Value = product.Product.ProductName; // Tên sản phẩm
                sheet.Cell($"C{row}").Value = product.Product.Barcode; // SKU
                sheet.Cell($"D{row}").Value = product.Product.Category?.CategoryName ?? "Không xác định"; // Danh mục
                sheet.Cell($"E{row}").Value = product.Quantity; // Số lượng bán
                sheet.Cell($"E{row}").Style.NumberFormat.Format = "#,##0";
                sheet.Cell($"F{row}").Value = product.Revenue; // Doanh thu
                sheet.Cell($"F{row}").Style.NumberFormat.Format = "#,##0";
                sheet.Cell($"G{row}").Value = product.AveragePrice; // Giá trung bình
                sheet.Cell($"G{row}").Style.NumberFormat.Format = "#,##0";
                row++;
            }
        }

        private void FillCategoryRevenueData(IXLWorksheet sheet, List<Order> orders)
        {
            var startRow = FindDataStartRow(sheet, "Danh mục");
            if (startRow == -1) return;

            var categoryRevenue = orders
                .SelectMany(o => o.OrderDetails)
                .Where(od => od.Product?.Category != null)
                .GroupBy(od => od.Product.Category.CategoryId)
                .Select(g => new
                {
                    Category = g.First().Product.Category,
                    ProductCount = g.Select(od => od.Product.ProductId).Distinct().Count(),
                    Quantity = g.Sum(od => od.Quantity ?? 0),
                    Revenue = g.Sum(od => od.TotalPrice ?? 0)
                })
                .OrderByDescending(x => x.Revenue)
                .ToList();

            var totalRevenue = categoryRevenue.Sum(c => c.Revenue);
            int row = startRow;
            foreach (var category in categoryRevenue)
            {
                var percentage = totalRevenue > 0 ? (category.Revenue / totalRevenue) : 0;
                
                // Định dạng dòng data
                var dataRowRange = sheet.Range($"A{row}:E{row}");
                dataRowRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                dataRowRange.Style.Fill.BackgroundColor = row % 2 == 1 ? XLColor.White : XLColor.Lavender;

                sheet.Cell($"A{row}").Value = category.Category.CategoryName;
                sheet.Cell($"B{row}").Value = category.ProductCount;
                sheet.Cell($"C{row}").Value = category.Quantity;
                sheet.Cell($"D{row}").Value = category.Revenue;
                sheet.Cell($"D{row}").Style.NumberFormat.Format = "#,##0";
                sheet.Cell($"E{row}").Value = percentage;
                sheet.Cell($"E{row}").Style.NumberFormat.Format = "0.00%";
                row++;
            }
        }

        private void FillCustomerAnalysisData(IXLWorksheet sheet, List<Order> orders)
        {
            // Fill thống kê tổng quan khách hàng
            var totalCustomers = orders.Select(o => o.CustomerId).Distinct().Count();
            var totalOrders = orders.Count;
            var avgOrdersPerCustomer = totalCustomers > 0 ? (double)totalOrders / totalCustomers : 0;

            // Tìm và fill thống kê tổng quan
            for (int searchRow = 1; searchRow <= 20; searchRow++)
            {
                var cellValue = sheet.Cell($"A{searchRow}").Value.ToString();
                if (cellValue.Contains("Tổng số khách hàng"))
                {
                    sheet.Cell($"B{searchRow}").Value = totalCustomers;
                }
                else if (cellValue.Contains("Tổng số đơn hàng"))
                {
                    sheet.Cell($"B{searchRow}").Value = totalOrders;
                }
                else if (cellValue.Contains("Trung bình đơn hàng/khách"))
                {
                    sheet.Cell($"B{searchRow}").Value = avgOrdersPerCustomer;
                    sheet.Cell($"B{searchRow}").Style.NumberFormat.Format = "0.00";
                }
            }

            // Fill top khách hàng
            var startRow = FindDataStartRow(sheet, "STT");
            if (startRow == -1) return;

            var topCustomers = orders
                .Where(o => o.CustomerId != null)
                .GroupBy(o => o.CustomerId)
                .Select(g => new
                {
                    CustomerId = g.Key,
                    Customer = g.First().Customer,
                    OrderCount = g.Count(),
                    TotalRevenue = g.Sum(o => o.TotalPrice ?? 0)
                })
                .OrderByDescending(x => x.TotalRevenue)
                .Take(10)
                .ToList();

            int row = startRow;
            int stt = 1;
            foreach (var customer in topCustomers)
            {
                var avgOrderValue = customer.OrderCount > 0 ? customer.TotalRevenue / customer.OrderCount : 0;
                
                // Định dạng dòng data
                var dataRowRange = sheet.Range($"A{row}:F{row}");
                dataRowRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                dataRowRange.Style.Fill.BackgroundColor = row % 2 == 1 ? XLColor.White : XLColor.LightGreen;

                sheet.Cell($"A{row}").Value = stt++;
                sheet.Cell($"B{row}").Value = customer.CustomerId;
                sheet.Cell($"C{row}").Value = customer.Customer?.FullName ?? "Khách vãng lai";
                sheet.Cell($"D{row}").Value = customer.OrderCount;
                sheet.Cell($"E{row}").Value = customer.TotalRevenue;
                sheet.Cell($"E{row}").Style.NumberFormat.Format = "#,##0";
                sheet.Cell($"F{row}").Value = avgOrderValue;
                sheet.Cell($"F{row}").Style.NumberFormat.Format = "#,##0";
                row++;
            }
        }

        private int FindDataStartRow(IXLWorksheet sheet, string searchText)
        {
            // Tìm dòng chứa text để xác định vị trí bắt đầu data
            for (int row = 1; row <= 50; row++)
            {
                for (int col = 1; col <= 10; col++)
                {
                    var cellValue = sheet.Cell(row, col).Value.ToString();
                    if (!string.IsNullOrEmpty(cellValue) && cellValue.Contains(searchText))
                    {
                        return row + 1; // Dòng tiếp theo là dòng data đầu tiên
                    }
                }
            }
            return -1; // Không tìm thấy
        }

        public async Task<StatisticsOverviewResponse> GetStatisticsOverviewAsync(long shopId)
        {
            try
            {
                // Lấy top 10 sản phẩm bán chạy từ inventory transactions
                var sellingTransactions = await _inventoryTransactionRepo.GetSellingTransactionsAsync(shopId);
                
                // Kiểm tra null
                if (sellingTransactions == null)
                {
                    sellingTransactions = new List<InventoryTransaction>();
                }
                
                
                var topProducts = sellingTransactions
                    .Where(it => it != null && it.ProductId.HasValue && it.Product != null)
                    .GroupBy(it => it.ProductId)
                    .Select(g => new TopProductResponse
                    {
                        ProductId = (long)g.Key,
                        ProductName = g.First()?.Product?.ProductName ?? "Unknown",
                        Barcode = g.First()?.Product?.Barcode ?? "",
                        CategoryName = g.First()?.Product?.Category?.CategoryName ?? "Unknown",
                        TotalQuantitySold = g.Sum(it => it?.Quantity ?? 0),
                        TotalRevenue = g.Sum(it => (it?.Price ?? 0) * (it?.Quantity ?? 0)),
                        AveragePrice = g.Average(it => it?.Price ?? 0),
                        ImageUrl = g.First()?.Product?.ImageUrl ?? ""
                    })
                    .Where(p => p.TotalQuantitySold > 0)
                    .OrderByDescending(x => x.TotalQuantitySold)
                    .Take(10)
                    .ToList();

                // Lấy thống kê doanh thu 7 ngày trước
                // Từ 00:00:00 của ngày 7 trước đến 23:59:59 của ngày hôm nay
                var endDate = DateTime.UtcNow.Date.AddDays(1).AddSeconds(-1); // 23:59:59 của ngày hôm nay
                var startDate = DateTime.UtcNow.Date.AddDays(-6); // 00:00:00 của ngày 7 trước
                
                // Lấy orders đã thanh toán trong 7 ngày (status = 1) với OrderDetails
                var allOrdersLast7Days = await _orderRepo.GetFiltered(new Order
                {
                    ShopId = shopId,
                    Status = 1 // Đã thanh toán
                })
                .Include(o => o.OrderDetails)
                .Where(o => o.CreatedAt.HasValue && 
                           o.CreatedAt.Value >= startDate && 
                           o.CreatedAt.Value <= endDate)
                .ToListAsync();

                // Kiểm tra null
                if (allOrdersLast7Days == null)
                {
                    allOrdersLast7Days = new List<Order>();
                }

                // Tính thống kê từ orders đã thanh toán
                var totalRevenue = allOrdersLast7Days.Sum(o => o?.TotalPrice ?? 0);
                var totalOrders = allOrdersLast7Days.Count;
                
                // Debug: Kiểm tra OrderDetails
                var orderDetailsList = allOrdersLast7Days
                    .Where(o => o?.OrderDetails != null)
                    .SelectMany(o => o.OrderDetails)
                    .ToList();
                
                var totalProductsSold = orderDetailsList.Sum(od => od?.Quantity ?? 0);
                var averageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

                // Tạo danh sách 7 ngày đầy đủ từ 7 ngày trước đến hôm nay
                var allDays = new List<DateTime>();
                for (int i = 6; i >= 0; i--)
                {
                    allDays.Add(DateTime.UtcNow.Date.AddDays(-i));
                }

                // Nhóm orders theo ngày
                var ordersByDate = allOrdersLast7Days
                    .Where(o => o != null)
                    .GroupBy(o => o.CreatedAt?.Date)
                    .ToDictionary(g => g.Key ?? DateTime.MinValue, g => g.ToList());

                // Tạo dailyRevenues với đủ 7 ngày
                var dailyRevenues = allDays.Select(date => 
                {
                    var ordersForDate = ordersByDate.ContainsKey(date) ? ordersByDate[date] : new List<Order>();
                    
                    return new DailyRevenueResponse
                    {
                        Date = DateTime.SpecifyKind(date, DateTimeKind.Utc),
                        Revenue = ordersForDate.Sum(o => o?.TotalPrice ?? 0),
                        OrderCount = ordersForDate.Count,
                        ProductCount = ordersForDate
                            .Where(o => o?.OrderDetails != null)
                            .SelectMany(o => o.OrderDetails)
                            .Sum(od => od?.Quantity ?? 0)
                    };
                })
                .OrderBy(x => x.Date)
                .ToList();

                var revenueStats = new RevenueStatsResponse
                {
                    StartDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc),
                    EndDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc),
                    TotalRevenue = totalRevenue,
                    TotalOrders = totalOrders,
                    TotalProductsSold = totalProductsSold,
                    AverageOrderValue = averageOrderValue,
                    DailyRevenues = dailyRevenues
                };

                // Lấy top categories bán chạy từ orders trong 7 ngày
                var categoriesWithOrders = await _categoryRepo.GetCategoriesWithOrdersAsync(shopId);
                
                // Kiểm tra null
                if (categoriesWithOrders == null)
                {
                    categoriesWithOrders = new List<Category>();
                }
                
                var topCategories = categoriesWithOrders
                    .Where(c => c != null && c.Products != null)
                    .Select(c => new
                    {
                        CategoryId = c.CategoryId,
                        CategoryName = c.CategoryName ?? "Unknown",
                        ProductCount = c.Products?.Count() ?? 0,
                        TotalQuantitySold = c.Products?
                            .Where(p => p?.OrderDetails != null)
                            .SelectMany(p => p.OrderDetails)
                            .Where(od => od?.Order != null && 
                                        od.Order.Status == 1 && // Đã thanh toán
                                        od.Order.CreatedAt.HasValue && 
                                        od.Order.CreatedAt.Value.Date >= startDate && 
                                        od.Order.CreatedAt.Value.Date <= endDate)
                            .Sum(od => od?.Quantity ?? 0) ?? 0,
                        TotalRevenue = c.Products?
                            .Where(p => p?.OrderDetails != null)
                            .SelectMany(p => p.OrderDetails)
                            .Where(od => od?.Order != null && 
                                        od.Order.Status == 1 && // Đã thanh toán
                                        od.Order.CreatedAt.HasValue && 
                                        od.Order.CreatedAt.Value.Date >= startDate && 
                                        od.Order.CreatedAt.Value.Date <= endDate)
                            .Sum(od => od?.TotalPrice ?? 0) ?? 0
                    })
                    .Where(c => c.TotalQuantitySold > 0)
                    .OrderByDescending(c => c.TotalRevenue)
                    .Take(10)
                    .ToList();

                var totalCategoryRevenue = topCategories.Sum(c => c.TotalRevenue);
                
                var topCategoriesResponse = topCategories.Select(c => new TopCategoryResponse
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName,
                    ProductCount = c.ProductCount,
                    TotalQuantitySold = c.TotalQuantitySold,
                    TotalRevenue = c.TotalRevenue,
                    PercentageOfTotal = totalCategoryRevenue > 0 ? c.TotalRevenue / totalCategoryRevenue * 100 : 0
                }).ToList();

                return new StatisticsOverviewResponse
                {
                    TopProducts = topProducts,
                    RevenueStats = revenueStats,
                    TopCategories = topCategoriesResponse
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting statistics overview: {ex.Message}", ex);
            }
        }
    }
}
