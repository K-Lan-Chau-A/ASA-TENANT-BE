using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.Enums
{
    public enum LogActivityType
    {
        [Description("Đăng nhập")]
        Login = 1,

        [Description("Đăng xuất")]
        Logout = 2,

        [Description("Đổi mật khẩu")]
        ChangePassword = 3,

        [Description("Khóa / Mở khóa tài khoản")]
        LockOrUnlockUser = 4,

        [Description("Tạo mới người dùng")]
        CreateUser = 5,

        [Description("Cập nhật thông tin người dùng")]
        UpdateUser = 6,

        [Description("Tạo đơn hàng mới")]
        CreateOrder = 7,

        [Description("Áp dụng khuyến mãi / giảm giá")]
        ApplyDiscount = 8,

        [Description("Thêm sản phẩm mới")]
        AddProduct = 9,

        [Description("Cập nhật sản phẩm")]
        UpdateProduct = 10,

        [Description("Vô hiệu hoá sản phẩm")]
        DeleteProduct = 11,

        [Description("Nhập kho")]
        StockIn = 12,

        [Description("Xuất kho")]
        StockOut = 13,

        [Description("Điều chỉnh tồn kho")]
        StockAdjustment = 14,

        [Description("Mở ca")]
        OpenShift = 15,

        [Description("Đóng ca")]
        CloseShift = 16,

        [Description("Tạo báo cáo")]
        GenerateReport = 17,

        [Description("Xuất báo cáo")]
        ExportReport = 18,

        [Description("Cập nhật cấu hình hệ thống")]
        UpdateSettings = 19
    }
}
