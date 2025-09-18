using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.Enums
{
    public enum TransactionReturnCode
    {
        [Description("Thành công")]
        Success = 0,

        [Description("Thất bại")]
        Failed = 1
    }
    public enum TransactionPaymentStatus
    {
        [Description("Chờ")]
        Pending = 1,
        [Description("Thành công")]
        Success = 2,
        [Description("Thất bại")]
        Failed = 3
    }
}
