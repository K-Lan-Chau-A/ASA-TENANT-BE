using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.Enums
{
    public enum OrderStatus
    {
        [Description("Chờ thanh toán")]
        Pending = 0,

        [Description("Đã thanh toán")]
        Paid = 1,

        [Description("Đã hủy")]
        Cancelled = 2
    }
    public enum PaymentMethodEnum
    {
        [Description("Tiền mặt")]
        Cash = 1,

        [Description("Chuyển khoản")]
        BankTransfer = 2,

        [Description("NFC")]
        NFC = 3
    }
}
